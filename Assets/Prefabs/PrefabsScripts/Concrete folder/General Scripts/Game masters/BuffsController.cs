using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffsController : MonoBehaviour
{
    [Header("Контроллер времени")]
    [SerializeField] private GameObject timeControllerObject;
    private TimeControllerScript timeControllerScript;

    protected List<AbstractBuffs> buffsWithTimer = new List<AbstractBuffs>();

    private void Start()
    {
        timeControllerObject.TryGetComponent(out timeControllerScript);

        timeControllerScript.OnHourHasPassed += HourHasPassed;
    }

    public void AddBuffToList(AbstractBuffs buff)
    {
        buffsWithTimer.Add(buff);

        buffsWithTimer.Sort((x, y) => DateTime.Compare(x.GetTimeToExpire(), y.GetTimeToExpire()));
    }
    private void HourHasPassed(DateTime currentTime)
    {
        if(buffsWithTimer.Count != 0)
        {
            if (buffsWithTimer[0].NeedToExpire(currentTime))
            {
                Debug.Log("Эту случилось");
                List<AbstractBuffs> buffsToRemove = new List<AbstractBuffs>();

                foreach (AbstractBuffs buff in buffsWithTimer)
                {
                    if (buff.NeedToExpire(currentTime)) { buffsToRemove.Add(buff);buff.InvokeClearEvent(buff); }
                    else { break; }
                }

                buffsWithTimer.RemoveAll(buff => buffsToRemove.Contains(buff));
            }
        }
    }
}
public class BuffTarget
{
    private GameObject cell;
    private AbstractSquad abstractSquad;
    private AbstractBuildings building;

    public BuffTarget(GameObject cell, AbstractSquad abstractSquad, AbstractBuildings building)
    {
        this.cell = cell;
        this.abstractSquad = abstractSquad;
        this.building = building;
    }

    public void TargetAddBuff(AbstractBuffs buff)
    {
        if(cell != null) { cell.GetComponent<CellTypeScript>().AddBuffToCell(buff); }
        if(abstractSquad != null) { abstractSquad.AddBuff(buff); }
        if(building != null) { }
    }
}
