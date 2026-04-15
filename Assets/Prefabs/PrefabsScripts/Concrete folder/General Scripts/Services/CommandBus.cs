using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CommandBus : ICommandBus
{
    private readonly Dictionary<Guid, ICommand> _commands = new();
    private readonly SortedList<CommandPriority, Queue<ICommand>> _queue = new();

    private readonly Dictionary<Guid, GameObject> _commandsConWithCell = new();
    private readonly Dictionary<Guid, int> _commandsLiveTime = new();

    public CommandBus()
    {
        foreach (CommandPriority prio in Enum.GetValues(typeof(CommandPriority)))
        {
            _queue[prio] = new Queue<ICommand>();
        }

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<Guid, ICommand>((id, sender) =>
        {
            _commands.Remove(id);
            _commandsConWithCell.Remove(id);
            if (_commandsLiveTime.ContainsKey(id)) { _commandsLiveTime.Remove(id); }
        });

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<GameController>((sender) =>
        {
            foreach (var kvp in _queue.OrderByDescending(q => q.Key))
            {
                if (kvp.Value.Count > 0)
                {
                    var cmd = kvp.Value.Dequeue();

                    switch (cmd.State)
                    {
                        case CommandState.Created:
                            cmd.Prepare();
                            //if (cmd.CanExecute()) cmd.Execute();
                            break;

                        case CommandState.Prepared:
                            if (cmd.CanExecute()) { cmd.Execute(); _commandsLiveTime.Remove(cmd.Id); }
                            else cmd.Cancel();
                            break;

                        case CommandState.Failed:
                            cmd.Cancel();
                            break;
                        case CommandState.Cancelled:
                            _commands.Remove(cmd.Id);
                            break;
                        case CommandState.Completed:
                            _commands.Remove(cmd.Id);
                            break;
                    }
                }
            }
            List<Guid> commandsToDelete = new();

            foreach(var item in _commandsLiveTime.Keys.ToList())
            {
                _commandsLiveTime[item]--;
                if (_commands[item].CanExecute() && _commands[item].State == CommandState.Prepared) {_commands[item].Execute(); commandsToDelete.Add(item);}

                if (_commandsLiveTime[item] == 0) { commandsToDelete.Add(item); }
            }

            foreach(var item in commandsToDelete) { DeleteCommand(item);}
            commandsToDelete.Clear();

        });
    }

    public Guid Enqueue(ICommand cmd, CommandPriority priority = CommandPriority.Normal, GameObject commandConnectedWith = null)
    {
        var id = Guid.NewGuid();
        _commands[id] = cmd;
        _queue[priority].Enqueue(cmd);

        _commandsConWithCell.Add(id, commandConnectedWith);

        switch (priority)
        {
            case CommandPriority.Low: _commandsLiveTime[id] = 1; break;
            case CommandPriority.Normal: _commandsLiveTime[id] = 5; break;
            case CommandPriority.High: _commandsLiveTime[id] = 15; break;
            case CommandPriority.Critical: _commandsLiveTime[id] = 30; break;
        }

        return id;
    }

    public void Cancel(Guid commandId)
    {
        if (_commands.TryGetValue(commandId, out var cmd))
        {
            cmd.Cancel();
            _commands.Remove(commandId);
            if (_commandsLiveTime.ContainsKey(commandId)) _commandsLiveTime.Remove(commandId);
            _commandsConWithCell.Remove(commandId);
        }
    }
    public void DeleteCommand(Guid commandId)
    {
        _commands.Remove(commandId);
        if (_commandsLiveTime.ContainsKey(commandId)) _commandsLiveTime.Remove(commandId);
        _commandsConWithCell.Remove(commandId);
    }

    public bool TryGet(Guid id, out ICommand cmd)
    {
        return _commands.TryGetValue(id, out cmd);
    }
}
