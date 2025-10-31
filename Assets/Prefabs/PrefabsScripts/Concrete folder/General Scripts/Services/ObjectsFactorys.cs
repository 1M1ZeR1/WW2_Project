using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Concrete realizations of factorys
/// </summary>
public class ObjectsFactory:FactoryOfGameObjects
{
    public void SaveObject<T>(GameObject objectToSave)
    {
        if (savedObjects.ContainsKey(typeof(T)))
        {
            savedObjects[typeof(T)].Add(objectToSave);
        }
        else
        {
            savedObjects.Add(typeof(T), new() { objectToSave});
        }
    }
    public override void StartFactory(){}
}

public class PanelFactory_BuildPanel : FactoryOfGameObjects,INeedTime
{
    public Action<object> Completed {  get; set; }

    protected bool _buildDataIsLoaded = false;

    public enum PanelType
    {
        Minimize,
        Normal,
        Maximize
    }

    private Transform[] prototypes;
    private Transform[] parents;
    private PanelType[] types;

    private List<string> keysToData = new();

    public override void StartFactory()
    {
        ServiceRegistry.RegisterInterface(this);

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<ObjectFactory_Builds>((sender) =>
        {
            _buildDataIsLoaded = true;
        });

        Debug.Log($"Starting factory:{this}");

        DataLoader.LoadArrayData<BuildPanelSO>(this, "BuildPanel");
        DataLoader.LoadedArrayData += (sender, array) =>
        {
            if (sender.GetType() == GetType()) 
            {
                foreach (var obj in array) 
                {
                    BuildPanelSO buildPanel = (BuildPanelSO)obj;
                    keysToData.Add($"{buildPanel.buildId}_panel");
                }

                SaveInDataHolder(array.Cast<BuildPanelSO>().ToList());
            }
        };
    }

    public void SetObjects(Transform[] prototypes, Transform[] parents, PanelType[] types)
    {
        this.prototypes = prototypes;
        this.parents = parents;
        this.types = types;
    }
    private void SaveInDataHolder(List<BuildPanelSO> allPresets)
    {
        Debug.Log($"Start to save objects in data holder:{this}");

        var dataholderObject = ServiceRegistry.WorkWithService<DataHolder>();

        foreach (var preset in allPresets)
        {
            Debug.Log($"{this}:Saving {preset} in data holder by id:{preset.buildId}_panel");
            dataholderObject.AddDataToHolder($"{preset.buildId}_panel", preset);
        }

        ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(CreatePanels());
    }
    private IEnumerator CreatePanels()
    {
        yield return new WaitUntil(()=>_buildDataIsLoaded);

        Debug.Log($"Start creating panels:{this}");

        for(int i =0;i<parents.Length;i++)
        {
            BuildPrototypePanel configurator;

            switch (types[i])
            {
                case PanelType.Minimize: configurator = prototypes[0].GetComponent<BuildPrototypePanel>(); break;
                case PanelType.Normal: configurator = prototypes[1].GetComponent<BuildPrototypePanel>(); break;
                case PanelType.Maximize: configurator = prototypes[2].GetComponent<BuildPrototypePanel>(); break;

                default: configurator = null; break;
            }

            foreach (string key in keysToData)
            {
                BuildPanelSO preset = (BuildPanelSO)ServiceRegistry.WorkWithService<DataHolder>().GetDataFromHolder(key);

                configurator.Clone(preset.buildName, preset.buildSprite,
                    ServiceRegistry.WorkWithController<ResourcesController>().GetBuildingCost(preset.buildId, 1),
                    ServiceRegistry.WorkWithController<ResourcesController>().GetBuildingCost(preset.buildId, 2),
                    preset.buildDescription, preset.buildId,
                    parents[i]);

                Debug.Log($"{this}:Create new panel by preset:{preset}");
            }
        }
        Completed?.Invoke(this);
    }
}
public class PanelFactory_SquadPanel : FactoryOfGameObjects,INeedTime
{
    public Action<object> Completed { get; set; }

    private TMP_Dropdown dropdown;

    private List<string> keys = new();

    public Dictionary<TMP_Dropdown.OptionData,string> OptionToId { get; set; } = new();

    public override void StartFactory()
    {
        ServiceRegistry.RegisterInterface(this);

        Debug.Log($"Starting factory:{this}");

        DataLoader.LoadArrayData<SquadData>(this, "SquadData");
        DataLoader.LoadedArrayData += (sender, array) =>
        {
            if (sender.GetType() == GetType())
            {
                foreach (var obj in array)
                {
                    SquadData squadData = (SquadData)obj;
                    keys.Add(squadData.id);
                }

                SaveInDataHolder(array.Cast<SquadData>().ToList());

                CreateLinks();
            }
        };
    }
    private void SaveInDataHolder(List<SquadData> allPresets)
    {
        Debug.Log($"Start to save objects in data holder:{this}");

        var dataholderObject = ServiceRegistry.WorkWithService<DataHolder>();

        foreach (var preset in allPresets)
        {
            Debug.Log($"{this}:Saving {preset} in data holder by id:{preset.id}");
            dataholderObject.AddDataToHolder($"{preset.id}", preset);
        }

        CreateLinks();
    }
    private void CreateLinks()
    {
        List<TMP_Dropdown.OptionData> options = new();

        foreach(var key in keys)
        {
            SquadData squadData = (SquadData)ServiceRegistry.WorkWithService<DataHolder>().GetDataFromHolder(key);

            var option = new TMP_Dropdown.OptionData(squadData.squadName, squadData.squadSprite);

            options.Add(option);

            OptionToId.Add(option, squadData.id);
        }

        dropdown.AddOptions(options);

        Completed?.Invoke(this);
    }
    public void SetObjects(TMP_Dropdown dropdown)
    {
        this.dropdown = dropdown;
    }
}

