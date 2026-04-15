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
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<MovementController, HeadquartersSquadsManager, AbstractSquad, GameObject>((sender, key, squad, cell) =>
        {
            if (cellToSquads.ContainsKey(cell))
            {
                if (!squadsInControlledArea.Contains(squad) && !cellToSquads[cell].Contains(squad))
                {
                    squadsInControlledArea.Add(squad);
                    cellToSquads[cell].Add(squad);
                }


            }
        });

        this.headquartersAreaCasing = headquartersAreaCasing;

        foreach (GameObject cell in cellsInControl) 
        {
            var cellSquadsOnArea = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellSquadsOnArea>();

            cellToSquads[cell] = cellSquadsOnArea.squadsOnCell.ToList();

            squadsInControlledArea.AddRange(cellToSquads[cell]);

            cellSquadsOnArea.SquadState += SquadExistenceWatcher;
        }
    }

    public bool ComandToMove(GameObject fromCell, GameObject toCell, bool randomChoise = true)
    {
        if (randomChoise)
        {
            CellSquadsOnArea cellSquadsOnArea = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(fromCell).GetParameter<CellSquadsOnArea>();

            List<AbstractSquad> freeSquads = new();
            foreach(var squad in cellSquadsOnArea.squadsOnCell)
            {
                if(squad.SquadAction == SquadActions.None) { freeSquads.Add(squad); }
            }
            if(freeSquads.Count <= 0)return false;

            EnemysSquadsMovement enemySquadMovement = new(freeSquads[UnityEngine.Random.Range(0,freeSquads.Count)],
                (fromCell,toCell), null,this);

            ServiceRegistry.WorkWithService<CommandBus>().Enqueue(enemySquadMovement);

            return true;
        }

        return false;
    }

    public void TryToSwitchSquad(AbstractSquad squad, GameObject fromCell, GameObject toCell)
    {
        if (cellToSquads.ContainsKey(fromCell) && fromCell != toCell) { cellToSquads[fromCell].Remove(squad); cellToSquads[toCell].Add(squad); }
    }
    public void TryToTransferSquad(AbstractSquad squad, GameObject fromCell, GameObject toCell)
    {

    }
    public bool TryToRemoveSquad(AbstractSquad squad, GameObject fromCell)
    {
        if (cellToSquads[fromCell].Contains(squad)) { cellToSquads[fromCell].Remove(squad);squadsInControlledArea.Remove(squad); return true; }
        return false;
    }

    public void SquadExistenceWatcher(GameObject cell, AbstractSquad squad, bool state)
    {
        if (!state)
        {
            cellToSquads[cell].Remove(squad);
            squadsInControlledArea.Remove(squad);
        }
        else {
            cellToSquads[cell].Add(squad);
            squadsInControlledArea.Add(squad);
        }
    }
    
    public void GetHelpToOtherHeadquarters(GameObject cellToHelp)
    {
        var cellWithHighReliability = headquartersAreaCasing.GetCellWithHighReliability(UnityEngine.Random.Range(35, 85));

        ComandToMove(cellWithHighReliability, cellToHelp);
    }
}


public class RandomChoseSquad : ICommand
{
    public Guid Id { get; private set; }

    public CommandState State { get; private set; } = CommandState.Prepared;

    public void Cancel()
    {
        throw new NotImplementedException();
    }

    public bool CanExecute()
    {
        return true;
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
    public enum TypeOfMovementWay
    {
        None,
        OnHomeTerritory,
        OnOtherTerritory
    }
    private TypeOfMovementWay typeOfMovement;

    private AbstractSquad squadInMovement;

    private Action<AbstractSquad, bool> notifyAction;

    private HeadquartersSquadsManager headquartersSquadsManager;
    private HeadquartersSquadsManager otherHeadquartersSquadsManager;

    private GameObject startCell;
    private GameObject endCell;

    private List<GameObject> cellWay = new();

    public EnemysSquadsMovement(AbstractSquad squadInMovement, (GameObject,GameObject) startEndCell,
        Action<AbstractSquad,bool> notifyAction = null,
        HeadquartersSquadsManager headquartersSquadsManager = null, HeadquartersSquadsManager otherHeadquartersSquadsManager = null,
        TypeOfMovementWay typeOfMovement = TypeOfMovementWay.None)
    {
        this.typeOfMovement = typeOfMovement;
        squadInMovement.SquadAction = SquadActions.Moving;

        this.squadInMovement = squadInMovement;
        this.notifyAction = notifyAction;

        this.headquartersSquadsManager = headquartersSquadsManager;
        this.otherHeadquartersSquadsManager = otherHeadquartersSquadsManager;

        startCell = startEndCell.Item1;
        endCell = startEndCell.Item2;

        CustomLog.BlueText($"{squadInMovement.Name} || {squadInMovement.Attack} {squadInMovement.Protection} {squadInMovement.Speed}");
    }


    public Guid Id { get; private set; }

    public CommandState State { get; private set; } = CommandState.Created;


    public void Prepare()
    {
        CustomLog.RedText_Warning($"Prepare command for {squadInMovement.Name} start cell {startCell.name}, end cell {endCell.name} || {squadInMovement.Attack} {squadInMovement.Protection} {squadInMovement.Speed}");

        cellWay = ServiceRegistry.WorkWithController<AAlgorithm>().CreateWay(startCell, endCell, SideEnum.Enemys).ToList();

        CustomLog.GreenText_Warning($"Cell way constructed for {squadInMovement.Name} start cell {startCell.name}, end cell {endCell.name} || {squadInMovement.Attack} {squadInMovement.Protection} {squadInMovement.Speed}");

        State = CommandState.Prepared;
    }
    public void Cancel()
    {
        
    }

    public bool CanExecute()
    {
        CustomLog.RedText_Warning($"Can execute for {squadInMovement.Name} start cell {startCell.name}, end cell {endCell.name} || {squadInMovement.Attack} {squadInMovement.Protection} {squadInMovement.Speed}");
        return cellWay.Count != 0;
    }

    public void Execute()
    {
        if (State != CommandState.Prepared) return;

        headquartersSquadsManager.TryToRemoveSquad(squadInMovement, startCell);


        CustomLog.GreenText_Warning($"Execution for {squadInMovement.Name} start cell {startCell.name}, end cell {endCell.name} || {squadInMovement.Attack} {squadInMovement.Protection} {squadInMovement.Speed}");

        ServiceRegistry.WorkWithController<MovementController>().AddMovementWithWay(cellWay, squadInMovement, false);

        ServiceRegistry.WorkWithController<MovementController>().AddEvent(squadInMovement, new Action<AbstractSquad, GameObject>((squad, cell) =>
        {
            notifyAction?.Invoke(squad,cell == endCell);

            CustomLog.BlueText($"{squad.Name} {startCell.name} {cell.name} {headquartersSquadsManager}");

            ServiceRegistry.WorkWithService<EventBus>().Publish<MovementController, HeadquartersSquadsManager, AbstractSquad, GameObject>(null,null,squad,cell);
        }));

        State = CommandState.Executing;
    }
}