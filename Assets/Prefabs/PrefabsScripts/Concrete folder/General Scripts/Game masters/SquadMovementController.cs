using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.PlayerLoop.PreUpdate;

public interface ICoroutineAction
{
    IEnumerator Execute();
}

public class MovementController
{
    private Action<GameObject> updateSquadsAction;

    private Dictionary<AbstractSquad,Action<AbstractSquad,GameObject>> squadsNotification = new();

    public void Start()
    {
        updateSquadsAction = ServiceRegistry.WorkWithController<CellUIScript>().UpdatePanelInfo;
    }

    public ICommand AddMovementForSquad(GameObject startCell, GameObject finishCell, AbstractSquad squad, bool UI_update = true)
    {
        if (squad.SquadAction != SquadActions.None) { return null; }

        if (UI_update)
        {
            ServiceRegistry.WorkWithController<GameController>().UpdateSquadInformation_SwipeState(squad);
            updateSquadsAction.Invoke(startCell);
        }

        SquadMovement newMovement = new SquadMovement(ServiceRegistry.WorkWithController<AAlgorithm>().CreateWay(startCell, finishCell,squad.Side), squad);

        newMovement.squadEndMovement += (AbstractSquad squad, GameObject cell) =>
        {
            if (squadsNotification.ContainsKey(squad)) { squadsNotification[squad].Invoke(squad, cell); squadsNotification.Remove(squad); }
        };

        ServiceRegistry.WorkWithService<CommandBus>().Enqueue(newMovement);

        return newMovement;
    }

    public ICommand AddMovementWithWay(List<GameObject> way, AbstractSquad squad, bool UI_update = true)
    {
        if (UI_update)
        {
            ServiceRegistry.WorkWithController<GameController>().UpdateSquadInformation_SwipeState(squad);
            updateSquadsAction.Invoke(way[0]);
        }

        SquadMovement newMovement = new SquadMovement(way, squad);

        newMovement.squadEndMovement += (AbstractSquad squad, GameObject cell) =>
        {
            if (squadsNotification.ContainsKey(squad)) { squadsNotification[squad].Invoke(squad, cell); squadsNotification.Remove(squad); }
        };

        ServiceRegistry.WorkWithService<CommandBus>().Enqueue(newMovement, CommandPriority.High);
        CustomLog.PurpleText($"Pushing movement command {squad.Name}");

        return newMovement;
    }

    public void AddEvent(AbstractSquad squad, Action<AbstractSquad,GameObject> action)
    {
        CustomLog.RedText_Warning($"Error for {squad.Name} || {squad.Attack} {squad.Protection} {squad.Speed}");
        squadsNotification.Add(squad, action);
    }
}
public class SquadMovement:ICommand
{
    public Guid Id { get; }
    public CommandState State { get; private set; } = CommandState.Created;



    private List<GameObject> wayCells;
    private AbstractSquad squad;

    public event Action<AbstractSquad,GameObject> squadEndMovement;

    private ArrowCanvasWorker arrowCanvasWorker;


    public void Prepare()
    {
        arrowCanvasWorker = new ArrowCanvasWorker(squad);

        squad.SquadAction = SquadActions.Moving;

        State = CommandState.Prepared;

        CustomLog.GreenText_Warning($"{squad.Name} prepared move");
    }
    public bool CanExecute()
    {
        if (wayCells == null || wayCells.Count == 0) { squadEndMovement?.Invoke(squad, wayCells[0]); return false; }

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<GameObject, SideEnum, CellController>((cell, side, sender) =>
        {
            CustomLog.RedText(cell.name);
            wayCells.ForEach(cell => { CustomLog.PurpleText(cell.name); });


            if (wayCells.Contains(cell) && side != squad.Side)
            {
                int index = wayCells.IndexOf(cell);

                if (index != -1) 
                {
                    GameObject previous = index > 0 ? wayCells[index - 1] : null;
                    GameObject next = index < wayCells.Count - 1 ? wayCells[index + 1] : null;

                    if(previous != null && next != null)
                    {
                        List<GameObject> pieceOfWay = ServiceRegistry.WorkWithController<AAlgorithm>().CreateWay(previous,next,squad.Side);

                        if (pieceOfWay == null || pieceOfWay.Count == 0) { return; }

                        pieceOfWay.Remove(previous);pieceOfWay.Remove(next);


                        wayCells.RemoveAt(index);
                        wayCells.InsertRange(index, pieceOfWay);

                    }
                }
            }
        });

        return true;
    }
    public void Execute()
    {
        if (State != CommandState.Prepared) return;

        ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(StartMovement());

        State = CommandState.Executing;
    }
    public void Cancel()
    {
        State = CommandState.Cancelled;
    }

