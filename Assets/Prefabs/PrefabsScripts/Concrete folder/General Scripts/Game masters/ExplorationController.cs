using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExplorationController : MonoBehaviour
{
    private Dictionary<AbstractSquad, ExplorationModule> explorationModules = new();

    public void StartExploration(GameObject startCell, GameObject finishCell, AbstractSquad squad)
    {
        explorationModules[squad] = new ExplorationModule(startCell, finishCell, squad);

        ServiceRegistry.WorkWithService<CommandBus>().Enqueue(explorationModules[squad]);
    }
}
public class ExplorationModule:ICommand
{
    private Action currentAction;

    private AbstractSquad scoutSquad;
    private GameObject exploratedCell;

    private int timeToReachLocation = 0;
    private bool isReached = false;

    public int SearchDepth { private get; set; } = 1;
    private float chanceToBeGrabbed = 0f;

    private List<GameObject> cellsNeedToExplore;
    private Dictionary<GameObject, List<System.Object>> informationByCell = new();
    private Dictionary<GameObject, int> chanceToExploreByCell = new();

    public ExplorationModule(GameObject startCell, GameObject finishCell, AbstractSquad squad)
    {
        //Debug.LogWarning($"Пытаюсь подсчиать цену пути для {startCell},{finishCell}");
        int wayCost = (int)ServiceRegistry.WorkWithController<AAlgorithm>().CalculateWayCost(startCell, finishCell, SideEnum.None);
        //Debug.LogWarning($"Подсчитана цена пути {wayCost}");

        timeToReachLocation =  wayCost * 1;//10

        scoutSquad = squad;
        exploratedCell = finishCell;

        ServiceRegistry.WorkWithService<EventBus>().Publish<AbstractSquad, Sprite>(squad, null);
        Debug.LogError("Запушил отряд");
    }

    private void SquadReachLocation()
    {
        timeToReachLocation--;

        chanceToBeGrabbed += 0.01f;
        if(timeToReachLocation == 0)
        {
            isReached = true;

            currentAction = SquadExploration;
        }
    }
    private void SquadExploration()
    {
        GameObject choosedCell = cellsNeedToExplore[UnityEngine.Random.Range(0,cellsNeedToExplore.Count)];

        if(UnityEngine.Random.Range(0, 100) < chanceToExploreByCell[choosedCell])
        {
            chanceToExploreByCell[choosedCell] = 1;

            System.Object exploratedObject = (System.Object)ServiceRegistry.WorkWithController<CellController>().
            WorkWithCell<CellParametersHandler>(choosedCell).GetParameter<CellBuildings>().GetRandomBuild(informationByCell[choosedCell]);

            if (exploratedObject != null && !informationByCell[choosedCell].Contains(exploratedObject)) 
            {
                //Логика упоминания в логах
                Debug.Log($"Для отряда {scoutSquad} добавлен объект {exploratedObject}");
                ServiceRegistry.WorkWithService<EventBus>().Publish<ExplorationModule, AbstractSquad, object>(this, scoutSquad, exploratedObject);

                informationByCell[choosedCell].Add(exploratedObject);

                chanceToBeGrabbed += 0.05f;
            }
            else
            {
                List<AbstractSquad> allExploratedSquads = informationByCell[choosedCell].OfType<AbstractSquad>().ToList();
                exploratedObject = ServiceRegistry.WorkWithController<GameController>().GetRandomSquadOnCell(choosedCell, allExploratedSquads);


                //Логика упоминания в логах
                Debug.Log($"Для отряда {scoutSquad} добавлен объект {exploratedObject}");
                ServiceRegistry.WorkWithService<EventBus>().Publish<ExplorationModule, AbstractSquad, object>(this, scoutSquad, exploratedObject);

                informationByCell[choosedCell].Add(exploratedObject);

                chanceToBeGrabbed += 0.05f;
            }
        }
        else { chanceToExploreByCell[choosedCell] += 5;
            //Debug.LogError($"{chanceToExploreByCell[choosedCell]} for {choosedCell}");
            ServiceRegistry.WorkWithService<EventBus>().Publish<ExplorationModule, AbstractSquad, object>(this, scoutSquad, null);
            }

            if (UnityEngine.Random.Range(0, 100) < chanceToBeGrabbed)
        {
            Debug.LogError($"Отряд {scoutSquad} был схвачен");
        }
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

        cellsNeedToExplore = ServiceRegistry.WorkWithController<CellController>().
            WorkWithCell<CellParametersHandler>(exploratedCell).GetParameter<CellArea>().NeighboresSearcher(SearchDepth);

        foreach (var cell in cellsNeedToExplore) 
        {
            informationByCell.Add(cell, new());
            chanceToExploreByCell.Add(cell, 1);
        }

        State = CommandState.Prepared;
    }

    public bool CanExecute()
    {
       return scoutSquad != null && timeToReachLocation != 0;
    }

    public void Execute()
    {
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<GameController>((sender) => { currentAction.Invoke();});
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
