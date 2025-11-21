using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;



public enum ControlSide
{
    none,
    enemys,
    allies
}


#if UNITY_EDITOR
public class MapLoader : EditorWindow
{
    [MenuItem("Tools/Load new map")]
    static void LoadMap()
    {
        return;
        //Ha ‚ÒˇÍËÈ
        ObjectDataHolder dataHolder = AssetDatabase.LoadAssetAtPath<ObjectDataHolder>("Assets/MapFolder/Map V1/Map_V1.asset");
        if(dataHolder == null) { Debug.Log("‘‡ÈÎ ÔÛÒÚ"); return; }

        GameObject gridTaker = GameObject.FindGameObjectWithTag("Grid");

        while (gridTaker.transform.childCount > 0)
        {
            DestroyImmediate(gridTaker.transform.GetChild(0).gameObject);
        }

        int index = 0;
        foreach (var data in dataHolder.savedObjects)
        {
            CreateObject(data, gridTaker.transform, index);
            index++;
        }
    }
    static void CreateObject(SavedObject data, Transform parent, int index)
    {
        var obj = new GameObject($"VoronoiCell({index})");
        obj.transform.SetParent(parent);
        obj.transform.localPosition = data.position;
        obj.transform.localRotation = data.rotation;
        obj.transform.localScale = data.scale;

        if (data.meshData != null)
        {
            var filter = obj.AddComponent<MeshFilter>();
            filter.sharedMesh = data.meshData.ToMesh();

            var renderer = obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = data.materials;

            if (data.hasCollider)
            {
                var collider = obj.AddComponent<MeshCollider>();
                collider.sharedMesh = filter.sharedMesh;
            }
        }
    }

    [MenuItem("Tools/Load map preset")]
    static void LoadPresetWithMenu()
    {
        BuildLoader.LoadPreset();
    }
   
    [MenuItem("Tools/Save working cells")]
    static void AddCellsToArray()
    {
        GameObject.Find("Random Event Master").GetComponent<EventController>().SetWorkingArray(Selection.gameObjects.ToArray());
    }
    /// <summary>
    /// 
    /// </summary>
}
#endif

public class BuildLoader
{
    private static string[] squadsId = new string[] {"InfantrySquad","EngineersSquad" };
    public static void LoadPreset()
    {
        ParametersCellsDataHolder parametersHolder = Resources.Load<ParametersCellsDataHolder>("CellsParameters_V1");
        if (parametersHolder == null) { Debug.Log("‘‡ÈÎ ÔÛÒÚ"); return; }

        var allCells = GameObject.FindGameObjectsWithTag("Interactable Cell");


        foreach (var cell in allCells)
        {
            var parameter = parametersHolder.GetPresetByGameObject(cell);

            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).SetParameters(cell);

            ConfigureCell(cell, parameter);
        }
        foreach(var cell in allCells)
        {
            var parameter = parametersHolder.GetPresetByGameObject(cell);
            CreateBuildings(cell, parameter);
        }
        foreach(var cell in allCells)
        {
            var parameter = parametersHolder.GetPresetByGameObject(cell);
            CreateSquads(cell, parameter);
        }

        AAlgorithm.SetAllCells(allCells.ToList());

        PauseScript.SetGameState(GameState.Play);
        Debug.Log("«¿√–”« ¿ œ–≈—≈“¿ «¿ ŒÕ◊≈ÕÕ¿.");

        ServiceRegistry.WorkWithService<EventBus>().Publish<MapLoader>(null);
    }
    static void ConfigureCell(GameObject cell, Parameters parameters)
    {
        if (parameters == null)
        {
            parameters = new Parameters()
            {
                controlSide = ControlSide.none,
                neighboresCells = new List<GameObject>(),
                height = 8,
                cellType = CellTypes_enum.Plain,
                buildings = new List<BuildData>()
            };
        }

        ServiceRegistry.WorkWithService<NavigationMesh>().AddCellToNavigationSystem(cell);

        if (parameters.height == 0 || parameters.height == null)
        {
            int newHeight = 8;
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellMovementParameters>().SetHeight(newHeight);
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellArea>().SetNeighbores(parameters.neighboresCells);
        }
        else
        {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellMovementParameters>().SetHeight(parameters.height);
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellArea>().SetNeighbores(parameters.neighboresCells);

            switch (parameters.controlSide) 
            {
                case ControlSide.enemys:
                    ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellArea>().SetSide_Simple(SideEnum.Allies); ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic(SideEnum.Allies, cell, false); break;
                case ControlSide.allies:
                    ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellArea>().SetSide_Simple(SideEnum.Enemys); ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic(SideEnum.Enemys, cell, false); break;
            }

        }

        if (parameters.cellType != CellTypes_enum.None) {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                ConfigurateCell_Type(parameters.cellType);
        }
        else
        {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                ConfigurateCell_Type(CellTypes_enum.Plain);
        }

        if (parameters.cellNameWhatPlayerSee == null || parameters.cellNameWhatPlayerSee == "")
        {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellDiscription>().SetBasicCellName();
        }
        else {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellDiscription>().SetCellName(parameters.cellNameWhatPlayerSee);
        }
    }
    static void CreateBuildings(GameObject cell, Parameters parameters)
    {
        if(parameters == null) return;

        if (parameters.isBase)
        {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
               GetParameter<CellBuildings>().IsCellBase();
        }

        for (int i = 0; i < parameters.buildings.Count; i++)
        {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellBuildings>().AddToBuildsList(
                (AbstractBuildings)ServiceRegistry.WorkWithService<ObjectFactory_Builds>().CreateObject(parameters.buildings[i].Id),
                parameters.buildings[i].Id
                );
        }

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellArea>().IsFrontCell = parameters.isFront;
    }
    static void CreateSquads(GameObject cell, Parameters parameters)
    {
        if (parameters == null) return;

        if (parameters.controlSide == ControlSide.enemys)
        {

            for (int i = 0; i < Random.Range(1, ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellSquadsOnArea>().GetCountCurrentMax().Item2); i++)
            {
                ServiceRegistry.WorkWithController<UnitsSpawner>().SpawnSquadOnCell(squadsId[Random.Range(0, squadsId.Length)], SideEnum.Allies, cell);
            }
        }
        if (parameters.controlSide == ControlSide.allies)
        {
            if (parameters.buildings.Count != 0) { return; }
                //ServiceRegistry.WorkWithController<EnemysController>().AddToCapturedCell_Safety(cell);
                for (int i = 0; i < Random.Range(1, ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellSquadsOnArea>().GetCountCurrentMax().Item2); i++)
            {
                ServiceRegistry.WorkWithController<UnitsSpawner>().SpawnSquadOnCell(squadsId[Random.Range(0, squadsId.Length)], SideEnum.Enemys, cell);
            }
        }
    }
}

public static class ServiceRegistry
{
    private static ControllersHub ControllersHub { get; set; }


    public static T WorkWithController<T>() { return ControllersHub.Get<T>(); }
    public static T WorkWithService<T>() { return ControllersHub.GetService<T>(); }

    public static void RegisterInterface(INeedTime inputInterface) { ControllersHub.AddINeedTimeInterfaceToList(inputInterface); }

    public static void Initialize()
    {
        ControllersHub = new ControllersHub();

        ControllersHub.StartLoadMap += () => BuildLoader.LoadPreset();

        ControllersHub.RegisterAllObjects();
    }
    public static void Start()
    {
        ControllersHub.Start();
    }
}