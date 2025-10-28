using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEventCellType
{
    public delegate void EventIsOver(AbstractCell abstractCell);
    public event EventIsOver eventIsOver;

    public int GetTimer();

}
public interface IEventSquadType
{
    public delegate void EventIsOver(AbstractSquad squad);
    public event EventIsOver eventIsOver;

    public int GetTimer();
}
