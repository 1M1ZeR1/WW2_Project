using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HeadquartersAreaCasing
{
    private float dangerDistance;
    protected int _localDangerParameter = 100;

    private Dictionary<GameObject, int> localCellDangerPoints = new();
    private bool headquartersInDanger = false;

    private HeadquartersBuild headquartersBuild;
    private GameObject cellWithThisHeadquarters;

    public HeadquartersAreaCasing(HeadquartersBuild headquartersBuild, float dangeDistanceParameter)
    {
        this.headquartersBuild = headquartersBuild;
        dangerDistance = dangeDistanceParameter;

        headquartersBuild.CellsInArea.ForEach(cell => { RecalculateLocalDanger();});
    }

    public void RecalculateAllDangers(GameObject cell)
    {
        RecalculateLocalDanger(cell,headquartersBuild.CellsInArea.Contains(cell));

        RecalculateGeneralDanger();
    }

    private void RecalculateLocalDanger(GameObject cell = null, bool recalculationByCellInArea = false)
    {
        if (cell == null)
        {
            foreach(var cellInArea in headquartersBuild.CellsInArea)
            {
                CellParametersHandler cellParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellInArea);

                var currentMaxCount = cellParametersHandler.GetParameter<CellSquadsOnArea>().GetCountCurrentMax();

                localCellDangerPoints[cellInArea] = (int)(_localDangerParameter * currentMaxCount.Item1 / (float)currentMaxCount.Item2);
            }
        }
        else
        {
            if (!recalculationByCellInArea)
            {
                List<GameObject> cellsInDangerZone = new();

                foreach (var cellInArea in headquartersBuild.CellsInArea)
                {
                    float currentDistance = Vector3.Distance(cellInArea.transform.position, cell.transform.position);

                    if (currentDistance <= dangerDistance)
                    {
                        CellParametersHandler cellParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellInArea);

                        var currentMaxCount = cellParametersHandler.GetParameter<CellSquadsOnArea>().GetCountCurrentMax();

                        localCellDangerPoints[cellInArea] = (int)(_localDangerParameter * (currentMaxCount.Item1 / (float)currentMaxCount.Item2) * (currentDistance / dangerDistance));
                    }
                }
            }
            else
            {
                CellParametersHandler cellParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell);
                CellArea cellArea = cellParametersHandler.GetParameter<CellArea>();


                headquartersInDanger = cellArea.IsCellNeighbor(cellWithThisHeadquarters) && cellArea.Side == SideEnum.Allies;


                switch (cellArea.Side)
                {
                    case SideEnum.Allies:
                        foreach (var cellInArea in headquartersBuild.CellsInArea)
                        {
                            if (cellInArea == cell) { localCellDangerPoints[cell] = -1; continue; }

                            CellParametersHandler cellInAreaParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellInArea);

                            if (cellInAreaParametersHandler.GetParameter<CellArea>().IsCellNeighbor(cell)) { localCellDangerPoints[cellInArea] = 0; continue; }

                            var currentMaxCount = cellParametersHandler.GetParameter<CellSquadsOnArea>().GetCountCurrentMax();

                            localCellDangerPoints[cellInArea] = (int)(_localDangerParameter * currentMaxCount.Item1 / (float)currentMaxCount.Item2);
                        }
                        break;
                    case SideEnum.Enemys:
                        foreach (var cellInArea in headquartersBuild.CellsInArea)
                        {
                            CellParametersHandler cellInAreaParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellInArea);

                            var currentMaxCount = cellParametersHandler.GetParameter<CellSquadsOnArea>().GetCountCurrentMax();

                            localCellDangerPoints[cellInArea] = (int)(_localDangerParameter * currentMaxCount.Item1 / (float)currentMaxCount.Item2);
                        }
                        break;
                }
            }
        }
    }

    private void RecalculateGeneralDanger()
    {
        if (headquartersInDanger) { ServiceRegistry.WorkWithController<EnemysController>().HeadquartersDangerPoints[headquartersBuild] = -1; return; }

        int generalSafety = 0;

        foreach(var localPoints in localCellDangerPoints)
        {
            generalSafety += localPoints.Value;
        }

        ServiceRegistry.WorkWithController<EnemysController>().HeadquartersDangerPoints[headquartersBuild]
            = (int)(generalSafety/(float)localCellDangerPoints.Values.Count);
    }
}
