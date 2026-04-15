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
            return;
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
        float minDistance = float.MaxValue;
        HeadquartersBuild choosedHeadquarters = null;

        foreach (var headquarters in ServiceRegistry.WorkWithController<EnemysController>().CellWithHeadquarters_Bot)
        {
            var distance = Vector3.Distance(headquarters.Key.transform.position, cell.transform.position);
            if (distance < minDistance) { choosedHeadquarters = headquarters.Value; minDistance = distance; }
        }

        if (choosedHeadquarters != null)ServiceRegistry.WorkWithController<EnemysController>().HeadquartersCasing[choosedHeadquarters].RecalculateAllDangers(cell);
    }
}
