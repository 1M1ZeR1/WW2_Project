using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ControllersHub 
{
    private readonly Dictionary<Type, object> controllers = new();
    private readonly Dictionary<Type, object> services = new();

    private void Register_Controller<T>(T controller)
    {
        controllers[typeof(T)] = controller;
    }
    private void Register_Service<T>(T service)
    {
        services[typeof(T)] = service;
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

    public void RegisterServices()
    {
        Register_Service(new DataHolder());
        Register_Service(GameObject.FindAnyObjectByType<MonobehaviourMaster>());
        Register_Service(new ObjectsFactory());

        Register_Service(new ObjectFactory_Builds());
        GetService<ObjectFactory_Builds>().StartFactory();

        Register_Service(new ObjectFactory_Squads());
        GetService<ObjectFactory_Squads>().StartFactory();

        Register_Service(new PanelFactory_SquadPanel());

        Register_Service(new PanelFactory_BuildPanel());

        Register_Service(new EventBus());
    }

    public void RegisterControllers()
    {
        Register_Controller(GameObject.FindAnyObjectByType<CellUIScript>());

        Register_Controller(GameObject.FindAnyObjectByType<FocusOnCellScript>());


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
    }

}
