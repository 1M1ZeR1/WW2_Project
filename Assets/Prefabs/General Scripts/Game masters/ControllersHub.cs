using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ControllersHub 
{
    private readonly Dictionary<Type, object> controllers = new();

    private void Register<T>(T controller)
    {
        controllers[typeof(T)] = controller;
    }

    public T Get<T>()
    {
        return (T)controllers[typeof(T)];
    }
    public void Start()
    {
        Get<ResourcesController>().Start();
        Get<GameController>().Start();
        Get<EnemysController>().Start();
        Get<MovementController>().Start();
    }

    public ControllersHub()
    {
        Register(GameObject.FindAnyObjectByType<MonobehaviourMaster>());
        Register(GameObject.FindAnyObjectByType<CellUIScript>());

        Register(GameObject.FindAnyObjectByType<FocusOnCellScript>());
        Register(new ObjectsFactory());


        Register(new GameController());

        Register(new BattleController());

        Register(new MovementController());

        Register(GameObject.FindAnyObjectByType<ArrowCanvasScript>());

        Register(new UnitsSpawner());

        Register(new AAlgorithm());

        Register(new CellController());

        Register(new CellInteraction());

        Register(new ResourcesController());

        Register(GameObject.FindAnyObjectByType<MessageScript>());
        Register(GameObject.FindAnyObjectByType<WorldOnCanvasScript>());

        Register(new BuilderController());

        Register(GameObject.FindAnyObjectByType<InteractableScript>());
        Register(new EnemysController());
    }

}
