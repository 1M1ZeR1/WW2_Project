using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HeadquartersSquadsManager
{
    private List<AbstractSquad> squadsInControlledArea = new();

    private Dictionary<GameObject, List<AbstractSquad>> cellToSquads = new();

    public HeadquartersSquadsManager(List<GameObject> cellsInControl)
    {
        foreach (GameObject cell in cellsInControl) 
        {
            cellToSquads[cell] = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellSquadsOnArea>().squadsOnCell.ToList();

            squadsInControlledArea.AddRange(cellToSquads[cell]);
        }
    }

    public void ComandToMove(GameObject fromCell, GameObject toCell, bool randomChoise = true)
    {
        if (randomChoise)
        {

        }
    }
}

public class EnemysPlatoonMovemnt : ICommand
{



    public Guid Id { get; private set; }

    public CommandState State { get; private set; } = CommandState.Created;

    public void Cancel()
    {
        throw new NotImplementedException();
    }

    public bool CanExecute()
    {
        throw new NotImplementedException();
    }

    public void Execute()
    {
        throw new NotImplementedException();
    }

    public void Prepare()
    {
        throw new NotImplementedException();
    }
}

public class EnemysSquadsMovement : ICommand
{
    private AbstractSquad squadInMovement;
    private Action<AbstractSquad, bool> notifyAction;

    private GameObject startCell;
    private GameObject endCell;

    private List<GameObject> cellWay;

    public EnemysSquadsMovement(AbstractSquad squadInMovement, (GameObject,GameObject) startEndCell,Action<AbstractSquad,bool> notifyAction = null)
    {
        this.squadInMovement = squadInMovement;
        this.notifyAction = notifyAction;

        startCell = startEndCell.Item1;
        endCell = startEndCell.Item2;
    }


    public Guid Id { get; private set; }

    public CommandState State { get; private set; } = CommandState.Created;


    public void Prepare()
    {
        cellWay = ServiceRegistry.WorkWithController<AAlgorithm>().CreateWay(startCell, endCell, SideEnum.Enemys).ToList();

        State = CommandState.Prepared;
    }
    public void Cancel()
    {
        
    }

    public bool CanExecute()
    {
        return cellWay.Count != 0;
    }

    public void Execute()
    {
        ServiceRegistry.WorkWithController<MovementController>().AddMovementWithWay(cellWay, squadInMovement, false);

        ServiceRegistry.WorkWithController<MovementController>().AddEvent(squadInMovement, new Action<AbstractSquad, GameObject>((squad, cell) =>
        {
            notifyAction?.Invoke(squad,cell == endCell);
        }));
    }
}