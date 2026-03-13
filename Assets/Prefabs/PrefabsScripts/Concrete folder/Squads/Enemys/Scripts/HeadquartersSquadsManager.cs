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

public class EnemysSquadsMovement : ICommand
{
    public Guid Id { get; private set; }

    public CommandState State { get; private set; } = CommandState.Created;

    public void Prepare()
    {
        

        State = CommandState.Prepared;
    }
    public void Cancel()
    {
        
    }

    public bool CanExecute()
    {
        
        return false;
    }

    public void Execute()
    {
        
    }
}