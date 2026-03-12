using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PullUpSquads : ICommand
{
    protected List<ICommand> _currentCommands;

    private List<AbstractSquad> squadInCommand = new();
    private GameObject cellFrom, cellTo;

    private int searchDepth, neededCountOfSquads;

    public Action<bool> CommandResult;


    public Guid Id { get; private set; }

    public CommandState State { get; private set; } = CommandState.Created;

    public PullUpSquads(GameObject cellFrom, GameObject cellTo, int searchDepth, int neededCountOfSquads)
    {
        this.cellFrom = cellFrom;
        this.cellTo = cellTo;

        this.searchDepth = searchDepth;
        this.neededCountOfSquads = neededCountOfSquads;
    }

    public void Prepare()
    {
        List<AbstractSquad> squadList = new();

        foreach (var cell in ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellFrom).GetParameter<CellArea>().NeighboresSearcher(searchDepth))
        {
            squadList.AddRange(ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellSquadsOnArea>().squadsOnCell);
        }

        //Реализация через рандом

        for (int i = 0; i < neededCountOfSquads; i++)
        {
            var squad = squadList[UnityEngine.Random.Range(0, squadList.Count - 1)];

            squadInCommand.Add(squad);
            squadList.Remove(squad);
        }

        State = CommandState.Prepared;
    }
    public void Cancel()
    {
        CommandResult.Invoke(false);
    }

    public bool CanExecute()
    {
        if (squadInCommand.Count > 0) { return true; }
        return false;
    }

    public void Execute()
    {
        _currentCommands = new();

        foreach (var squad in squadInCommand)
        {
            _currentCommands.Add(ServiceRegistry.WorkWithController<MovementController>().AddMovementForSquad(cellFrom, cellTo, squad));
        }

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<ICommand, SquadMovement, AbstractSquad, GameObject>((command, sender, squad, cell) =>
        {
            if (_currentCommands.Contains(command))
            {
                squadInCommand.Remove(squad);
                if (squadInCommand.Count == 0)
                {
                    if (cell == cellTo) { CommandResult.Invoke(true); }
                    else { CommandResult.Invoke(false); }
                }

                _currentCommands.Remove(command);
            }
        });
    }
}
