using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class CellController
{
    private Dictionary<Type, object> cellsAndTheirsScripts = new()
    {
        { typeof(CellParametersHandler),new Dictionary<GameObject, object>()},
    };

    public T WorkWithCell<T>(GameObject cell) where T : new()
    {
        if (cellsAndTheirsScripts.TryGetValue(typeof(T), out var dictObj))
        {
            var dict = dictObj as Dictionary<GameObject, object>;

            if (dict.TryGetValue(cell, out var script))
            {
                return (T)script;
            }
            else
            {
                dict.Add(cell, new T());
                return (T)dict[cell];
            }
        }

        return default;
    }

    public List<GameObject> FastDrop_GetNeighbores(GameObject cell) { return WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().GetNeighbores(); }
    public SideEnum FastDrop_CellSide(GameObject cell) { return WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().Side; }
}

public class CellParametersHandler:ICellParser,IParametersHandlerParser
{
    private Dictionary<Type, object> savedParameters = new();

    private GameObject currentCell;

    public void SetParameters(GameObject cell)
    {
        currentCell = cell;

        savedParameters.Add(typeof(CellMovementParameters), new CellMovementParameters());
        savedParameters.Add(typeof(CellType), new CellType(this));
        savedParameters.Add(typeof(CellBuffs), new CellBuffs(this));
        savedParameters.Add(typeof(CellDiscription), new CellDiscription(this));
        savedParameters.Add(typeof(CellBuildings), new CellBuildings(this));
        CellAreaRegistry();
        savedParameters.Add(typeof(CellSquadsOnArea), new CellSquadsOnArea(this));
    }
    protected void CellAreaRegistry()
    {
        var newCellAreaObject = new CellArea(this,this);

        savedParameters.Add(typeof(CellArea), newCellAreaObject);
        savedParameters.Add(typeof(ISide), newCellAreaObject);
    }

    public T GetParameter<T>()
    {
        if (savedParameters.TryGetValue(typeof(T), out var obj))
        {
            return (T)obj;
        }
        return default;
    }


    public void ConfigurateCell_Type(CellTypes_Enum type)
    {
        foreach(var obj in savedParameters.Values)
        {
            if(obj is ICellNeeder_Type typeConfigurator)
            {
                typeConfigurator.SetType(type);
            }
        }
    }

    public GameObject GetCellWorkWith() { return currentCell; }
    public CellParametersHandler GetCellParametersHandler() { return this; }
}


//Внутренние классы
public class CellType:ICellNeeder_Type
{
    private AbstractCell cellClass;

    private ICellParser cellParser;

    protected bool _hasEventBuff = false;


    public CellType(ICellParser cellParser) { this.cellParser = cellParser; }


