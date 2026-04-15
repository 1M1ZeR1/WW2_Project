using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AAlgorithm
{
    private int penalty = 20;

    protected static List<GameObject> _cells;

    protected static Dictionary<GameObject, CellArea> _cellToAreaController = new Dictionary<GameObject, CellArea>();

    public static void SetAllCells(List<GameObject> cells)
    {
        _cells = cells;

        foreach (GameObject cell in _cells)
        {
            _cellToAreaController.Add(cell, ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>());
        }
    }
    public void SetDictionary(Dictionary<GameObject, CellArea> cells) { _cellToAreaController=cells;}
    public List<GameObject> CreateWay(GameObject startCell, GameObject endCell, SideEnum side)
    {
        if (!PermittedSide(side, ServiceRegistry.WorkWithController<CellController>().FastDrop_CellSide(endCell)))
        {
            return null;
        }

        List<GameObject> openSet = new List<GameObject> { startCell };
        List<GameObject> closedSet = new List<GameObject>();

        Dictionary<GameObject, float> cellToCost = new Dictionary<GameObject, float>
    {
        { startCell, 0 }
    };

        Dictionary<GameObject, float> heuristics = new Dictionary<GameObject, float>
    {
        { startCell, GetCostDistance(startCell, endCell) }
    };

        Dictionary<GameObject, GameObject> cameFrom = new Dictionary<GameObject, GameObject>();

        while (openSet.Count > 0)
        {
            GameObject currentCell = FindCellWithLowestCost(openSet, heuristics);

            if (currentCell == endCell)
            {
                return ReconstructWay(cameFrom, currentCell);
            }

            openSet.Remove(currentCell);
            closedSet.Add(currentCell);

            foreach (GameObject neighbor in _cellToAreaController[currentCell].GetNeighbores())
            {
                if (!PermittedSide(side, ServiceRegistry.WorkWithController<CellController>().FastDrop_CellSide(neighbor)) || closedSet.Contains(neighbor))
                {
                    continue;
                }

                float tentativeCost = cellToCost[currentCell] + ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(neighbor).GetParameter<CellMovementParameters>().GetCost();

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeCost >= cellToCost.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    continue;
                }

                cameFrom[neighbor] = currentCell;
                cellToCost[neighbor] = tentativeCost;
                heuristics[neighbor] = cellToCost[neighbor] + GetCostDistance(neighbor, endCell);
            }
        }
        return null;
    }
    private bool PermittedSide(SideEnum squadSide, SideEnum cellSide)
    {
        switch (squadSide) 
        {
            case SideEnum.Allies: return cellSide != SideEnum.Enemys;
            case SideEnum.Enemys: return cellSide != SideEnum.Allies;
        }

        return false;
    }
    public float? CalculateWayCost(GameObject startCell, GameObject endCell, SideEnum side)
    {
        var cellController = ServiceRegistry.WorkWithController<CellController>();

        if (side != SideEnum.None && cellController.FastDrop_CellSide(endCell) != side)
        {
            return null;
        }

        if (startCell == endCell)
        {
            return 0f;
        }

        List<GameObject> openSet = new List<GameObject> { startCell };
        HashSet<GameObject> closedSet = new HashSet<GameObject>();

        Dictionary<GameObject, float> cellToCost = new Dictionary<GameObject, float>
    {
        { startCell, 0 }
    };

        Dictionary<GameObject, float> heuristics = new Dictionary<GameObject, float>
    {
        { startCell, GetCostDistance(startCell, endCell) }
    };

        while (openSet.Count > 0)
        {
            GameObject currentCell = FindCellWithLowestCost(openSet, heuristics);
            Debug.LogWarning(currentCell);

            if (currentCell == endCell)
            {
                //foreach(var cell in cellToCost)
                //{
                //    Debug.LogWarning($"{cell.Key} and {cell.Value}");
                //}
                //foreach(var cell in closedSet) { Debug.LogWarning($"{cell}"); }
                return cellToCost[endCell];
            }

            openSet.Remove(currentCell);
            closedSet.Add(currentCell);

            foreach (GameObject neighbor in _cellToAreaController[currentCell].GetNeighbores())
            {
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                float movementCost = cellController
                    .WorkWithCell<CellParametersHandler>(neighbor)
                    .GetParameter<CellMovementParameters>()
                    .GetCost();
                //Debug.LogWarning($"{movementCost} for {neighbor}");

                float tentativeCost = cellToCost[currentCell] + movementCost;

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeCost >= cellToCost.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    continue;
                }

                cellToCost[neighbor] = tentativeCost;
                heuristics[neighbor] = tentativeCost + GetCostDistance(neighbor, endCell);
            }
        }

        return null;
    }
    private float CraftWayCost(List<GameObject> cellsInShortedWay, Dictionary<GameObject, float> cellToCost)
    {
        float result = 0f;

        foreach (var cell in cellsInShortedWay) { result += cellToCost[cell]; }

        return result;
    }



    private float GetCostDistance(GameObject startCell, GameObject endCell)
    {
        return Vector3.Distance(startCell.transform.position,endCell.transform.position) * penalty;
    }
    private GameObject FindCellWithLowestCost(List<GameObject> cells, Dictionary<GameObject,float> cellToCost)
    {
        float minCost = float.MaxValue;

        GameObject potencialCell = null;

        foreach(var cell in cells)
        {
            if(cellToCost.TryGetValue(cell, out float cost) && cost < minCost)
            {
                minCost = cost;
                potencialCell= cell;
            }
        }

        return potencialCell;
    }
    private GameObject FindCellWithLowestDistance(GameObject cell)
    {
        float minDistance = float.MaxValue;

        GameObject potentialCell = null;

        foreach(var cellNeighbore in _cellToAreaController[cell].GetNeighbores())
        {
            var currentDistance = Vector3.Distance(cell.transform.position, cellNeighbore.transform.position);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                potentialCell= cellNeighbore;
            }
        }

        return potentialCell;
    }
    private List<GameObject> ReconstructWay(Dictionary<GameObject, GameObject> cameFrom, GameObject currentCell)
    {
        List<GameObject> wayCells = new List<GameObject> { currentCell };

        while(cameFrom.ContainsKey(currentCell))
        {
            currentCell = cameFrom[currentCell];
            wayCells.Add(currentCell);
        }
        wayCells.Reverse();
        return wayCells;
    }

    public GameObject GetRandomCell() { return _cells[UnityEngine.Random.Range(0, _cells.Count)]; }
}
