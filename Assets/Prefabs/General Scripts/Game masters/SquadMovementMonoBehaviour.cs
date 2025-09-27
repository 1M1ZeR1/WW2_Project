using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SquadMovementMonoBehaviour : MonoBehaviour
{
    [SerializeField] private CellUIScript cellUIScript;
    [SerializeField]private AAlgorithm aAlgorithm;

 
    private void Start()
    {
        ServiceRegistry.WorkWithController<MovementController>().SetWorkers(aAlgorithm,cellUIScript.UpdatePanelInfo, this);
    }
}

public interface ICoroutineAction
{
    IEnumerator Execute();
}

public class MovementController
{
    private AAlgorithm aAlgorithm;
    private Action<GameObject> updateSquadsAction;

    private MonoBehaviour coroutineStarter;//СДЕЛАТЬ ОТДЕЛЬНЫЙ COROUTINE HOST для запуска и контроля всех коротин в игре!!!!!

    protected Dictionary<AbstractSquad, SquadMovement> _squadAndThierMovement = new();

    private Dictionary<AbstractSquad,Action<AbstractSquad,GameObject>> squadsNotification = new();

    public void SetWorkers(AAlgorithm aAlgorithm, Action<GameObject> actionToUpdateInfo, MonoBehaviour coroutineStarter)
    {
        this.aAlgorithm = aAlgorithm;
        updateSquadsAction = actionToUpdateInfo;

        this.coroutineStarter = coroutineStarter;
    }

    public void AddMovementForSquad(GameObject startCell, GameObject finishCell, AbstractSquad squad)
    {
        if (squad.Action != SquadActions.None) { return; }
        squad.Action = SquadActions.Moving;

        ServiceRegistry.WorkWithController<GameController>().UpdateSquadInformation_SwipeState(squad);
        updateSquadsAction.Invoke(startCell);

        if (squad.Side == SideEnum.Enemys) 
        {
            SquadMovement newMovement = new SquadMovement(aAlgorithm.CreateWay_Enemy(startCell, finishCell),squad);
            _squadAndThierMovement.Add(squad, newMovement);

            newMovement.squadEndMovement += (AbstractSquad squad, GameObject cell) =>
            {
                _squadAndThierMovement.Remove(squad);

                if (squadsNotification.ContainsKey(squad)) { squadsNotification[squad].Invoke(squad, cell); squadsNotification.Remove(squad); }
            };

            coroutineStarter.StartCoroutine(newMovement.Execute()); 
        }
        else 
        {
            SquadMovement newMovement = new SquadMovement(aAlgorithm.CreateWay(startCell, finishCell), squad);
            _squadAndThierMovement.Add(squad, newMovement);

            newMovement.squadEndMovement += (AbstractSquad squad, GameObject cell) =>
            {
                _squadAndThierMovement.Remove(squad);

                if (squadsNotification.ContainsKey(squad)) { squadsNotification[squad].Invoke(squad, cell); squadsNotification.Remove(squad); }
            };

            coroutineStarter.StartCoroutine(newMovement.Execute());
        }
    }

    public void AddEvent(AbstractSquad squad, Action<AbstractSquad,GameObject> action)
    {
        squadsNotification.Add(squad,action);
    }
}
public class SquadMovement:ICoroutineAction
{
    private List<GameObject> wayCells;
    private AbstractSquad squad;

    public event Action<AbstractSquad,GameObject> squadEndMovement;

    private ArrowCanvasWorker arrowCanvasWorker;

    public SquadMovement(List<GameObject> wayCells, AbstractSquad squad)
    {
        this.wayCells = wayCells.ToList();
        this.squad = squad;

        arrowCanvasWorker = new ArrowCanvasWorker(squad);
    }

    public IEnumerator Execute()
    {
        if (wayCells == null || wayCells.Count == 0) { squadEndMovement?.Invoke(squad, wayCells[0]); }

        GameObject startWayCell = wayCells[0];

        AbstractSquad currentSquad = squad;
        GameObject startCell = wayCells[0];

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(wayCells[0]).GetParameter<CellSquadsOnArea>().squadsOnCell.Remove(squad);

        bool inMovement = false;

        void OneStepIsOver(AbstractSquad squad)
        {
            if (squad == currentSquad)
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
        }

        arrowCanvasWorker.SquadEndOneStep += OneStepIsOver;

        while (wayCells.Count > 0)
        {
            if (PauseScript.CurrentGameState != GameState.Play || inMovement)
            {
                yield return null;
                continue;
            }

            if (wayCells.Count == 1) break;

            if (squad.Side == SideEnum.Allies)//Можно упростить
            {
                if (!ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(wayCells[wayCells.IndexOf(startCell) + 1])){ break; }
            }
            else { if (ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(wayCells[wayCells.IndexOf(startCell) + 1])) { break; } }

            arrowCanvasWorker.CreateArrow(
                startCell.transform.position,
                wayCells[wayCells.IndexOf(startCell) + 1].transform.position,
                currentSquad.GetAllSpeedOfMovement(),
                currentSquad
            );

            inMovement = true;

            yield return new WaitUntil(() => !inMovement);
        }

        squadEndMovement?.Invoke(squad, wayCells[0]);
        squad.Action = SquadActions.None;
        LogsController.AddLogElement($"Отряд {squad.Name} прибыл на клетку {wayCells[0]}", squad.Side);

        ServiceRegistry.WorkWithController<BattleController>().CheckDrawnIntoBattle(squad, wayCells[0]);

        ServiceRegistry.WorkWithController<GameController>().UpdateSquadInformation_ChangeCell(squad, startWayCell, wayCells[0]);

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(wayCells[0]).GetParameter<CellSquadsOnArea>().squadsOnCell.Add(squad);
    }
}
public class ArrowCanvasWorker
{
    public event Action<AbstractSquad> SquadEndOneStep;

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
            SquadEndOneStep?.Invoke(squad);
        }
    }
}