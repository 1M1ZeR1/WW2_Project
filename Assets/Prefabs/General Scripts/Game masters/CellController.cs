using System;
using System.Collections;
using System.Collections.Generic;
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

    public void AssetHelper(SideEnum sideEnum, GameObject cell)
    {
        ServiceRegistry.WorkWithController<UnitsSpawner>().SpawnSquadOnCell(sideEnum, cell);
    }

    public List<GameObject> FastDrop_GetNeighbores(GameObject cell) { return WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().GetNeighbores(); }
    public bool FastDrop_IsAllies(GameObject cell) { return WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().IsAllies(); }
}

public class CellParametersHandler:ICellParser,IParametersHandlerParser
{
    private Dictionary<Type, object> savedParameters = new();

    private GameObject currentCell;

    public void SetParameters(GameObject cell)
    {
        currentCell = cell;


        savedParameters.Add(typeof(CellType), new CellType(this));
        savedParameters.Add(typeof(CellBuffs), new CellBuffs(this));
        savedParameters.Add(typeof(CellMovementParameters), new CellMovementParameters());
        savedParameters.Add(typeof(CellDiscription), new CellDiscription(this));
        savedParameters.Add(typeof(CellBuildings), new CellBuildings(this));
        CellAreaRegistry();
        savedParameters.Add(typeof(CellSquadsOnArea), new CellSquadsOnArea());


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


    public void ConfigurateCell_Type(CellTypes_enum type)
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


    public void SetType(CellTypes_enum type)
    {
        cellClass = type switch
        {
            CellTypes_enum.Plain => new PlainCell(20),
            CellTypes_enum.Forest => new ForestCell(30),
            CellTypes_enum.City => new CityCell(10),
            CellTypes_enum.River => new RiverCell(),
            CellTypes_enum.Beach => new BeachCell(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        ServiceRegistry.WorkWithController<CellController>().
            WorkWithCell<CellParametersHandler>(cellParser.GetCellWorkWith()).GetParameter<CellBuffs>().
            SetStartBuff(cellClass.GetBuff());
    }
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

    public void SetType(CellTypes_enum type)
    {
        costToMove = type switch
        {
            CellTypes_enum.Plain => 0.5f,
            CellTypes_enum.Forest => 1f,
            CellTypes_enum.City => 10f,
            CellTypes_enum.River => 5f,
            CellTypes_enum.Beach => 10f,
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
        set { _side = value; cellChangedSide?.Invoke(_cellParser.GetCellWorkWith()); }
    }

    private List<GameObject> cellNeighbores = new List<GameObject>();

    public bool IsAllies()
    {
        if (Side == SideEnum.Enemys) return false;
        return true;
    }
    public void RequestToControlCell(SideEnum whatSide)
    {
        Side = whatSide;
    }

    public GameObject GetNeighbor(int index) { return cellNeighbores[index]; }
    public int GetCountOfNeighbores() { return cellNeighbores.Count; }
    public bool IsCellNeighbor(GameObject cell)
    {
        if (cellNeighbores.Contains(cell)) return true;
        return false;
    }
    public List<GameObject> GetNeighbores() { return cellNeighbores; }
    public void SetNeighbores(List<GameObject> cells) { cellNeighbores = cells; }
}
public class CellBuildings
{
    private ICellParser cellParser;

    public CellBuildings(ICellParser cellParser) { this.cellParser = cellParser; }

    public Dictionary<BuildsEnum, AbstractBuildings> builds { get; private set; } = new();

    public Dictionary<BuildsEnum, bool> beenBuildingBuilt { get; private set; } = new()
    {
        
        {BuildsEnum.Foxhole,false},
        {BuildsEnum.HeadQuarters,false},
        {BuildsEnum.Camp, false},
        {BuildsEnum.Fort, false},
        {BuildsEnum.Bridge,false},
        {BuildsEnum.MilitaryAcademy, false},
    };

    public void AddToBuildList_Safely(AbstractBuildings building, BuildsEnum buildType)
    {
        if (builds.ContainsKey(buildType))
        {
            builds[buildType] = building;
            ChangeDictionaryState(building);

            //Возможен баг, дублирование эффектов.
        }
        else
        {
            AddToBuildsList(building, buildType);
        }
    }
    public void AddToBuildsList(AbstractBuildings building, BuildsEnum buildType)
    {
        builds.Add(buildType, building); ChangeDictionaryState(building);
        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellParser.GetCellWorkWith()).
                GetParameter<CellBuffs>().AddBuffToCell(building.Buff);
    }
    public void RemoveFromBuildsList(AbstractBuildings building, BuildsEnum buildType) { builds.Remove(buildType); ChangeDictionaryState(building); 
        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellParser.GetCellWorkWith()).
                GetParameter<CellBuffs>().RemoveBuffFromCell(building.Buff);
    }

    private void ChangeDictionaryState(IType<BuildsEnum> building)
    {
        if (building.GetType() == typeof(HeadquartersBuild))
        {
            if (beenBuildingBuilt[BuildsEnum.HeadQuarters]) { beenBuildingBuilt[BuildsEnum.HeadQuarters] = false; }
            else beenBuildingBuilt[BuildsEnum.HeadQuarters] = true;
        }
        if (building.GetType() == typeof(CampBuild))
        {
            if (beenBuildingBuilt[BuildsEnum.Camp]) { beenBuildingBuilt[BuildsEnum.Camp] = false; }
            else beenBuildingBuilt[BuildsEnum.Camp] = true;
        }
        if (building.GetType() == typeof(FortBuild))
        {
            if (beenBuildingBuilt[BuildsEnum.Fort]) { beenBuildingBuilt[BuildsEnum.Fort] = false; }
            else beenBuildingBuilt[BuildsEnum.Fort] = true;
        }
        if (building.GetType() == typeof(MilitaryAcademy))
        {
            if (beenBuildingBuilt[BuildsEnum.MilitaryAcademy]) { beenBuildingBuilt[BuildsEnum.MilitaryAcademy] = false; }
            else beenBuildingBuilt[BuildsEnum.MilitaryAcademy] = true;
        }
        if (building.GetType() == typeof(FoxholeBuild))
        {
            if (beenBuildingBuilt[BuildsEnum.Foxhole]) { beenBuildingBuilt[BuildsEnum.Foxhole] = false; }
            else beenBuildingBuilt[BuildsEnum.Foxhole] = true;
        }
    }

    public void DebugFunction_NameAllBuildings()
    {
        foreach (var build in beenBuildingBuilt)
        {
            if (build.Value) { Debug.Log($"{build.Key} - построено"); }
            else { Debug.Log($"{build.Key} - не построено"); }
        }
    }
    public bool CheckBuildIsBuilt(BuildsEnum buildType)
    {
        //Debug.Log($"{buildType} - {beenBuildingBuilt[buildType]}");
        return beenBuildingBuilt[buildType];
    }
    public List<bool> GetBuildingsStates()
    {
        List<bool> result = new List<bool>();

        result.Add(CheckBuildIsBuilt(BuildsEnum.Camp));
        if (!result[0]) result.Add(false);
        else
        {
            if (builds[BuildsEnum.Camp].IsUpgraded()) { result.Add(true); }
            else { result.Add(false); }
        }

        result.Add(CheckBuildIsBuilt(BuildsEnum.Fort));
        if (!result[2]) result.Add(false);
        else
        {
            if (builds[BuildsEnum.Fort].IsUpgraded()) { result.Add(true); }
            else { result.Add(false); }
        }

        result.Add(CheckBuildIsBuilt(BuildsEnum.MilitaryAcademy));
        if (!result[4]) result.Add(false);
        else
        {
            if (builds[BuildsEnum.MilitaryAcademy].IsUpgraded()) { result.Add(true); }
            else { result.Add(false); }
        }

        return result;
    }
    public void UpgradeBuildBuff(BuildsEnum buildType)
    {
        builds[buildType].UpgradeBuild();
    }
    public AbstractBuildings GetBuildByType(BuildsEnum type) { return builds[type]; }

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
    public void IsCellBase() {  cellIsEnemyBase = true; }
}
public class CellSquadsOnArea:ICellNeeder_Type
{
    public List<AbstractSquad> squadsOnCell { get; private set; } = new();

    public int maxCountOfSquads { get; private set; }
    public int bonusHarden { private get; set; } = 0;



    private int _bonusHeadquarters = 0;
    public int BonusHeadquarters
    {
        private  get 
        {
            return _bonusHeadquarters; 
        }
        set { _bonusHeadquarters = value; CheckCount(); }
    }

    public void SetType(CellTypes_enum cellType)
    {
        switch (cellType) 
        {
            case CellTypes_enum.Plain: maxCountOfSquads = 3;break;
            case CellTypes_enum.Forest: maxCountOfSquads = 2;break;
            case CellTypes_enum.City: maxCountOfSquads = 5;break;

            default: maxCountOfSquads = 1;break;
        }

    }

    private void CheckCount()
    {
        
    }

    private bool ReCheckSqaudsCount()
    {
        if(squadsOnCell.Count < maxCountOfSquads)
        {
            //алгоритм переселения

            return false;
        }

        return true;
    }

    public (int,int) GetCountCurrentMax()
    {
        return (squadsOnCell.Count, maxCountOfSquads + BonusHeadquarters + bonusHarden);
    }
}

public interface ICellNeeder_Type
{
    public void SetType(CellTypes_enum type);
}
public interface ICellParser
{
    public GameObject GetCellWorkWith();
}
public interface IParametersHandlerParser
{
    public CellParametersHandler GetCellParametersHandler();
}
