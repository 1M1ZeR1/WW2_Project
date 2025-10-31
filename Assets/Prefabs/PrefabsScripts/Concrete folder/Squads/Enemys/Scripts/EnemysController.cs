using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EnemysController
{
    protected Dictionary<AbstractSquad,GameObject> _squadToCell = new Dictionary<AbstractSquad, GameObject>();

    [SerializeField] private GameObject mainBase;

    protected Dictionary<GameObject,bool> _capturedCells = new Dictionary<GameObject, bool>();

    protected DecisionTreeController treeController;

    [SerializeField] private AAlgorithm aAlgorithm;

    private int difficulty = 1;

    public void Start()
    {
        ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(baseCollector());

        ServiceRegistry.WorkWithController<BattleController>().SideOnCellWasChanged += CapturedCellListener;

        treeController = new DecisionTreeController(this);

        ServiceRegistry.WorkWithController<GameController>().oneSecondPassed += OneStep;
    }

    private IEnumerator baseCollector()
    {
        yield return new WaitForSeconds(5);

        foreach (var cell in ServiceRegistry.WorkWithController<GameController>().GetGlobalList())
        {
            if (cell.GetComponent<EnemysSpawner>().enabled)
            {
                _capturedCells.Add(cell,true);
            }
        }
    }

    private void OneStep()
    {
        //treeController.OneStep();
    }

    private void CapturedCellListener(GameObject cell)
    {
        if (_capturedCells.Keys.Contains(cell)) 
        {
            if (!_capturedCells[cell]) { _capturedCells[cell] = true; return; }
            treeController.AddLostedCells(cell); _capturedCells[cell] = false; 
        }
    }

    public List<AbstractSquad> GetAllEnemysOnCell(GameObject cell)
    {
        return _squadToCell.Where(p => p.Value == cell).Select(p => p.Key).ToList();
    }
    public void SetCell(AbstractSquad squad,GameObject cell)
    {
        _squadToCell[squad] = cell;
    }

    public void AllowAll(AbstractSquad squad)
    {
        if (_squadToCell.ContainsKey(squad)) { squad.SquadAction = SquadActions.None; }
    }
    public void AddBot(AbstractSquad squad,GameObject cell) { _squadToCell.Add(squad,cell); }
    public void RemoveBot(AbstractSquad squad) { _squadToCell.Remove(squad); }

    public GameObject GetMainBaseCell() { return mainBase; }
    public int GetDifficulty() { return difficulty; }

    public void AddToCapturedCell(GameObject cell) { _capturedCells.Add(cell, true); }
    public void AddToCapturedCell_Safety(GameObject cell) { if (_capturedCells.Keys.Contains(cell)) _capturedCells[cell] = true;
        else _capturedCells.Add(cell, true);
    }

    public GameObject GetRandomCapturedCell()
    {
        if(_capturedCells.Count == 0) return null;
        List<GameObject> listOfCells = _capturedCells.Keys.ToList();
        
        var selectedCell = listOfCells[Random.Range(0, listOfCells.Count - 1)];

        if (!_capturedCells[selectedCell]) { return GetRandomCapturedCell(); }
        else { return selectedCell; }
    }
}

public class DecisionTreeController
{
    private List<GameObject> lostedCells = new List<GameObject>();

    private Dictionary<DecisionAction,bool> actions = new Dictionary<DecisionAction,bool>();

    private List<DecisionAction> actionsToAdd = new List<DecisionAction>();

    public readonly EnemysController enemysControllerScript;

    protected int _Economy_Score = 1000;

    protected float _difficulty = 1;

    public DecisionTreeController(EnemysController enemysControllerScript)
    {
        this.enemysControllerScript = enemysControllerScript;
    }

    public void OneStep()
    {
        if(lostedCells.Count > 0)
        {
            Debug.Log($"Я хочу захватить обратно {lostedCells[0]}|| Кол-во утраченых клеток:{lostedCells.Count}");

            DecisionAction_ReturnTerritory action_ReturnTerritory = new DecisionAction_ReturnTerritory(enemysControllerScript, lostedCells[0], _Economy_Score);

            action_ReturnTerritory.actionIsOver += (bool success, DecisionAction action, GameObject cell) => 
            {
                ActionIsOver(success,action,cell);
            };

            actions.Add(action_ReturnTerritory,false);

            lostedCells.RemoveAt(0);
        }

        EconomyStep();

        if(actions.Count > 0)
        {
            var actionsList = actions.Keys.ToList();

            for(int i  = 0; i < actionsList.Count; i++)
            {
                actionsList[i].Execute();
            }
        }

        AddToActionList();
    }

