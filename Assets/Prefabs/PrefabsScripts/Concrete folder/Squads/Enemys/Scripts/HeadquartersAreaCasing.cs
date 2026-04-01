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
    protected int _localScattering = 20;

    private Dictionary<GameObject, int> localCellDangerPoints = new();
    private bool headquartersInDanger = false;

    private HeadquartersBuild headquartersBuild;
    private GameObject cellWithThisHeadquarters;

    private HeadquartersSquadsManager squadsManager;

    public HeadquartersAreaCasing(HeadquartersBuild headquartersBuild, float dangeDistanceParameter)
    {
        this.headquartersBuild = headquartersBuild;
        dangerDistance = dangeDistanceParameter;

        headquartersBuild.CellsInArea.ForEach(cell => { RecalculateLocalDanger();});

        squadsManager = new HeadquartersSquadsManager(headquartersBuild.CellsInArea,this);
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
                localCellDangerPoints[cellInArea] = CalculateLocalDanger(cellInArea);
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
                        localCellDangerPoints[cellInArea] = CalculateLocalDanger(cellInArea);

                        if (localCellDangerPoints[cellInArea]-_localScattering < 20) { cellsInDangerZone.Add(cellInArea); }
                    }
                }

                if (cellsInDangerZone.Count > 0) { DangerSituation(cellsInDangerZone); }
            }
            else
            {
                CellParametersHandler cellParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell);
                CellArea cellArea = cellParametersHandler.GetParameter<CellArea>();

                List<GameObject> cellsInDangerZone = new();

                if(cellArea.IsCellNeighbor(cellWithThisHeadquarters) && cellArea.Side == SideEnum.Allies)
                {
                    headquartersInDanger = true;
                    cellsInDangerZone.Add(cellWithThisHeadquarters);
                }


                switch (cellArea.Side)
                {
                    case SideEnum.Allies:
                        foreach (var cellInArea in headquartersBuild.CellsInArea)
                        {
                            if (cellInArea == cell) { localCellDangerPoints[cell] = -1; continue; }

                            CellParametersHandler cellInAreaParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellInArea);

                            if (cellInAreaParametersHandler.GetParameter<CellArea>().IsCellNeighbor(cell)) {
                                localCellDangerPoints[cellInArea] = 0;
                                cellsInDangerZone.Add(cellInArea);
                                continue; }

                            localCellDangerPoints[cellInArea] = CalculateLocalDanger(cellInArea,cellInAreaParametersHandler);
                        }
                        break;
                    case SideEnum.Enemys:
                        foreach (var cellInArea in headquartersBuild.CellsInArea)
                        {
                            localCellDangerPoints[cellInArea] = CalculateLocalDanger(cellInArea);
                        }
                        break;
                }

                if (cellsInDangerZone.Count > 0) { DangerSituation(cellsInDangerZone, headquartersInDanger); }
            }
        }
    }

    public void CalculateDanger_ByCellInArea(GameObject cell)
    {
        localCellDangerPoints[cell] = CalculateLocalDanger(cell);

        if (!headquartersInDanger) { RecalculateGeneralDanger(); }
    }

    private int CalculateLocalDanger(GameObject cell, CellParametersHandler cellParametersHandler = null)
    {

        if (cellParametersHandler == null) { cellParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell); }

        var currentMaxCount = cellParametersHandler.GetParameter<CellSquadsOnArea>().GetCountCurrentMax();

        return (int)(_localDangerParameter * currentMaxCount.Item1 / (float)currentMaxCount.Item2);
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

    //Возможно перенос на Coroutine
    private void DangerSituation(List<GameObject> forCells, bool cellWithHeadquarters = false)
    {
        //Нужно будет сделать более умное решение для просчёта локальной угрозы. Так как заранее бот не будет подвигать войска.

        int dangerLimit = 90;
        bool reinforced = false;

        Dictionary<GameObject, int> needSquads = new();

        forCells.ForEach(cell =>
        {
            (int,int) countSquadsOnCell = ServiceRegistry.WorkWithController<CellController>()
                        .WorkWithCell<CellParametersHandler>(cell).GetParameter<CellSquadsOnArea>().GetCountCurrentMax();

            needSquads[cell] = countSquadsOnCell.Item2 - countSquadsOnCell.Item1;
        });

        if (cellWithHeadquarters)
        {
            CellParametersHandler cellWithHeadquartersParameters = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellWithThisHeadquarters);

            List<GameObject> cellsDonors = cellWithHeadquartersParameters.GetParameter<CellArea>().
                GetNeighbores().Where(cell => localCellDangerPoints.ContainsKey(cell)).ToList();

            while (needSquads[cellWithThisHeadquarters] != 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, cellsDonors.Count);

                CellParametersHandler cellParametersHandler = ServiceRegistry.WorkWithController<CellController>()
                        .WorkWithCell<CellParametersHandler>(cellsDonors[randomIndex]);

                CellSquadsOnArea cellSquadsOnArea = cellParametersHandler.GetParameter<CellSquadsOnArea>();

                if (cellSquadsOnArea.GetCountCurrentMax().Item1 <= 1)
                {
                    cellsDonors.RemoveAt(randomIndex);

                    if (cellsDonors.Count == 0) break;

                    continue;
                }

                squadsManager.ComandToMove(cellsDonors[randomIndex], cellWithThisHeadquarters, true);

                needSquads[cellWithThisHeadquarters] -= 1;
            }
        }

        while (dangerLimit > 10)
        {
            List<GameObject> cellsWithLowestDanger = localCellDangerPoints.Keys.Where(cell => localCellDangerPoints[cell] <=  dangerLimit 
            && !forCells.Contains(cell)).ToList();

            if(cellsWithLowestDanger.Count > 0)
            {
                while(forCells.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, cellsWithLowestDanger.Count);

                    CellParametersHandler cellParametersHandler = ServiceRegistry.WorkWithController<CellController>()
                        .WorkWithCell<CellParametersHandler>(cellsWithLowestDanger[randomIndex]);

                    CellSquadsOnArea cellSquadsOnArea = cellParametersHandler.GetParameter<CellSquadsOnArea>();

                    if (cellSquadsOnArea.GetCountCurrentMax().Item1 <= 1)
                    {
                        cellsWithLowestDanger.RemoveAt(randomIndex);

                        continue;
                    }

                    int randomIndex_ForCells = UnityEngine.Random.Range(0, forCells.Count);

                    squadsManager.ComandToMove(cellsWithLowestDanger[randomIndex], forCells[randomIndex_ForCells], true);

                    needSquads[forCells[randomIndex_ForCells]] -= 1;

                    if(needSquads[forCells[randomIndex_ForCells]] == 0) { needSquads.Remove(forCells[randomIndex_ForCells]); 
                        forCells.RemoveAt(randomIndex_ForCells);

                        if (forCells.Count == 0) { reinforced = true; break; }
                    } 
                }
            }

            if (reinforced) { break; }

            dangerLimit -= 20;
        }
    }
}