public class ObjectFactory_Builds : FactoryOfConcreteObjects,INeedTime
{
    public Action<object> Completed {  get; set; }

    protected Dictionary<string, AbstractBuildings> _idToBuildings = new();

    protected Dictionary<string, BuildData> _buildsData = new();

    public override object CreateObject(string id)
    {
        return _idToBuildings[id].Clone();
    }
    public override void StartFactory()
    {
        ServiceRegistry.RegisterInterface(this);

        Debug.Log($"Starting factory:{this}");

        DataLoader.LoadArrayData<BuildData>(this, "BuildData");
        DataLoader.LoadedArrayData += (sender, array) =>
        {
            if (sender.GetType() == GetType())
            {
                foreach (var obj in array)
                {
                    BuildData buildData = (BuildData)obj;
                    _buildsData.Add(buildData.Id, buildData);
                    ServiceRegistry.WorkWithService<DataHolder>().AddDataToHolder(buildData.Id, buildData);
                }

                ServiceRegistry.WorkWithService<EventBus>().Publish<ObjectFactory_Builds>(this);
                StartBuildingsInstance();
            }
        };
    }

    private void StartBuildingsInstance()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(AbstractBuildings)));

        Debug.Log($"{this}:Find {types.Count()} of subclasses");

        foreach (var type in types)
        {
            var attr = type.GetCustomAttributes(typeof(BuildingAttribute), false).FirstOrDefault() as BuildingAttribute;
            Debug.Log($"{this}:Work with {attr} attribute");

            _idToBuildings.Add(attr.Id,CreateInstance(attr.Id,type));

            Debug.Log($"{this}:Saving object with id:{attr.Id}. Count of saving data build is:{_idToBuildings.Count}");
        }

        Completed?.Invoke(this);
    }
    private AbstractBuildings CreateInstance(string id,Type buildType)
    {
        var createdInstance = (AbstractBuildings)Activator.CreateInstance(buildType);

        createdInstance.InstanceBuild(_buildsData[id].TimeToBuild, _buildsData[id].TimeToUpgrade, _buildsData[id].MaxLevel);

        return createdInstance;
    }
}
public class ObjectFactory_Squads:FactoryOfConcreteObjects
{
    private Dictionary<string, AbstractSquad> _idToSquad = new();

    public override object CreateObject(string id){ return _idToSquad[id].Clone(); }
    public override void StartFactory()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(AbstractSquad)));

        Debug.Log($"{this}:Find {types.Count()} of subclasses");

        foreach (var type in types)
        {
            var attr = type.GetCustomAttributes(typeof(SquadAttribute), false).FirstOrDefault() as SquadAttribute;
            Debug.Log($"{this}:Work with {attr} attribute");

            _idToSquad.Add(attr.Id, CreateInstance(type));
            Debug.Log($"{this}:Saving object with id:{attr.Id}. Count of saving data build is:{_idToSquad.Count}");
        }
    }

    private AbstractSquad CreateInstance(Type squadType)
    {
        var createdInstance = (AbstractSquad)Activator.CreateInstance(squadType);

        return createdInstance;
    }
}

/// <summary>
/// Interfaces of factorys
/// </summary>
public interface IFactory_GameObjects
{
    public GameObject CreateObject<T>(int index);
    public GameObject CreateObject<T>(Transform? parent = null);

}
public interface IFactory_Objects
{
    public object CreateObject(string id);
}
public interface IFactory_CanStart
{
    public void StartFactory();
}
/// <summary>
/// Abstract realization of factorys
/// </summary>
public abstract class Factory : IFactory_CanStart
{
    public Dictionary<Type, List<GameObject>> savedObjects { get; set; } = new();
    public abstract void StartFactory();
}
public abstract class FactoryOfGameObjects : Factory, IFactory_GameObjects
{
    public GameObject CreateObject<T>(int index)
    {
        if (!savedObjects.ContainsKey(typeof(T))) { return null; }
        else
        {
            return GameObject.Instantiate(savedObjects[typeof(T)][index]);
        }
    }

    public GameObject CreateObject<T>(Transform parent = null)
    {
        if (parent == null) return GameObject.Instantiate(savedObjects[typeof(T)][0]);
        else { return GameObject.Instantiate(savedObjects[typeof(T)][0], parent); }
    }
}
public abstract class FactoryOfConcreteObjects : Factory,IFactory_Objects
{
    public abstract object CreateObject(string id);
}