    public void EconomyStep()
    {
        _Economy_Score += (int)(1 * _difficulty);if(_Economy_Score <= 200) { return; }

        List<EconomyStepEnum> economySteps = new List<EconomyStepEnum>();

        foreach (var economyStep in EconomyStepPrice)
        {
            if (economyStep.Value <= _Economy_Score)
            {
                economySteps.Add(economyStep.Key);
            }
        }

        if (economySteps != null && economySteps.Count != 0)
        {
            switch (economySteps[Random.Range(0, economySteps.Count)])
            {
                case EconomyStepEnum.Buildings:

                    var selectedCell = enemysControllerScript.GetRandomCapturedCell();
                    if (selectedCell != null)
                    {
                        _Economy_Score -= EconomyStepPrice[EconomyStepEnum.Buildings];

                        var action = new DecisionAction_StreigtheningTerritory(selectedCell);

                        action.actionIsOver += (bool success, DecisionAction action, GameObject cell) =>
                        {
                            ActionIsOver(success, action, cell);
                        };
                        AddActionToListLater(action);
                    }
                    break;

            }
        }
    }
    private void ActionIsOver(bool success, DecisionAction decisionAction, GameObject cell)
    {
        if(success) { actions.Remove(decisionAction); Debug.Log($"Удаляю ивент {decisionAction}"); }
        else
        {
            if(decisionAction is IDecisionAction_CanRestart restart)
            {
                if (restart.Restart() == null) { actions.Remove(decisionAction); }
            }
        }
    }

    public void AddLostedCells(GameObject cell) { lostedCells.Add(cell); }

    private void AddToActionList()
    {
        while(actionsToAdd.Count > 0)
        {
            actions.Add(actionsToAdd[0], false);
            actionsToAdd.RemoveAt(0);
        }
    }
    public void AddActionToListLater(DecisionAction action)
    {
        actionsToAdd.Add(action);
    }

    private enum EconomyStepEnum
    {
        None,
        Buildings,

    }
    private Dictionary<EconomyStepEnum, int> EconomyStepPrice = new Dictionary<EconomyStepEnum, int>
    {
        { EconomyStepEnum.Buildings,1000}
    };
}

public abstract class DecisionAction
{
    public delegate void ActionIsOver(bool success, DecisionAction decisionAction, GameObject cell);
    public event ActionIsOver actionIsOver;

    public void TriggerEvent(bool success, DecisionAction decisionAction, GameObject cell)
    {
        actionIsOver.Invoke(success, decisionAction, cell);
    }

    public abstract void Execute();
}
public class DecisionAction_ReturnTerritory:DecisionAction,IDecisionAction_WorkWithSquads,IDecisionAction_CanRestart, ICoroutineWorker
{
    protected EnemysController enemysControllerScript;
    protected GameObject cellNeedToCapture;

    protected List<AbstractSquad> _squadInThisAction = new List<AbstractSquad>();
    protected List<AbstractSquad> _spawnedSquads = new List<AbstractSquad>();

    protected int _countOfReachedSquads = 0;

    protected readonly float _freeRatio = 0.5f;
    protected bool inAction = false;

    protected int economyScore;

    public void AddSquadToActionList(AbstractSquad squad) { _squadInThisAction.Add(squad); }
    public void RemoveSquadToActionList(AbstractSquad squad) { _squadInThisAction.Remove(squad); }

    public DecisionAction_ReturnTerritory(
        EnemysController enemysControllerScript, GameObject cellWhatLost,
        int economyScore) : base()
    {
        this.enemysControllerScript = enemysControllerScript;
        cellNeedToCapture = cellWhatLost;

        this.economyScore = economyScore;
    }
    public DecisionAction Restart()
    {
        bool hasNeighbore = false;

        foreach(var cell in ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellNeedToCapture).GetParameter<CellArea>().GetNeighbores())
        {
            if(!ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().IsAllies())hasNeighbore = true;
        }

