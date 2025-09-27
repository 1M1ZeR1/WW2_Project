using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadEvent_Good : IEventSquadType
{
    protected int _timer = 0;
    protected AbstractSquad _squad;

    protected List<AbstractBuffs> _buffsOnThisEvent = new List<AbstractBuffs>();

    public event IEventSquadType.EventIsOver eventIsOver;

    public SquadEvent_Good(int timer, AbstractSquad abstractSquad, List<AbstractBuffs> abstractBuffs) 
    {
        _timer = timer;

        if(abstractSquad == null)
        {

        }
        _squad = abstractSquad;

        _buffsOnThisEvent = abstractBuffs;


        SetAllBuffs();
    }

    public int GetTimer() { return  _timer; }

    private void SetAllBuffs()
    {
        foreach(var buff in _buffsOnThisEvent)
        {
            _squad.AddBuff(buff);
        }
    }

    public void RemoveAllBuffs()
    {
        foreach(var buff in _buffsOnThisEvent)
        {
            _squad.RemoveBuff(buff);
        }
    }
    public IEnumerator StartTimer()
    {
        int timer = _timer;

        while(timer != 0)
        {
            if(PauseScript.CurrentGameState != GameState.Play) { yield return null; }

            yield return new WaitForSeconds(1f);

            timer--;
        }

        RemoveAllBuffs();
    }
}
