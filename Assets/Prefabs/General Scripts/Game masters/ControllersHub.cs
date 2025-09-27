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

    public ControllersHub()
    {
        Register(GameObject.FindAnyObjectByType<GameController>());

        Register(GameObject.FindAnyObjectByType<BattleController>());

        Register(new MovementController());

        Register(GameObject.FindAnyObjectByType<ArrowCanvasScript>());

        Register(new UnitsSpawner());

        Register(GameObject.FindAnyObjectByType<AAlgorithm>());

        Register(new CellController());

        Register(new CellInteraction());

        Register(GameObject.FindAnyObjectByType<ResourcesController>());
        Register(GameObject.FindAnyObjectByType<MessageScript>());
        Register(GameObject.FindAnyObjectByType<WorldOnCanvasScript>());
        Register(GameObject.FindAnyObjectByType<CellUIScript>());

        Register(new BuilderController());

        Register(GameObject.FindAnyObjectByType<InteractableScript>());
    }

}
