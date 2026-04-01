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
    private HeadquartersAreaCasing headquartersAreaCasing;

    public HeadquartersSquadsManager(List<GameObject> cellsInControl, HeadquartersAreaCasing headquartersAreaCasing)
    {
        this.headquartersAreaCasing = headquartersAreaCasing;

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
            CellSquadsOnArea cellSquadsOnArea = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(fromCell).GetParameter<CellSquadsOnArea>();

            EnemysSquadsMovement enemySquadMovement = new(
                cellSquadsOnArea.squadsOnCell[UnityEngine.Random.Range(0, cellSquadsOnArea.GetCountCurrentMax().Item1)],
                (fromCell,toCell), null,headquartersAreaCasing.CalculateDanger_ByCellInArea);

        }
    }

    public void TryToSwitchSquad(AbstractSquad squad, GameObject fromCell, GameObject toCell)
    {
        if (cellToSquads.ContainsKey(fromCell) && fromCell != toCell) { cellToSquads[fromCell].Remove(squad); cellToSquads[toCell].Add(squad); }
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
    private Action<GameObject> localDangerFunction;

    private HeadquartersSquadsManager headquartersSquadsManager;

    private GameObject startCell;
    private GameObject endCell;

    private List<GameObject> cellWay;

    public EnemysSquadsMovement(AbstractSquad squadInMovement, (GameObject,GameObject) startEndCell,
        Action<AbstractSquad,bool> notifyAction = null, Action<GameObject> localDangerFunction = null,
        HeadquartersSquadsManager headquartersSquadsManager = null)
    {
        this.squadInMovement = squadInMovement;
        this.notifyAction = notifyAction;
        this.localDangerFunction = localDangerFunction;

        this.headquartersSquadsManager = headquartersSquadsManager;

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
            localDangerFunction?.Invoke(cell);

            headquartersSquadsManager.TryToSwitchSquad(squad, startCell, cell);
        }));
    }
}