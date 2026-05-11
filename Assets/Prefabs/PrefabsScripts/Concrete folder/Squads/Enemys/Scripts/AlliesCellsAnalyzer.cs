using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AlliesCellsAnalyzer
{
    private List<GameObject> cellOnAlliesControl = new();
    
    public AlliesCellsAnalyzer()
    {
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<GameObject, SideEnum, CellController>((cell, side, key) =>
        {
            switch (side)
            {
                case SideEnum.Allies:
                    AlliesCapturedCell(cell);
                    break;
                default:
                    AlliesLoseCell(cell);
                    break;
            }
        });

    }

    public void AlliesCapturedCell(GameObject cell)
    {
        if (!cellOnAlliesControl.Contains(cell))
        {
            CustomLog.YellowText($"Player capture cell - {cell.name}");

            cellOnAlliesControl.Add(cell);

            HeadquartersAwakener(cell);
        }
    }
    public void AlliesLoseCell(GameObject cell)
    {
        if (!cellOnAlliesControl.Contains(cell))
        {
            CustomLog.YellowText($"Player lost control of cell - {cell.name}");

            cellOnAlliesControl.Remove(cell);

            HeadquartersAwakener(cell);
        }
    }

    private void HeadquartersAwakener(GameObject cell)
    {
        var headquarters = ServiceRegistry.WorkWithController<EnemysController>().FindNearestHeadquarters(cell);

        if(headquarters!= null) ServiceRegistry.WorkWithController<EnemysController>().HeadquartersCasing[headquarters].RecalculateAllDangers(cell);
    }
}
