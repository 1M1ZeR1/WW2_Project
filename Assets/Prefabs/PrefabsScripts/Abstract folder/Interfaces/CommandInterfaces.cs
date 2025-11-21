using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand
{
    Guid Id { get; }
    CommandState State { get; }

    bool CanExecute();
    void Prepare();
    void Execute();
    void Cancel();

}
public interface IReversible
{
    void Undo();
}
public enum CommandState { Created, Prepared, Executing, Completed, Cancelled, Failed}


public interface ICommandBus
{
    Guid Enqueue(ICommand cmd, CommandPriority priority = CommandPriority.Normal, GameObject commandConnectedWith = null);
    void Cancel(Guid commandId);
    bool TryGet(Guid id, out ICommand cmd);
}

public enum CommandPriority { Low, Normal, High, Critical }
