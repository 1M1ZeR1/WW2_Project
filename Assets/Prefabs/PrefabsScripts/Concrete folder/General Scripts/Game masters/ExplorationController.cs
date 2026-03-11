using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExplorationController : MonoBehaviour
{
    private Dictionary<AbstractSquad, ExplorationModule> explorationModules = new();

    public AbstractSquad SquadGoingRevoke { private get; set; }

    private void Start()
    {
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<ExplorationController, ExplorationSquadList, AbstractSquad>((Key1, key2, scoutSquad)=>{ explorationModules.Remove(scoutSquad); });
    }

    public void StartExploration(GameObject startCell, GameObject finishCell, List<GameObject> cellsToExplore, AbstractSquad squad)
    {
        explorationModules[squad] = new ExplorationModule(startCell, finishCell, squad);
        explorationModules[squad].cellsNeedToExplore = cellsToExplore.ToList();

        ServiceRegistry.WorkWithService<CommandBus>().Enqueue(explorationModules[squad]);
    }

    public void FinishExploration(GameObject cellToRevoke)
    {
        explorationModules[SquadGoingRevoke].RevokeSquad(cellToRevoke);
        SquadGoingRevoke = null;
    }
}
public class ExplorationModule:ICommand
{
    private bool inRevokeAction = false;
    private bool inAction = false;

    private Action currentAction;

    private AbstractSquad scoutSquad;
    private GameObject exploratedCell;
    private GameObject revokeCell;

    private int timeToReachLocation = 0;
    private bool isReached = false;

    private float chanceToBeGrabbed = 0f;

    private int explorationPoints = 0;

    private GameObject startCell;

    public List<GameObject> cellsNeedToExplore {private get; set; }
    private Dictionary<GameObject, List<System.Object>> informationByCell = new();
    private Dictionary<GameObject, int> chanceToExploreByCell = new();

    public ExplorationModule(GameObject startCell, GameObject finishCell, AbstractSquad squad)
    {
        //Debug.LogWarning($"Пытаюсь подсчиать цену пути для {startCell},{finishCell}");
        int wayCost = (int)ServiceRegistry.WorkWithController<AAlgorithm>().CalculateWayCost(startCell, finishCell, SideEnum.None);
        //Debug.LogWarning($"Подсчитана цена пути {wayCost}");

        timeToReachLocation =  wayCost * 1;//10

        this.startCell = startCell;

        scoutSquad = squad;
        exploratedCell = finishCell;

        ServiceRegistry.WorkWithService<EventBus>().Publish<ScoutSquad, Sprite>((ScoutSquad)squad, null);
        
        Debug.LogError("Запушил отряд");
    }

    public void RevokeSquad(GameObject toCell)
    {
        revokeCell = toCell;

        int wayCost = (int)ServiceRegistry.WorkWithController<AAlgorithm>().CalculateWayCost(cellsNeedToExplore[0], toCell, SideEnum.None);

        timeToReachLocation = wayCost * 1;

        currentAction = SquadReachLocation;

        inRevokeAction = true;

        ServiceRegistry.WorkWithService<EventBus>().Publish<ExplorationController, ExplorationSquadList, AbstractSquad>(null, null, scoutSquad);
    }

    private void SquadReachLocation()
    {
        timeToReachLocation--;

        if(timeToReachLocation <= 0)
        {
            if (inRevokeAction)
            {
                inAction = false;

                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(revokeCell).GetParameter<CellSquadsOnArea>().TryAddSquad(scoutSquad);

                if(startCell == revokeCell)
                {
                    ServiceRegistry.WorkWithService<EventBus>().Publish<GameController, int, AbstractSquad>(null,1, scoutSquad);
                }
                else ServiceRegistry.WorkWithController<CellUIScript>().HelpToSwipeSquadPanel_Outside(scoutSquad, startCell, revokeCell);

                return;
            }
            isReached = true;

            currentAction = SquadExploration;
        }
        chanceToBeGrabbed += 0.01f;
    }
    private void SquadExploration()
    {
        explorationPoints++;

        if(explorationPoints < 12)return;

        explorationPoints = 0;

        GameObject choosedCell = cellsNeedToExplore[UnityEngine.Random.Range(0,cellsNeedToExplore.Count)];

        if(UnityEngine.Random.Range(0, 100) < chanceToExploreByCell[choosedCell])
        {
            chanceToExploreByCell[choosedCell] = 20;

            System.Object exploratedObject = (System.Object)ServiceRegistry.WorkWithController<CellController>().
            WorkWithCell<CellParametersHandler>(choosedCell).GetParameter<CellBuildings>().GetRandomBuild(informationByCell[choosedCell]);

            if (exploratedObject != null && !informationByCell[choosedCell].Contains(exploratedObject)) 
            {
                //Логика упоминания в логах
                Debug.Log($"Для отряда {scoutSquad} добавлен объект {exploratedObject}");
                ServiceRegistry.WorkWithService<EventBus>().Publish<ExplorationModule, AbstractSquad, object>(this, scoutSquad, exploratedObject);

                informationByCell[choosedCell].Add(exploratedObject);

                //chanceToBeGrabbed += 0.05f;
                chanceToBeGrabbed += 10;
            }
            else
            {
                List<AbstractSquad> allExploratedSquads = informationByCell[choosedCell].OfType<AbstractSquad>().ToList();
                exploratedObject = ServiceRegistry.WorkWithController<GameController>().GetRandomSquadOnCell(choosedCell, allExploratedSquads);


                //Логика упоминания в логах
                Debug.Log($"Для отряда {scoutSquad} добавлен объект {exploratedObject}");
                ServiceRegistry.WorkWithService<EventBus>().Publish<ExplorationModule, AbstractSquad, object>(this, scoutSquad, exploratedObject);

                informationByCell[choosedCell].Add(exploratedObject);

                //chanceToBeGrabbed += 0.05f;
                chanceToBeGrabbed += 10;
            }
        }
        else { chanceToExploreByCell[choosedCell] += 20;
            //Debug.LogError($"{chanceToExploreByCell[choosedCell]} for {choosedCell}");
            ServiceRegistry.WorkWithService<EventBus>().Publish<ExplorationModule, AbstractSquad, object>(this, scoutSquad, null);
        }

        if (UnityEngine.Random.Range(0, 100) < chanceToBeGrabbed)
        {
            SquadWasGrabbed();
        }
    }

    private void SquadWasGrabbed()
    {
        inAction = false;

        ServiceRegistry.WorkWithService<EventBus>().Publish<ExplorationController, ExplorationSquadList, AbstractSquad>(null,null,scoutSquad);
    }


    //Command logic
    public Guid Id { get; set; }

    public CommandState State { get; private set; } = CommandState.Created;

    public void Cancel()
    {
        throw new NotImplementedException();
    }

    public void Prepare()
    {
        currentAction = SquadReachLocation;

        foreach (var cell in cellsNeedToExplore) 
        {
            if (informationByCell.ContainsKey(cell)) continue;
            informationByCell.Add(cell, new());
            chanceToExploreByCell.Add(cell, 20);
        }

        State = CommandState.Prepared;

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<GameController>((sender) => {if(inAction)currentAction.Invoke(); });
    }

    public bool CanExecute()
    {
       return scoutSquad != null && timeToReachLocation != 0;
    }

    public void Execute()
    {
        inAction = true;
    }

    private class ExploratedInformationUnit
    {
        public System.Object Object { get; private set; }

        public ExploratedInformationUnit(System.Object Object)
        {
            this.Object = Object;
        }
    }
}
