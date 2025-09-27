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
        BuildLoader.LoadPreset(GameObject.Find("Enemys Master").GetComponent<EnemysController>());
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
    public static void LoadPreset(EnemysController enemysController)
    {
        ParametersCellsDataHolder parametersHolder = Resources.Load<ParametersCellsDataHolder>("CellsParameters_V1");
        if (parametersHolder == null) { Debug.Log("‘‡ÈÎ ÔÛÒÚ"); return; }

        var allCells = GameObject.FindGameObjectsWithTag("Interactable Cell");


        foreach (var cell in allCells)
        {
            var parameter = parametersHolder.GetPresetByGameObject(cell);

            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).SetParameters(cell);

            ConfigureCell(cell, parameter,enemysController);
        }

        AAlgorithm.SetAllCells(allCells.ToList());

        PauseScript.SetGameState(GameState.Play);
        Debug.Log("«¿√–”« ¿ œ–≈—≈“¿ «¿ ŒÕ◊≈ÕÕ¿.");
    }
    static void ConfigureCell(GameObject cell, Parameters parameters, EnemysController enemysController)
    {
        if (parameters == null)
        {
            parameters = new Parameters()
            {
                controlSide = ControlSide.none,
                neighboresCells = new List<GameObject>(),
                height = 8,
                cellType = CellTypes_enum.Plain,
                buildings = new List<BuildsEnum>()
            };
        }

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
                GetParameter<CellArea>().Side = SideEnum.Allies; ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic(SideEnum.Allies, cell, false); break;
                case ControlSide.allies:
                    ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellArea>().Side = SideEnum.Enemys; ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic(SideEnum.Enemys, cell, false); break;
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

        if (parameters.cellNameWhatPlayerSee == null)
        {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellDiscription>().SetBasicCellName();
        }
        else {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellDiscription>().SetCellName(parameters.cellNameWhatPlayerSee);
        }
        ;

        if (parameters.isBase) 
        {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
               GetParameter<CellBuildings>().IsCellBase();
        }

        for (int i = 0; i < parameters.buildings.Count; i++)
        {
            if (parameters.buildings[i] is BuildsEnum.Foxhole) {
                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellBuildings>().AddToBuildsList(new FortBuild(), BuildsEnum.Foxhole); }
            if (parameters.buildings[i] is BuildsEnum.Camp) {
                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellBuildings>().AddToBuildsList(new CampBuild(), BuildsEnum.Camp); }
            if (parameters.buildings[i] is BuildsEnum.MilitaryAcademy) {
                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellBuildings>().AddToBuildsList(new FortBuild(), BuildsEnum.Fort); }
        }


        if (parameters.controlSide == ControlSide.enemys)
        {

            for (int i = 0; i < Random.Range(1, ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellSquadsOnArea>().GetCountCurrentMax().Item2); i++)
            {
                ServiceRegistry.WorkWithController<UnitsSpawner>().SpawnSquadOnCell(SideEnum.Allies, cell);
            }
        }
        if (parameters.controlSide == ControlSide.allies)
        {
            enemysController.AddToCapturedCell_Safety(cell);
            for (int i = 0; i < Random.Range(1, ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellSquadsOnArea>().GetCountCurrentMax().Item2); i++)
            {
                ServiceRegistry.WorkWithController<UnitsSpawner>().SpawnSquadOnCell(SideEnum.Enemys, cell);
            }
        }
    }
}

public static class ServiceRegistry
{
    private static ControllersHub ControllersHub { get; set; }

    public static T WorkWithController<T>() { return ControllersHub.Get<T>(); }

    public static void Initialize()
    {
        ControllersHub = new ControllersHub();
    }
}