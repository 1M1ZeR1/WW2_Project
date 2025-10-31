using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameController
{
    public delegate void GameTimer();
    public GameTimer oneSecondPassed;
    protected float _timer;

    public delegate void TimeTake();
    public TimeTake oneHourLeft;
    protected float _timerTime;

    public delegate void CellCaptured(GameObject cell);
    public event CellCaptured SideOnCellWasChanged;

    private Dictionary<AbstractSquad,GameObject> squadsDictionary = new Dictionary<AbstractSquad,GameObject>();

    private Dictionary<GameObject,List<AbstractSquad>> campsAndThereTrainingSquads = new Dictionary<GameObject, List<AbstractSquad>>();

    protected static ConcurrentQueue<System.Action> gameActions = new ConcurrentQueue<System.Action>();

    private List<GameObject> cellsList = new List<GameObject>();

    public delegate void SquadDictionaryHasChanged_ChangedCell(AbstractSquad squad, GameObject cellFrom, GameObject cellTo);
    public event SquadDictionaryHasChanged_ChangedCell dictionaryUpdatedEvent;


    public delegate void SquadDictionaryHasIncreased(AbstractSquad squad, GameObject cell);
    public event SquadDictionaryHasIncreased dictionaryIncreased;

    public void Start()
    {
        Transform cellGrid = GameObject.Find("2 Layer(Grid)").transform;

        for(int i = 0; i < cellGrid.childCount; i++)
        {
            cellsList.Add(cellGrid.GetChild(i).gameObject);
        }

        ServiceRegistry.WorkWithService<MonobehaviourMaster>().actionsToUpdate.Add(Update);
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if(_timer >= 1f)
        {
            if(oneSecondPassed != null)
            {
                oneSecondPassed.Invoke();
            }
            _timer = 0f;
        }
        if(PauseScript.CurrentGameState == GameState.Play)
        {
            _timerTime += Time.deltaTime;
            if(_timerTime >= 1f)
            {
                if(oneHourLeft != null) { oneHourLeft.Invoke();}
                _timerTime = 0f;
            }

            ProccesingQueue();
        }
    }
    public static void AddActionToQueue(System.Action action) {gameActions.Enqueue(action); }
    private void ProccesingQueue()
    {
        if(gameActions.TryDequeue(out var nextAction))
        {
            nextAction.Invoke();
        }
    }

    public void AddSquadInDictionary(AbstractSquad squad, GameObject cell)
    {
        squadsDictionary.Add(squad,cell);
        dictionaryIncreased.Invoke(squad,cell);

        if(squad.Side == SideEnum.Enemys) { ServiceRegistry.WorkWithController<EnemysController>().AddBot(squad,cell); }
    }
    public void AddSquadInDictionary_Safety(AbstractSquad squad, GameObject cell)
    {
        if (squadsDictionary.ContainsKey(squad))
        {
            squadsDictionary[squad] = cell;
            dictionaryIncreased.Invoke(squad, cell);

            if (squad.Side == SideEnum.Enemys) { ServiceRegistry.WorkWithController<EnemysController>().SetCell(squad, cell); }
        }
        else
        {
            squadsDictionary.Add(squad, cell);
            if(dictionaryIncreased != null)dictionaryIncreased.Invoke(squad, cell);

            if (squad.Side == SideEnum.Enemys) { ServiceRegistry.WorkWithController<EnemysController>().AddBot(squad, cell); }
        }
    }
    public void RemoveSquadFromDictionary(AbstractSquad squad, GameObject cell)
    {
        squadsDictionary[squad] = null;
    }
    public void UpdateSquadInformation_ChangeCell(AbstractSquad squad, GameObject cellFrom, GameObject cellTo)
    {
        if (squadsDictionary.ContainsKey(squad))
        {
            squadsDictionary[squad] = cellTo;

            dictionaryUpdatedEvent.Invoke(squad, cellFrom,cellTo);
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellFrom).GetParameter<CellSquadsOnArea>().SwitchSquad(squad,cellTo);

            if (squad.Side == SideEnum.Enemys)
            {
                ServiceRegistry.WorkWithController<EnemysController>().SetCell(squad, cellTo);
            }
        }
    }
    public void UpdateSquadInformation_SwipeState(AbstractSquad squad)
    {
        if (squadsDictionary.ContainsKey(squad))
        {
            ServiceRegistry.WorkWithService<EventBus>().Publish<GameController, int, AbstractSquad>(this, 1, squad);
        }
    }

    public void OnlyCaptureCell(SideEnum side, GameObject cell)
    {
        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().RequestToControlCell(side);

        if (SideOnCellWasChanged != null) { SideOnCellWasChanged.Invoke(cell); }
    }
    public void ActionIsOver(AbstractSquad? squad,GameObject? cell)
    {
        if(cell == null)
        {

        }

    }
    public List<AbstractSquad> ConvertSquadToSquadList(AbstractSquad squad)
    {
        return GetAllSquadsOnCell(squadsDictionary[squad]);
    }
    public List<AbstractSquad>? GetAllSquadsInGame()
    {
        return squadsDictionary.Keys.ToList<AbstractSquad>();
    }
    public List<AbstractSquad> GetAllSquadsOnCell(GameObject cell)
    {
        return squadsDictionary.Where(p => p.Value == cell).Select(p => p.Key).ToList();
    }
    public List<AbstractSquad> GetAllEnemysOnCell(GameObject cell)
    {
        return ServiceRegistry.WorkWithController<EnemysController>().GetAllEnemysOnCell(cell);
    }
    public GameObject GetCellWithThisSquad(AbstractSquad squad) { return squadsDictionary[squad]; }

    public AbstractSquad GetNewEnemySquad(List<AbstractSquad> squads, GameObject cell)
    {
        foreach (var squad in GetAllSquadsOnCell(cell))
        {
            if (!squads.Contains(squad) || squads.Count == 0)
            {
                return squad;
            }
        }

        return null;
    }

    public void AllowBotToAct(AbstractSquad squad)
    {
        ServiceRegistry.WorkWithController<EnemysController>().AllowAll(squad);
    }


    public void SingleThrasher_Squad(AbstractSquad squad)
    {
        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(squadsDictionary[squad]).GetParameter<CellSquadsOnArea>().squadsOnCell.Remove(squad);
        if (squadsDictionary.ContainsKey(squad))squadsDictionary.Remove(squad);

        ServiceRegistry.WorkWithService<EventBus>().Publish<GameController,int,AbstractSquad>(this,2,squad);
    }

    public void AddSquadInTraining(GameObject cell, AbstractSquad squad)
    {
        if (!campsAndThereTrainingSquads.ContainsKey(cell))
        {
            campsAndThereTrainingSquads.Add(cell, new List<AbstractSquad>());
        }
        campsAndThereTrainingSquads[cell].Add(squad);
    }
    public List<AbstractSquad> GetAllSquadsInTraining(GameObject cell)
    {
        if (campsAndThereTrainingSquads.Count > 0)
        {
            if (campsAndThereTrainingSquads.ContainsKey(cell))
            {
                return campsAndThereTrainingSquads[cell];
            }
        }
        return new List<AbstractSquad> { };
    }
    public void TrainingIsOver(AbstractSquad squad)
    {
        GameObject cell = campsAndThereTrainingSquads.FirstOrDefault(k => k.Value.Contains(squad)).Key;

        campsAndThereTrainingSquads[cell].Remove(squad);

        AddSquadInDictionary(squad, cell);
    }

    public void AddCellToGlobalList(GameObject cell) { cellsList.Add(cell); }
    public List<GameObject> GetGlobalList() { return cellsList; }


    //EnemyController field

    public int GetDefenceParameterFromCell(GameObject cell)
    {
        int result = 0;

        foreach(var squad in GetAllSquadsOnCell(cell))
        {
            result += squad.GetAllProtection();
        }

        return result;
    }
}