    public void SetType(CellTypes_Enum type)
    {
        cellClass = type switch
        {
            CellTypes_Enum.Plain => new PlainCell(20),
            CellTypes_Enum.Forest => new ForestCell(30),
            CellTypes_Enum.City => new CityCell(10),
            CellTypes_Enum.River => new RiverCell(),
            CellTypes_Enum.Beach => new BeachCell(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        ServiceRegistry.WorkWithController<CellController>().
            WorkWithCell<CellParametersHandler>(cellParser.GetCellWorkWith()).GetParameter<CellBuffs>().
            SetStartBuff(cellClass.GetBuff());

        ServiceRegistry.WorkWithController<CellController>().
            WorkWithCell<CellParametersHandler>(cellParser.GetCellWorkWith()).GetParameter<CellMovementParameters>().
            SetType(type);

        CellTypeObject = type;
    }

    public CellTypes_Enum CellTypeObject { private set; get; }
    public AbstractCell ConntectWithAbstractCell() { return cellClass; }

    public bool IsWithEventBuff() { return _hasEventBuff; }
    public void SetWithEventBuff(bool state) { _hasEventBuff = state; }
}
public class CellBuffs
{
    private List<AbstractBuffs> buffsOnCell = new();

    private ICellParser cellParser;

    public CellBuffs(ICellParser cellParser) { this.cellParser = cellParser; }

    public void SetStartBuff(AbstractBuffs buff) { buffsOnCell.Add(buff); }

    public void AddBuffWithTimer(AbstractBuffs buff)
    {
        CustomLog.RedText($"Added new buff to {cellParser.GetCellWorkWith().name}");

        buffsOnCell.Add(buff);
        UpdateBuffsForSquad();

        if (buff is IEventBuff eventBuff)
        {
           eventBuff.Execute();
        }

        buff.RequestToClear += BuffRemoverListener;
    }
    private void BuffRemoverListener(AbstractBuffs buff)
    {
        buffsOnCell.Remove(buff);

        if (buff is IEventBuff eventBuff)
        {
            eventBuff.Remove();
        }
    }

    public void SetToSquadBuffs(AbstractSquad squad)
    {
        foreach (var buff in buffsOnCell)
        {
            squad.AddBuff(buff);
        }
    }
    public void RemoveFromSquadBuffs(AbstractSquad squad)
    {
        foreach (var buff in buffsOnCell)
        {
            squad.RemoveBuff(buff);
        }
    }

    private void UpdateBuffsForSquad()
    {
        foreach (var squad in ServiceRegistry.WorkWithController<GameController>().GetAllSquadsOnCell(cellParser.GetCellWorkWith()))
        {
            var allBuffs = squad.GetAllSquadBuffs();

            foreach (var cellBuff in buffsOnCell)
            {
                if (!allBuffs.Contains(cellBuff) && cellBuff != null)
                {
                    squad.AddBuff(cellBuff);
                }
            }
        }
    }
    private void RemoveUpdateBuffsForSquad(AbstractBuffs buff)
    {
        foreach (var squad in ServiceRegistry.WorkWithController<GameController>().GetAllSquadsOnCell(cellParser.GetCellWorkWith()))
        {
            if (squad.GetAllSquadBuffs().Contains(buff)) { squad.RemoveBuff(buff); }
        }
    }

    public void AddBuffToCell(AbstractBuffs buff)
    {
        if (buff == null) { return; }
        buffsOnCell.Add(buff);
        UpdateBuffsForSquad();
    }
    public void RemoveBuffFromCell(AbstractBuffs buff)
    {
        RemoveUpdateBuffsForSquad(buff);
        buffsOnCell.Remove(buff);
    }

    public List<AbstractBuffs> GetAllCellBuffs() { return buffsOnCell; }
}
public class CellMovementParameters
{
    private float height;
    private float costToMove;

    public void SetType(CellTypes_Enum type)
    {
        costToMove = type switch
        {
            CellTypes_Enum.Plain => 0.5f,
            CellTypes_Enum.Forest => 1f,
            CellTypes_Enum.City => 10f,
            CellTypes_Enum.River => 5f,
            CellTypes_Enum.Beach => 10f,
            _ => 0f
        };
    }

    public float GetCost() { return costToMove; }

    public float GetHeight() { return height; }
    public void SetHeight(float height ) { this.height = height; }
}

public class CellDiscription
{
    private ICellParser cellParser;
    private string cellType = "";

    public CellDiscription(ICellParser cellParser)
    {
        this.cellParser = cellParser;
    }

    private string cellName;


    public void SetCellName(string name) { cellName = name; cellType = ServiceRegistry.WorkWithController<CellController>().
            WorkWithCell<CellParametersHandler>(cellParser.GetCellWorkWith()).GetParameter<CellType>().
            ConntectWithAbstractCell().GetTypeCell();
    }
    public string GetCellName() { return cellName; }
    public string GetCellType() { return cellType; }

    public void SetBasicCellName() {
        cellName = $"Неизвестная {ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellParser.GetCellWorkWith()).GetParameter<CellType>().ConntectWithAbstractCell().GetTypeCell()}";

        cellType = ServiceRegistry.WorkWithController<CellController>().
            WorkWithCell<CellParametersHandler>(cellParser.GetCellWorkWith()).GetParameter<CellType>().
            ConntectWithAbstractCell().GetTypeCell();
    }

    public string GetCurrentMaxCountOfSquads_String()
    {
        var information = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellParser.GetCellWorkWith()).GetParameter<CellSquadsOnArea>().GetCountCurrentMax();

        return $"{information.Item1}/{information.Item2}";
    }
}
public class CellArea:ISide
{
    private bool isFrontCell = false;
    public bool IsFrontCell 
    {
        get 
        {
            return isFrontCell;
        }
        set 
        {
            if (value && !isFrontCell) {
                ServiceRegistry.WorkWithController<CellController>()
                                    .WorkWithCell<CellParametersHandler>(_cellParser.GetCellWorkWith())
                                    .GetParameter<CellSquadsOnArea>().
                                    bonusFront = 5;
            }
            else if(!value && isFrontCell)
            {
                ServiceRegistry.WorkWithController<CellController>()
                                    .WorkWithCell<CellParametersHandler>(_cellParser.GetCellWorkWith())
                                    .GetParameter<CellSquadsOnArea>().
                                    bonusFront = 0;
            }

            isFrontCell = value;
        } 
    }    

