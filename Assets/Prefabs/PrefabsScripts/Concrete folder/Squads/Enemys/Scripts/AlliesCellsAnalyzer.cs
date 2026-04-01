using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AlliesCellsAnalyzer
{
    private List<GameObject> cellOnAlliesControl = new();

    public void AlliesCapturedCell(GameObject cell)
    {
        if (!cellOnAlliesControl.Contains(cell))
        {
            Debug.LogWarning($"Player capture cell - {cell.name}");

            cellOnAlliesControl.Add(cell);

            HeadquartersAwakener(cell);
        }
    }
    public void AlliesLoseCell(GameObject cell)
    {
        if (!cellOnAlliesControl.Contains(cell))
        {
            Debug.LogWarning($"Player lost control of cell - {cell.name}");

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
            if (Vector3.Distance(headquarters.Key.transform.position,cell.transform.position) < minDistance) { choosedHeadquarters = headquarters.Value; }
        }

        if(choosedHeadquarters != null)ServiceRegistry.WorkWithController<EnemysController>().HeadquartersCasing[choosedHeadquarters].RecalculateAllDangers(cell);
    }
}