    public SquadMovement(List<GameObject> wayCells, AbstractSquad squad)
    {
        this.wayCells = wayCells.ToList();
        this.squad = squad;

        State = CommandState.Created;
    }

    private IEnumerator StartMovement()
    {
        GameObject startWayCell = wayCells[0];

        GameObject startCell = wayCells[0];

        bool outOfTime = false;

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(startWayCell).GetParameter<CellSquadsOnArea>().TryRemoveSquad(squad);

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(wayCells[wayCells.Count - 1]).GetParameter<CellSquadsOnArea>().ChangePhantomCount(true);
        ServiceRegistry.WorkWithController<CellUIScript>().UpdateByMovementController(startCell);

        bool inMovement = false;

        void OneStepIsOver()
        {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(wayCells[0]).GetParameter<CellBuffs>().RemoveFromSquadBuffs(squad);
            wayCells.Remove(startCell);
            if (wayCells.Count == 1)
            {
                //gameControllerScript.UpdateSquadInformation(squad, wayCells[0]);
                ServiceRegistry.WorkWithController<GameController>().OnlyCaptureCell(squad.Side, wayCells[0]);
                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(wayCells[0]).GetParameter<CellBuffs>().SetToSquadBuffs(squad);

                inMovement = false;
                return;
            }

            ServiceRegistry.WorkWithController<GameController>().OnlyCaptureCell(squad.Side, wayCells[0]);
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(wayCells[0]).GetParameter<CellBuffs>().SetToSquadBuffs(squad);

            startCell = wayCells[0];
            inMovement = false;
        }

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<ArrowCanvasWorker>((sender) =>
        {
            if(sender == arrowCanvasWorker) { OneStepIsOver(); }
        });

        while (wayCells.Count > 0)
        {
            foreach (var cell in wayCells) { CustomLog.PurpleText($"{cell.name}"); }

            if (PauseScript.CurrentGameState != GameState.Play || inMovement)
            {
                yield return null;
                continue;
            }

            if (wayCells.Count == 1) break;

            if (squad.Side == SideEnum.Allies)//Можно упростить
            {
                if (!ServiceRegistry.WorkWithService<CellAccessibilityValidator>().InteractWithAlliesCell(wayCells[wayCells.IndexOf(startCell) + 1])){ 
                    outOfTime = true;
                    break; 

                }
            }
            else { if (ServiceRegistry.WorkWithService<CellAccessibilityValidator>().InteractWithAlliesCell(wayCells[wayCells.IndexOf(startCell) + 1])) { outOfTime = true; break; } }

            arrowCanvasWorker.CreateArrow(
                startCell.transform.position,
                wayCells[wayCells.IndexOf(startCell) + 1].transform.position,
                squad.GetAllSpeedOfMovement(),
                squad
            );

            inMovement = true;

            yield return new WaitUntil(() => !inMovement);
        }


        squadEndMovement?.Invoke(squad, wayCells[0]);
        squad.SquadAction = SquadActions.None;
        LogsController.AddLogElement($"Отряд {squad.Name} прибыл на клетку {wayCells[0]}", squad.Side);

        ServiceRegistry.WorkWithController<BattleController>().CheckDrawnIntoBattle(squad, wayCells[0]);

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(wayCells[wayCells.Count - 1]).GetParameter<CellSquadsOnArea>().ChangePhantomCount(false);

        ServiceRegistry.WorkWithController<GameController>().UpdateSquadInformation_ChangeCell(squad, startWayCell, wayCells[0]);
        ServiceRegistry.WorkWithController<CellUIScript>().UpdateByMovementController(wayCells[0]);

        ServiceRegistry.WorkWithService<EventBus>().Publish<SquadMovement, AbstractSquad, SideEnum>(this, squad, squad.Side);
        ServiceRegistry.WorkWithService<EventBus>().Publish<ICommand, SquadMovement, AbstractSquad, GameObject>(this, this, squad, wayCells[0]);
    }
}
public class ArrowCanvasWorker
{
    private AbstractSquad squadFor;

    public ArrowCanvasWorker(AbstractSquad squad)
    {
        squadFor = squad;

        ServiceRegistry.WorkWithController<ArrowCanvasScript>().squadUpdateRequest += OnEventTrigger;
    }

    public void CreateArrow(Vector3 startPostiion, Vector3 endPosition, float speed, AbstractSquad squad)
    {
        ServiceRegistry.WorkWithController<ArrowCanvasScript>().CreateArrow(startPostiion, endPosition, speed, squad);
    }

    private void OnEventTrigger(AbstractSquad squad)
    {
        if(squad == squadFor)
        {
            ServiceRegistry.WorkWithService<EventBus>().Publish<ArrowCanvasWorker>(this);
        }
    }
}