    protected IParametersHandlerParser _parser;

    protected ICellParser _cellParser;

    public Action<GameObject> cellChangedSide;

    public CellArea(IParametersHandlerParser parser, ICellParser cellParser)
    {
        _parser = parser;
        _cellParser = cellParser;
    }


    private SideEnum _side = SideEnum.None;
    public SideEnum Side
    {
        get { return _side;}
        set {
            if (value != _side) 
            {
                _side = value; 
                cellChangedSide?.Invoke(_cellParser.GetCellWorkWith());

                ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic
                    (
                    _side,
                    _cellParser.GetCellWorkWith(),
                    false
                    );
                FrontChecker();
            } }
    }
    public void SetSide_Simple(SideEnum side) { _side = side; }


    private List<GameObject> cellNeighbores = new List<GameObject>();

    public bool IsAllies()
    {
        if (Side == SideEnum.Enemys) return false;
        return true;
    }
    public void RequestToControlCell(SideEnum whatSide)
    {
        if (whatSide == Side) return;

        if(_parser.GetCellParametersHandler().GetParameter<CellSquadsOnArea>().GetCountCurrentMax().Item1 != 0) { return; }

        Side = whatSide;

        ServiceRegistry.WorkWithService<EventBus>().Publish<GameObject, SideEnum, CellController>(_cellParser.GetCellWorkWith(), whatSide, null);
    }
    public void RequestToControlCell_Hard(SideEnum whatSide)
    {
        if (whatSide == Side) return;

        Side = whatSide;

        ServiceRegistry.WorkWithService<EventBus>().Publish<GameObject, SideEnum, CellController>(_cellParser.GetCellWorkWith(), whatSide, null);
    }

    public GameObject GetNeighbor(int index) { return cellNeighbores[index]; }
    public int GetCountOfNeighbores() { return cellNeighbores.Count; }
    public bool IsCellNeighbor(GameObject cell)
    {
        return cellNeighbores.Contains(cell);
    }
    public List<GameObject> GetNeighbores() { return cellNeighbores; }
    public void SetNeighbores(List<GameObject> cells) { cellNeighbores = cells; }


    public List<GameObject> NeighboresSearcher(int deepCount)
    {
        List<GameObject> result = cellNeighbores.ToList();
        result.Add(_cellParser.GetCellWorkWith());
        deepCount--;
        if(deepCount == 0) return result;

        foreach(var cell in cellNeighbores)
        {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().NeibhoresSeracher_Node(result, deepCount);
        }

        result.Distinct().ToList();

        return result;
    }
    private void NeibhoresSeracher_Node(List<GameObject>cellsInList, int deepCount)
    {
        deepCount--;

        cellsInList.AddRange(cellNeighbores);

        if(deepCount == 0) return;

        foreach(GameObject cell in cellNeighbores)
        {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().NeibhoresSeracher_Node(cellsInList,deepCount);
        }
    }

