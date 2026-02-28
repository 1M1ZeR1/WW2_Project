using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class ControllersHub 
{
    public Action StartLoadMap;

    protected bool _servicesLoaded = false;

    private readonly Dictionary<Type, object> controllers = new();
    private readonly Dictionary<Type, object> services = new();

    private readonly Queue<INeedTime> queueToLoad_Services = new();

    protected int _countOfRegisterInterfaces = 0;
    protected List<object> _registerInterfaces = new List<object>();

    private void Register_Controller<T>(T controller)
    {
        controllers[typeof(T)] = controller;
    }
    private void Register_Service<T>(T service)
    {
        services[typeof(T)] = service;

        if(service.GetType() == typeof(INeedTime)) queueToLoad_Services.Enqueue((INeedTime)service);
    }

    public void AddINeedTimeInterfaceToList(INeedTime inputInterface)
    {
        Debug.Log($"Registered new interface, count of registered interfaces:{_countOfRegisterInterfaces}");
        _countOfRegisterInterfaces++;

        _registerInterfaces.Add(inputInterface);

        inputInterface.Completed += (sender) => { _registerInterfaces.Remove(sender); Debug.Log($"Loaded interface, {_countOfRegisterInterfaces-_registerInterfaces.Count}/{_countOfRegisterInterfaces} loaded"); };
    }
    public System.Collections.IEnumerator LoadWaiter()
    {
        yield return new WaitUntil(()=>_registerInterfaces.Count == 0 && _servicesLoaded);

        StartLoadMap?.Invoke();
    }


    public T Get<T>()
    {
        return (T)controllers[typeof(T)];
    }
    public T GetService<T>()
    {
        return (T)services[typeof(T)];
    }

    public void Start()
    {
        Get<ResourcesController>().Start();
        Get<GameController>().Start();
        Get<EnemysController>().Start();
        Get<MovementController>().Start();
    }

    public void RegisterAllObjects()
    {
        RegisterServices();
        RegisterControllers();

        GetService<MonobehaviourMaster>().CoroutineStarter(LoadWaiter());

    }
    public void RegisterServices()
    {
        Register_Service(new EventBus());
        Register_Service(new CommandBus());

        Register_Service(new NavigationMesh());

        Register_Service(new DataHolder());
        Register_Service(GameObject.FindAnyObjectByType<MonobehaviourMaster>());
        Register_Service(new ObjectsFactory());

        Register_Service(new ObjectFactory_Builds());
        GetService<ObjectFactory_Builds>().StartFactory();

        Register_Service(new ObjectFactory_Squads());
        GetService<ObjectFactory_Squads>().StartFactory();

        Register_Service(new PanelFactory_SquadPanel());

        Register_Service(new PanelFactory_BuildPanel());

        _servicesLoaded = true;

        GameObject.FindAnyObjectByType<TimeControllerScript>().StartTime();
    }

    public void RegisterControllers()
    {
        Register_Controller(GameObject.FindAnyObjectByType<CellUIScript>());

        Register_Controller(GameObject.FindAnyObjectByType<FocusOnCellScript>());

        Register_Controller(GameObject.FindAnyObjectByType<ChoosingScript>());

        Register_Controller(new AlliesSpawner());

        Register_Controller(new GameController());

        Register_Controller(new BattleController());

        Register_Controller(new MovementController());

        Register_Controller(GameObject.FindAnyObjectByType<ArrowCanvasScript>());

        Register_Controller(new UnitsSpawner());

        Register_Controller(new AAlgorithm());

        Register_Controller(new CellController());

        Register_Controller(new CellInteraction());

        Register_Controller(new ResourcesController());


        Register_Controller(GameObject.FindAnyObjectByType<MessageScript>(FindObjectsInactive.Include));
        Register_Controller(GameObject.FindAnyObjectByType<WorldOnCanvasScript>());

        Register_Controller(new BuilderController());

        Register_Controller(GameObject.FindAnyObjectByType<InteractableScript>());
        Register_Controller(new EnemysController());

        Register_Controller(GameObject.FindAnyObjectByType<TimeControllerScript>());
        Register_Controller(new BuffsController());

        Register_Controller(GameObject.FindAnyObjectByType<ExplorationController>());
    }

    //private IEnumerator ServicesLoader()
    //{

    //}
}

