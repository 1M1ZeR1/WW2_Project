using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        //headquartersBuild.GetCellsInArea_BySide(SideEnum.Enemys).ForEach(cell => { RecalculateLocalDanger();});
        RecalculateLocalDanger();

        squadsManager = new HeadquartersSquadsManager(headquartersBuild.GetCellsInArea_BySide(SideEnum.Enemys),this);
    }


    public void RecalculateAllDangers(GameObject cellInitiator = null, GameObject cellNeedRecalculation = null)
    {
        if (cellNeedRecalculation != null)
        {
            localCellDangerPoints[cellNeedRecalculation] = CalculateLocalDanger(cellNeedRecalculation);
        }
        else
        {
            if (cellInitiator == null) RecalculateLocalDanger();
            else RecalculateLocalDanger(cellInitiator, headquartersBuild.GetCellsInArea_BySide(SideEnum.Enemys).Contains(cellInitiator));
        }

        RecalculateGeneralDanger();
    }

    private void RecalculateLocalDanger(GameObject cellInitiator = null, bool recalculationByCellInArea = false)
    {
        if (cellInitiator == null)
        {
            foreach(var cellInArea in headquartersBuild.GetCellsInArea_BySide(SideEnum.Enemys))
            {
                localCellDangerPoints[cellInArea] = CalculateLocalDanger(cellInArea);
            }
        }
        else
        {
            if (!recalculationByCellInArea)
            {
                CustomLog.PurpleText($"Recalculation by {cellInitiator.name}.");

                List<GameObject> cellsInDangerZone = new();

                foreach (var cellInArea in headquartersBuild.GetCellsInArea_BySide(SideEnum.Enemys))
                {
                    float currentDistance = Vector3.Distance(cellInArea.transform.position, cellInitiator.transform.position);

                    if (currentDistance <= dangerDistance)
                    {
                        CustomLog.YellowText($"{cellInArea.name} in danger distance of {cellInitiator.name} | {currentDistance}<={dangerDistance}");

                        localCellDangerPoints[cellInArea] = CalculateLocalDanger(cellInArea);

                        if (localCellDangerPoints[cellInArea]-_localScattering < 20) {
                            cellsInDangerZone.Add(cellInArea);

                            CustomLog.RedText($"Add {cellInArea.name} to cells in danger");
                        }
                    }
                }

                if (cellsInDangerZone.Count > 0) { DangerSituation(cellsInDangerZone); }
            }
            else
            {
                CustomLog.PurpleText($"Recalculation by {cellInitiator.name} what in area.");

                CellParametersHandler cellParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellInitiator);
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
                        foreach (var cellInArea in headquartersBuild.GetCellsInArea_BySide(SideEnum.Enemys))
                        {
                            if (cellInArea == cellInitiator) { localCellDangerPoints[cellInitiator] = -1; continue; }

                            CellParametersHandler cellInAreaParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellInArea);

                            if (cellInAreaParametersHandler.GetParameter<CellArea>().IsCellNeighbor(cellInitiator)) {
                                localCellDangerPoints[cellInArea] = 0;
                                cellsInDangerZone.Add(cellInArea);
                                continue; }

                            localCellDangerPoints[cellInArea] = CalculateLocalDanger(cellInArea,cellInAreaParametersHandler);
                        }
                        break;
                    case SideEnum.Enemys:
                        foreach (var cellInArea in headquartersBuild.GetCellsInArea_BySide(SideEnum.Enemys))
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
        if (headquartersInDanger) {
            CustomLog.RedText($"Enemys headquarters in danger:{headquartersBuild.CellWithThisBuild}");
            ServiceRegistry.WorkWithController<EnemysController>().HeadquartersDangerPoints[headquartersBuild] = -1; return; }

        int generalSafety = 0;

        foreach (var localPoints in localCellDangerPoints)
        {
            if(localPoints.Value == 0 ||  localPoints.Value == -1)
            {
                CustomLog.TextWithWordHighlight($"Headqurters on cell {localPoints.Key.name} get 0 danger by cell", $"{localPoints.Key.name}");
                ServiceRegistry.WorkWithController<EnemysController>().HeadquartersDangerPoints[headquartersBuild] = 0;
                return;
            }

            CustomLog.TextWithWordHighlight($"Local danger for {localPoints.Key.name} in area of {headquartersBuild.CellWithThisBuild.name}",$"{localPoints.Value}");
            generalSafety += localPoints.Value;
        }

        ServiceRegistry.WorkWithController<EnemysController>().HeadquartersDangerPoints[headquartersBuild]
            = (int)(generalSafety/(float)localCellDangerPoints.Values.Count);

        CustomLog.YellowText($"Headquarters danger:{ServiceRegistry.WorkWithController<EnemysController>().HeadquartersDangerPoints[headquartersBuild]}");
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
            CustomLog.GreenText($"Trying help {cell.name}");

            (int,int) countSquadsOnCell = ServiceRegistry.WorkWithController<CellController>()
                        .WorkWithCell<CellParametersHandler>(cell).GetParameter<CellSquadsOnArea>().GetCountCurrentMax();

            needSquads[cell] = countSquadsOnCell.Item2 - countSquadsOnCell.Item1;
        });

        if (cellWithHeadquarters)
        {
            CellParametersHandler cellWithHeadquartersParameters = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellWithThisHeadquarters);

            List<GameObject> cellsDonors = cellWithHeadquartersParameters.GetParameter<CellArea>().
                GetNeighbores().Where(cell => localCellDangerPoints.ContainsKey(cell)).ToList();

            //while (needSquads[cellWithThisHeadquarters] != 0)
            //{
            //    int randomIndex = UnityEngine.Random.Range(0, cellsDonors.Count);

            //    CellParametersHandler cellParametersHandler = ServiceRegistry.WorkWithController<CellController>()
            //            .WorkWithCell<CellParametersHandler>(cellsDonors[randomIndex]);

            //    CellSquadsOnArea cellSquadsOnArea = cellParametersHandler.GetParameter<CellSquadsOnArea>();

            //    if (cellSquadsOnArea.GetCountCurrentMax().Item1 <= 1)
            //    {
            //        cellsDonors.RemoveAt(randomIndex);

            //        if (cellsDonors.Count == 0) break;

            //        continue;
            //    }

            //    squadsManager.ComandToMove(cellsDonors[randomIndex], cellWithThisHeadquarters, true);

            //    needSquads[cellWithThisHeadquarters] -= 1;
            //}
        }

        //while (dangerLimit > 10)
        //{
        //    List<GameObject> cellsWithLowestDanger = localCellDangerPoints.Keys.Where(cell => localCellDangerPoints[cell] >= dangerLimit
        //    && !forCells.Contains(cell)).ToList();

        //    CustomLog.GreenText($"Found {cellsWithLowestDanger.Count} donors with danger lowest then {dangerLimit}");

        //    if (cellsWithLowestDanger.Count > 0)
        //    {
        //        while (forCells.Count > 0)
        //        {
        //            int randomIndex = UnityEngine.Random.Range(0, cellsWithLowestDanger.Count);

        //            CellParametersHandler cellParametersHandler = ServiceRegistry.WorkWithController<CellController>()
        //                .WorkWithCell<CellParametersHandler>(cellsWithLowestDanger[randomIndex]);

        //            CellSquadsOnArea cellSquadsOnArea = cellParametersHandler.GetParameter<CellSquadsOnArea>();

        //            if (cellSquadsOnArea.GetCountCurrentMax().Item1 <= 1)
        //            {
        //                cellsWithLowestDanger.RemoveAt(randomIndex);

        //                continue;
        //            }

        //            int randomIndex_ForCells = UnityEngine.Random.Range(0, forCells.Count);

        //            CustomLog.GreenText($"Send squad from {cellsWithLowestDanger[randomIndex]} to {forCells[randomIndex_ForCells]}");
        //            squadsManager.ComandToMove(cellsWithLowestDanger[randomIndex], forCells[randomIndex_ForCells], true);

        //            needSquads[forCells[randomIndex_ForCells]] -= 1;
        //            CustomLog.GreenText($"Need squads - {needSquads[forCells[randomIndex_ForCells]]} for cell {forCells[randomIndex_ForCells]}");

        //            if (needSquads[forCells[randomIndex_ForCells]] == 0)
        //            {
        //                needSquads.Remove(forCells[randomIndex_ForCells]);
        //                forCells.RemoveAt(randomIndex_ForCells);

        //                if (forCells.Count == 0) { reinforced = true; break; }
        //            }
        //        }
        //    }

        //    if (reinforced) { break; }

        //    dangerLimit -= 20;
        //}

        if (needSquads.Keys.Count() > 0)
        {
            ServiceRegistry.WorkWithService<EventBus>().Publish<HeadquartersAreaCasing, HeadquartersBuild, List<GameObject>>(this, headquartersBuild, needSquads.Keys.ToList());
        }
    }

    public List<GameObject> GetCellsWhatNeedHelp(int reliabilityLimit = 0)
    {
        List<GameObject> dangerCells = localCellDangerPoints.Where(x => x.Value == 0).Select(x => x.Key).ToList();

        if (dangerCells.Count > 0) { return dangerCells; }

        if (reliabilityLimit != 0)
        {
            dangerCells = localCellDangerPoints.Where(x => x.Value < reliabilityLimit).Select(x => x.Key).ToList();
        }
        else { dangerCells = localCellDangerPoints.Where(x => x.Value < 25).Select(x => x.Key).ToList(); }

        return dangerCells;
    }

    public GameObject GetCellWithHighReliability(int reliabilityLimit = 0)
    {
        if(reliabilityLimit != 0)
        {
            var foundedCells = localCellDangerPoints.Where(x => x.Value >= reliabilityLimit).ToList();

            return foundedCells.Count > 0 ? foundedCells[UnityEngine.Random.Range(0, foundedCells.Count)].Key : null;
        }

        return localCellDangerPoints.OrderByDescending(x => x.Value).First().Key;
    }

    public void SupportAction(List<GameObject> cellNeedHelp)
    {
        for(int i = 0; i < cellNeedHelp.Count; i++)
        {
            GameObject cellWithHighReliability = GetCellWithHighReliability(10 * (1 + i));

            int randomIndex = UnityEngine.Random.Range(0,cellNeedHelp.Count);
            bool commandMoveResult = squadsManager.ComandToMove(cellWithHighReliability, cellNeedHelp[randomIndex],true,true);

            CustomLog.PurpleText_Warning($"Founded cell:{cellWithHighReliability.name} to help cell:{cellNeedHelp[randomIndex]}");

            if (commandMoveResult) { continue; }
            else { i--; continue; }
        }
    }
}
