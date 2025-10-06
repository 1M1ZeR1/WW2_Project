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
    public List<GameObject> CreateWay(GameObject startCell, GameObject endCell)
    {
        // Проверка на доступность конечной клетки
        if (!ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(endCell).GetParameter<CellArea>().IsAllies())
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
                // Пропускаем недоступные клетки или клетки врага
                if (!ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(neighbor).GetParameter<CellArea>().IsAllies() || closedSet.Contains(neighbor))
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

        // Если путь не найден
        return null;
    }
    public List<GameObject> CreateWay_Enemy(GameObject startCell, GameObject endCell)
    {
        // Проверка на доступность конечной клетки
        if (!ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(endCell).GetParameter<CellArea>().IsAllies())
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
                // Пропускаем недоступные клетки или клетки врага
                if (closedSet.Contains(neighbor))
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

        // Если путь не найден
        return null;
    }

    private IEnumerator FindGoodCellHelper(GameObject startCell, float maxDistance, SideEnum sideEnum)
    {
        while (true) 
        {
            var cellToAttack = _cells[UnityEngine.Random.Range(0, _cells.Count)];

            if (Vector3.Distance(cellToAttack.transform.position, startCell.transform.position) > maxDistance) continue;


        }
    }
    private List<GameObject> GetAttackWay(GameObject startCell,GameObject endCell,SideEnum sideEnum)
    {
        Dictionary<GameObject,GameObject> wayList = new Dictionary<GameObject, GameObject> {};

        GameObject currentCell = startCell;

        while (true) 
        {
            var nextCell = FindCellWithLowestDistance(currentCell);

            wayList.Add(nextCell, currentCell);

            currentCell = nextCell;

            if(currentCell == endCell)
            {
                return ReconstructWay(wayList, endCell);
            }
        }

        // Если путь не найден
        return null;
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