    private void FrontChecker()
    {
        bool otherSideCellFinded = false;

        foreach(var cell in cellNeighbores)
        {
            if(ServiceRegistry.WorkWithController<CellController>().FastDrop_CellSide(cell) != Side && ServiceRegistry.WorkWithController<CellController>().FastDrop_CellSide(cell)!=SideEnum.None)
            {
                otherSideCellFinded = true;
                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().IsFrontCell = true;
                continue;
            }
            else { ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().IsFrontCell = false; }

            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().FrontChecker_Single();
        }

        if (otherSideCellFinded) IsFrontCell = true;
    }
    public void FrontChecker_Single()
    {
        if (_side == SideEnum.None) return;
        bool otherSideCellFinded = false;

        foreach (var cell in cellNeighbores)
        {

            if (ServiceRegistry.WorkWithController<CellController>().FastDrop_CellSide(cell) != Side && ServiceRegistry.WorkWithController<CellController>().FastDrop_CellSide(cell) != SideEnum.None)
            {
                otherSideCellFinded = true;
            }
        }
        IsFrontCell = otherSideCellFinded;
    }

    private bool PermittedSide(SideEnum cellSide, SideEnum cellNeighboreSide)
    {
        switch (cellSide)
        {
            case SideEnum.Allies: return cellNeighboreSide != SideEnum.Enemys;
            case SideEnum.Enemys: return cellNeighboreSide != SideEnum.Allies;
        }

        return false;
    }
}
public class CellBuildings
{
    private ICellParser cellParser;

    public CellBuildings(ICellParser cellParser) { this.cellParser = cellParser; }

    public Dictionary<string, AbstractBuildings> builds { get; private set; } = new();

    public string buildInBuilding { get; set; }

    public void AddToBuildList_Safely(AbstractBuildings building, string id)
    {
        if (builds.ContainsKey(id))
        {
            builds[id] = building;

            //Возможен баг, дублирование эффектов.
        }
        else
        {
            AddToBuildsList(building, id);
        }
    }
    public void AddToBuildsList(AbstractBuildings building, string id)
    {
        builds.Add(id, building);
        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellParser.GetCellWorkWith()).
                GetParameter<CellBuffs>().AddBuffToCell(building.Buff);

        if (building.GetType() == typeof(HeadquartersBuild))
        {
            ((HeadquartersBuild)building).CellWithThisBuild = cellParser.GetCellWorkWith().transform;

            HeadquartersAdder_EnemyController((HeadquartersBuild)building, cellParser.GetCellWorkWith());
        }

        building.ActivateBuild();

        ServiceRegistry.WorkWithService<EventBus>().Publish<CellBuildings, GameObject, AbstractBuildings>(this, cellParser.GetCellWorkWith(), building);

        if (!ServiceRegistry.WorkWithService<CellAccessibilityValidator>().InteractWithAlliesCell(cellParser.GetCellWorkWith()) && building.GetType() == typeof(HeadquartersBuild))
        {
            ServiceRegistry.WorkWithController<EnemysController>().HeadquartersDangerPoints.Add((HeadquartersBuild)building, 0);
        }
    }
    private void HeadquartersAdder_EnemyController(HeadquartersBuild build, GameObject cell)
    {
        if (ServiceRegistry.WorkWithService<CellAccessibilityValidator>().InteractWithAlliesCell(cellParser.GetCellWorkWith()))
        {
            ServiceRegistry.WorkWithController<EnemysController>().CellWithHeadquarters_Player.Add(cell, build);
        }
        else { ServiceRegistry.WorkWithController<EnemysController>().CellWithHeadquarters_Bot.Add(cell, build);
            ServiceRegistry.WorkWithController<EnemysController>().HeadquartersCasing.Add(build, null);
        }
    }