        if (hasNeighbore) { return this; }
        else { return null; }
    }
    public override void Execute()
    {
        if (inAction) return;

        List<AbstractSquad> currentNoneActionSquads = new List<AbstractSquad>();

        foreach (var cell in ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellNeedToCapture).GetParameter<CellArea>().GetNeighbores()) 
        {
            if (!ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().IsAllies())
            {
                currentNoneActionSquads.AddRange(ServiceRegistry.WorkWithController<GameController>().GetAllEnemysOnCell(cell));
            }
        }

        _squadInThisAction = currentNoneActionSquads.ToList();

        if (_squadInThisAction.Count > 5)
        {
            StartAttack();

            _squadInThisAction.RemoveRange(0,6);
        }
        else
        {
            if (economyScore >= 200 && _spawnedSquads.Count == 0)
            {
                economyScore -= 200;
                _spawnedSquads = SpawnSquadsToAction(enemysControllerScript.GetDifficulty()).ToList();

                foreach (var squad in _spawnedSquads)
                {
                    ServiceRegistry.WorkWithService<MovementController>().AddMovementForSquad(enemysControllerScript.GetMainBaseCell(), cellNeedToCapture, squad);

                    ServiceRegistry.WorkWithService<MovementController>().AddEvent(squad, SquadHasReached);
                }
            }
        }
    }

    private void SquadHasReached(AbstractSquad squad, GameObject stopCell)
    {
        if (_spawnedSquads.Contains(squad)) 
        {
            _squadInThisAction.Add(squad);
            _spawnedSquads.Remove(squad);
        }
    }

    private void StartAttack()
    {
        ServiceRegistry.WorkWithController<BattleController>().TryStartBattle_Simple(cellNeedToCapture, _squadInThisAction.GetRange(0, 6));

        ServiceRegistry.WorkWithController<BattleController>().BattleOnCellIsOver += OnBattleIsOver;

        ServiceRegistry.WorkWithController<BattleController>().SideOnCellWasChanged += OnSideChanged;

        inAction = true;
    }

    public void CoroutineWorker()
    {
        ServiceRegistry.WorkWithController<MonobehaviourMaster>().CoroutineStarter(Cooldown(30 - enemysControllerScript.GetDifficulty()));
    }
    private IEnumerator Cooldown(int count)
    {
        WaitForSeconds timer = new WaitForSeconds(count/1);

        yield return timer;

        inAction = false;
    }

    private List<AbstractSquad> SpawnSquadsToAction(int count)
    {
        List<AbstractSquad> result = new List<AbstractSquad>();

        //for(int i =0; i< count; i++)
        //{
        //    InfantrySquad squad = new InfantrySquad(3, Random.Range(10, 25), RandomTransport(), RandomWeapon());
        //    squad.Side = SideEnum.Enemys;

        //    ServiceRegistry.WorkWithController<GameController>().AddSquadInDictionary_Safety(squad, enemysControllerScript.GetMainBaseCell());

        //    result.Add(squad);
        //}

        return result;
    }

    private SquadTransport RandomTransport()
    {
        switch (Random.Range(0, 3))
        {
            case 0: return SquadTransport.ByFoot;
            case 1: return SquadTransport.Horses;
            case 2: return SquadTransport.Cars;
        }
        return SquadTransport.None;
    }
    private SquadWeapon RandomWeapon()
    {
        switch (Random.Range(0, 3))
        {
            case 0: return SquadWeapon.MachineHun;
            case 1: return SquadWeapon.Rifle;
            case 2: return SquadWeapon.SniperRifle;
        }
        return SquadWeapon.None;
    }

    private void OnSideChanged(GameObject cell)
    {
        if (cell == cellNeedToCapture) { TriggerEvent(true, this, cellNeedToCapture); Debug.Log($"Удачный захват {cellNeedToCapture}");

            ServiceRegistry.WorkWithController<BattleController>().BattleOnCellIsOver -= OnBattleIsOver;

            ServiceRegistry.WorkWithController<BattleController>().SideOnCellWasChanged -= OnSideChanged;
        }
    }
    private void OnBattleIsOver(GameObject cell)
    {
        if (cell == cellNeedToCapture) { TriggerEvent(false, this, cellNeedToCapture); Debug.Log($"Неудачный захват {cellNeedToCapture}");

            ServiceRegistry.WorkWithController<BattleController>().BattleOnCellIsOver -= OnBattleIsOver;

            ServiceRegistry.WorkWithController<BattleController>().SideOnCellWasChanged -= OnSideChanged;

            CoroutineWorker();
        }
    }
}
public class DecisionAction_StreigtheningTerritory:DecisionAction
{
    private GameObject selectedCell;
    public DecisionAction_StreigtheningTerritory(GameObject cellNeedToStreigthening):base() 
    {
        Debug.Log($"Укрепля точку:{cellNeedToStreigthening}");
        selectedCell = cellNeedToStreigthening;
    }

    public override void Execute()
    {
        BuildStreigthening(selectedCell);
    }

