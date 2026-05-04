using SharpVoronoiLib;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class LandscapeCreater : MonoBehaviour
{
    [SerializeField] private Material testWater;
    [SerializeField] private Material testSand;
    [SerializeField] private Material testMountains;
    [SerializeField] private Material testRock;

    [Header("Кол-во гор на карте")]
    [SerializeField] private int countOfMountains;

    [Header("Кол-во озёр на карте")]
    [SerializeField] private int countOfLakes;

    [Header("Кол-во рек на карте")]
    [SerializeField] private int countOfRivers;

    protected Dictionary<GameObject, CellTypeScript> _cellToType;
    protected Dictionary<GameObject, CellArea> _cellToArea;

    protected HashSet<GameObject> _cellWeCantUse = new HashSet<GameObject>();

    protected GameObject candidate;

    public void CreateLandScape(Dictionary<GameObject,CellTypeScript> cellTotype, Dictionary<GameObject,CellArea> cellToArea)
    {
        _cellToArea = cellToArea;
        _cellToType = cellTotype;

        for(int  i = 0; i < countOfRivers ; i++)
        {
            RiversGenerator(FindObjectWithMaxHeight());
        }

        for (int i = 0; i < countOfLakes; i++)
        {
            LakesGenerator(FindObjectWithMinHeight());
        }
    }
    private void RiversGenerator(GameObject riverBeggining)
    {
        HashSet<GameObject> allreadyRivers = new HashSet<GameObject>() {};

        GameObject currentCell = riverBeggining;

        bool haveContinuation;

        while (true)
        {
            _cellToType[currentCell].SetTypeCell(CellTypes_Enum.River);
            currentCell.GetComponent<MeshRenderer>().material = testWater;

            float currentCellHeight = _cellToType[currentCell].GetHeight();

            haveContinuation = false;

            float maxHeight = float.MinValue;

            foreach(var cellNeighbor in _cellToArea[currentCell].GetNeighbores())
            {
                float heightNeighbor = _cellToType[cellNeighbor].GetHeight();

                if(heightNeighbor > maxHeight && heightNeighbor < currentCellHeight && !_cellWeCantUse.Contains(cellNeighbor))
                {
                    haveContinuation = true;

                    maxHeight = heightNeighbor;
                    candidate = cellNeighbor;

                }
            }
            if(haveContinuation)
            {
                allreadyRivers.Add(currentCell);
                currentCell = candidate;
            }
            else
            {
                allreadyRivers.Add(currentCell);
                _cellWeCantUse.Add(currentCell);
                break;
            }
            _cellWeCantUse.Add(currentCell);
        }

        foreach (var riverCell in allreadyRivers)
        {
            foreach(var beaches in _cellToArea[riverCell].GetNeighbores())
            {
                if (_cellToType[beaches].IsRiver()) { continue; }
                else
                {
                    _cellToType[beaches].SetTypeCell(CellTypes_Enum.Beach);
                    beaches.GetComponent<MeshRenderer>().material = testSand;

                    _cellWeCantUse.Add(beaches);
                }
            }
        }
    }
    private GameObject FindObjectWithMaxHeight()
    {
        float maxHeight = float.MinValue;

        foreach(var cell in _cellToType)
        {//ВОЗМОЖЕН БАГ!!!
            if(cell.Value.GetHeight() > maxHeight && !_cellWeCantUse.Contains(cell.Key))
            {
                maxHeight = cell.Value.GetHeight();
                candidate =  cell.Key;
            }
        }
        return candidate;
    }
    private GameObject FindObjectWithMinHeight()
    {
        
        while (true)
        {
            GameObject cell = _cellToType.Keys.ElementAt(Random.Range(0, _cellToType.Count));
            if (!_cellWeCantUse.Contains(cell))
            {
                candidate = cell; break;
            }
        }

        return candidate;
    }

    private void LakesGenerator(GameObject lakeCell)
    {
        if ((_cellToType[lakeCell].IsBeach() || _cellToType[lakeCell].IsRiver() || _cellToType[lakeCell].IsLake()) && !_cellWeCantUse.Contains(lakeCell))
        {
            _cellWeCantUse.Add(lakeCell);

            LakesGenerator(FindObjectWithMinHeight());
            return;
        }
        
        bool secondCheck = true;

        List<GameObject> candidates = new List<GameObject>();
        foreach (var cell in _cellToArea[lakeCell].GetNeighbores())
        {
            if (_cellToType[lakeCell].IsRiver() || _cellToType[lakeCell].IsLake()) { secondCheck = false;_cellWeCantUse.Add(cell); break; }
            candidates.Add(cell);
        }
        if (!secondCheck) {
            _cellWeCantUse.Add(lakeCell);
            LakesGenerator(FindObjectWithMinHeight()); 
            return; 
        }
        _cellWeCantUse.Add(lakeCell);
        foreach(var cell in candidates)
        {
            _cellToType[cell].SetTypeCell(CellTypes_Enum.Beach);
            _cellWeCantUse.Add(cell);
            cell.GetComponent<MeshRenderer>().material = testSand;
        }
        lakeCell.GetComponent<MeshRenderer>().material = testWater;
    }
}