    public void DebugFunction_NameAllBuildings()
    {
        foreach (var build in builds)
        {
            if (build.Value != null) { Debug.Log($"{build.Key} - построено"); }
            else { Debug.Log($"{build.Key} - не построено"); }
        }
    }
    public bool CheckBuildIsBuilt(string id)
    {
        if (builds.ContainsKey(id))
        {
            if (builds[id] == null) { return false; }
            else { return true; }
        }
        else { return false; }
    }

    public int GetCountOfBuildings() { return builds.Count; }
    public string[] GetNamesOfBuildings()
    {
        List<string> names = new List<string>();

        foreach (var build in builds.Values)
        {
            names.Add(build.Name);
        }

        return names.ToArray();
    }
    public AbstractBuildings GetNewBuilding(List<AbstractBuildings> currentBuildings)
    {
        foreach (var build in builds.Values)
        {
            if (!currentBuildings.Contains(build)) { return build; }
        }

        return null;
    }

    private bool cellIsEnemyBase = false;
    public void IsCellBase() { cellIsEnemyBase = true; }

    public AbstractBuildings GetRandomBuild(List<System.Object> objects)
    {
        if (builds.Count == 0) { return null; }

        foreach (var build in builds.Values)
        {
            if (!objects.Contains(build)) { return build; }
        }

        return null;
    }
}
public class CellSquadsOnArea : ICellNeeder_Type
{
    private ICellParser cellParser;

    public CellSquadsOnArea(ICellParser cellParser) { this.cellParser = cellParser; }

    public List<AbstractSquad> squadsOnCell { get; private set; } = new();

    public Action<GameObject, AbstractSquad, bool> SquadState; 


    private int currentCount = 0;
    private int phantomCount = 0;
    public int maxCountOfSquads { get; private set; }
    public int bonusHarden { private get; set; } = 0;

    public int bonusFront { private get; set; } = 0;



    private int _bonusHeadquarters = 0;
    public int BonusHeadquarters
    {
        get
        {
            return _bonusHeadquarters;
        }
        set { _bonusHeadquarters = value; }
    }

    public void SetType(CellTypes_Enum cellType)
    {
        switch (cellType)
        {
            case CellTypes_Enum.Plain: maxCountOfSquads = 3; break;
            case CellTypes_Enum.Forest: maxCountOfSquads = 2; break;
            case CellTypes_Enum.City: maxCountOfSquads = 5; break;

            default: maxCountOfSquads = 1; break;
        }

    }
    public void SwitchSquad(AbstractSquad squad, GameObject toCell)
    {
        TryRemoveSquad(squad);

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(toCell).GetParameter<CellSquadsOnArea>().TryAddSquad(squad);
    }

    public (int, int) GetCountCurrentMax()
    {
        return (currentCount + phantomCount, maxCountOfSquads + BonusHeadquarters + bonusHarden + bonusFront);
    }

    public bool TryAddSquad(AbstractSquad squad)
    {
        if (!squadsOnCell.Contains(squad))
        {
            squadsOnCell.Add(squad);
            currentCount = squadsOnCell.Count;

            SquadState?.Invoke(cellParser.GetCellWorkWith(), squad, true);

            return true;
        }
        return false;
    }

    public bool TryRemoveSquad(AbstractSquad squad)
    {
        if (squadsOnCell.Contains(squad))
        {
            squadsOnCell.Remove(squad);
            currentCount = squadsOnCell.Count;

            SquadState?.Invoke(cellParser.GetCellWorkWith(), squad, false);

            return true;
        }
        return false;
    }

    public void ChangePhantomCount(bool increase)
    {
        if (increase) { phantomCount++; }
        else { phantomCount--; }
    }

    public void SwitchCountSquad(AbstractSquad squad, GameObject toCell)
    {
        phantomCount--;

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(toCell).
            GetParameter<CellSquadsOnArea>().phantomCount++;
    }
    public void AddCountOfSquad() { currentCount++; }
}

public interface ICellNeeder_Type
    {
        public void SetType(CellTypes_Enum type);
    }
    public interface ICellParser
    {
        public GameObject GetCellWorkWith();
    }
    public interface IParametersHandlerParser
    {
        public CellParametersHandler GetCellParametersHandler();
    }