    private void BuildStreigthening(GameObject cell)
    {
        CellBuildings cellBuildings = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuildings>();

        cellBuildings.DebugFunction_NameAllBuildings();

        Debug.Log($"Список для:{cell}");

        if (!cellBuildings.CheckBuildIsBuilt("FoxholeBuild"))
        {
            foreach(var squad in ServiceRegistry.WorkWithController<GameController>().GetAllEnemysOnCell(cell))
            {
                if (squad is EngineerSquad) { cellBuildings.AddToBuildList_Safely(new FoxholeBuild(), "FoxholeBuild"); TriggerEvent(true,this,cell); Debug.Log("Вырыл окоп"); break; }
            }
        }
        if (!cellBuildings.CheckBuildIsBuilt("FortBuild"))
        {
            if(ServiceRegistry.WorkWithController<GameController>().GetAllEnemysOnCell(cell).Count != 0)
            {
                cellBuildings.AddToBuildList_Safely(new FortBuild(), "FortBuild");

                TriggerEvent(true, this, cell);

                Debug.Log("Построил форт");
            }
        }

        TriggerEvent(false, this, cell);
    }
    public void ChangeCell(GameObject cell) { selectedCell = cell; }
}
public class DecisionAction_CatchTerritory:DecisionAction,IDecisionAction_WorkWithSquads
{
    private string[] squadsId = new string[] { "InfantrySquad", "Engineers" };

    private List<AbstractSquad> squadsInAction = new List<AbstractSquad>();
    private WayCellsToCatch _cellToCatchController;

    private GameObject mainBase;

    protected bool _inAction = false;

    protected int _freeSquads = 0;

    private float maxFinderDistance;

    public DecisionAction_CatchTerritory(GameObject mainBase,float maxFinderDistance)
    {

        this.mainBase = mainBase;
        _cellToCatchController = new WayCellsToCatch();

        this.maxFinderDistance = maxFinderDistance;

        for (int i = 0; i < 5; i++)
        {
            ServiceRegistry.WorkWithController<UnitsSpawner>().SpawnSquadOnCell(squadsId[Random.Range(0, squadsId.Length)],SideEnum.Enemys, mainBase);
        }

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<GameController, int, AbstractSquad>((sender, var, squad) =>
        {
            if (var == 2)
            {
                if (squadsInAction.Contains(squad)) { squadsInAction.Remove(squad); }
            }
        });


    }

    public override void Execute()
    {
        if (_inAction) { return; }
        if (!_cellToCatchController.HasWay()) 
        {
            if (!_cellToCatchController.CreateWay(mainBase, maxFinderDistance)) { return; }
        }

        var cellsToMove_OneStep = _cellToCatchController.GetOneStep();

        foreach(var squad in squadsInAction)
        {
            ServiceRegistry.WorkWithService<MovementController>().AddMovementForSquad(cellsToMove_OneStep.Item1, cellsToMove_OneStep.Item2, squad);

            ServiceRegistry.WorkWithService<MovementController>().AddEvent(squad, (AbstractSquad squad, GameObject cell) =>
            {
                if (squadsInAction.Contains(squad))
                {
                    if (squad.SquadAction == SquadActions.None) { _freeSquads++; }

                    if (_freeSquads == squadsInAction.Count)
                    {
                        _inAction = false;
                    }
                }
            });
        }


    }
    public void RemoveSquadToActionList(AbstractSquad squad)
    {
        squadsInAction.Remove(squad);
    }
    public void AddSquadToActionList(AbstractSquad squad)
    {
        squadsInAction.Add(squad);
    }
    public void CoroutineWorker()
    {
        throw new System.NotImplementedException();
    }

    private class WayCellsToCatch
    {
        private List<GameObject> cellsWay;

        private GameObject currentCell, nextCell;

        public bool CreateWay(GameObject mainBase, float maxDistance)
        {
            GameObject cellToCapture = ServiceRegistry.WorkWithController<AAlgorithm>().GetRandomCell();
            if (Vector3.Distance(cellToCapture.transform.position, mainBase.transform.position) > maxDistance) return false;


            //cellsWay = ServiceRegistry.WorkWithController<AAlgorithm>().GetAttackWay(mainBase, cellToCapture, SideEnum.Enemys).ToList();

            currentCell = cellsWay[0];
            nextCell = cellsWay[1];

            return true;
        }

        public (GameObject,GameObject) GetOneStep()
        {
            var result = (currentCell, nextCell);

            cellsWay.RemoveAt(0);

            currentCell = cellsWay[0];
            nextCell = cellsWay[1];

            return result;
        } 

        public bool HasWay() { return cellsWay.Count > 0; }
    }
}

public interface IDecisionAction_WorkWithSquads
{
    public void AddSquadToActionList(AbstractSquad squad);
    public void RemoveSquadToActionList(AbstractSquad squad);
}
public interface ICoroutineWorker
{
    public void CoroutineWorker();
}
public interface IDecisionAction_CanRestart
{
    public DecisionAction Restart();
}
public interface IDecisionAction_SwitchMode
{
    
}
