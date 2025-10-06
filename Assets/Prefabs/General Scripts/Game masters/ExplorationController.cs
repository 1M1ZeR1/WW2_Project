using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorationController : MonoBehaviour
{
    protected enum TypeOfExploration
    {
        None,
        Squads,
        Buildings
    }

    [Header("Всплывающее уведомление")]
    [SerializeField] private GameObject messageTaker;
    private MessageScript messageScript;

    [Header("Панель выбора разведки")]
    [SerializeField] private GameObject explorationTypePanelObject;
    private ExplorationTypePanelScript explorationTypePanelScript;

    private Dictionary<GameObject, bool> cellInExplorationing = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, bool> cellHasExploration = new Dictionary<GameObject, bool>();

    private Dictionary<GameObject, List<AbstractSquad>> exploretedSquadsOnCell = new Dictionary<GameObject, List<AbstractSquad>>();
    private Dictionary<GameObject, List<AbstractBuildings>> exploretedBuildingsOnCell = new Dictionary<GameObject, List<AbstractBuildings>>();

    protected Dictionary<TypeOfExploration, int> _costOfExploration = new Dictionary<TypeOfExploration, int>() 
    {
        {TypeOfExploration.Squads, 20 },
        {TypeOfExploration.Buildings, 40 }
    };

    protected GameObject _currentWorkingCell;
    protected int _currentWorkingPoints;

    private void Start()
    {
        messageTaker.TryGetComponent(out messageScript);

        explorationTypePanelObject.TryGetComponent(out explorationTypePanelScript);
    }

    public void RequestToStartExploration(GameObject cell, int explorationPoints)
    {
        if (!CheckCellInExplorationing(cell))
        {
            if(!cellInExplorationing.ContainsKey(cell))cellInExplorationing.Add(cell, true);

            if (!cellHasExploration.ContainsKey(cell) || !cellHasExploration[cell]) 
            {
                explorationTypePanelScript.OpenExplorationTypePanel(false);
            }
            else
            {
                explorationTypePanelScript.OpenExplorationTypePanel(true);
            }

            explorationTypePanelScript.playerChosed += ExplorationTypePanelScript_playerChosed;

            _currentWorkingCell = cell;
            _currentWorkingPoints = explorationPoints;
        }
    }

    private void ExplorationTypePanelScript_playerChosed(int playerChosed)
    {
        if(playerChosed == 1)
        {
            ExplorationIsOvered(_currentWorkingCell, _currentWorkingPoints);
        }
        if(playerChosed == 2)
        {
            exploretedSquadsOnCell[_currentWorkingCell] = new List<AbstractSquad>();
            exploretedBuildingsOnCell[_currentWorkingCell] = new List<AbstractBuildings>();

            ExplorationIsOvered(_currentWorkingCell, _currentWorkingPoints);
        }

        _currentWorkingCell = null;
        _currentWorkingPoints = 0;

        explorationTypePanelScript.playerChosed -= ExplorationTypePanelScript_playerChosed;
    }

    public bool CheckCellInExplorationing(GameObject cell) {
        if (!cellInExplorationing.ContainsKey(cell))return false;
        return cellInExplorationing[cell]; }
    public bool CheckHasExploration(GameObject cell)
    {
        return cellHasExploration.ContainsKey(cell);
    }
    public void ExplorationIsOvered(GameObject cell, int exporationPoints)
    {
        int countOfBuildingExploration = UnityEngine.Random.Range(1, exporationPoints / _costOfExploration[TypeOfExploration.Buildings]+1);

        exporationPoints = countOfBuildingExploration * _costOfExploration[TypeOfExploration.Buildings];

        if (!exploretedBuildingsOnCell.ContainsKey(cell)){ exploretedBuildingsOnCell.Add(cell, new List<AbstractBuildings>()); }

        for(int i = 0; i < countOfBuildingExploration; i++)
        {
            var newExploratedBuild = cell.GetComponent<CellBuildings>().GetNewBuilding(exploretedBuildingsOnCell[cell]);

            if(newExploratedBuild == null)
            {
                exporationPoints += _costOfExploration[TypeOfExploration.Buildings];
            }
            else
            {
                exploretedBuildingsOnCell[cell].Add(newExploratedBuild);
            }
        }

        int countOfSquadsExploration = UnityEngine.Random.Range(1, exporationPoints / _costOfExploration[TypeOfExploration.Squads] + 1);

        if (!exploretedSquadsOnCell.ContainsKey(cell)) { exploretedSquadsOnCell.Add(cell, new List<AbstractSquad>()); }

        for(int i = 0; i < countOfSquadsExploration; i++)
        {
            var newExploratedSquad = ServiceRegistry.WorkWithController<GameController>().GetNewEnemySquad(exploretedSquadsOnCell[cell],cell);

            if(newExploratedSquad != null) { exploretedSquadsOnCell[cell].Add(newExploratedSquad); }
        }

        if(!cellHasExploration.ContainsKey(cell))cellHasExploration.Add(cell, true);

        cellInExplorationing[cell] = false;
    }
    public List<string[]> GetExplorationSummary(GameObject cell)
    {
        List<string[]> result = new List<string[]>
        {
            new string[]
            {
                TimeControllerScript.GetCurrentTime(),
                cell.GetComponent<CellTypeScript>().GetCellName(),
                "Гаврилов У.Е."
            }
        };

        if(!exploretedBuildingsOnCell.ContainsKey(cell) || exploretedBuildingsOnCell[cell] == null) 
        {
            result.Add(null);
        }
        else
        {
            List<string> infoBuildings = new List<string>
            {
                exploretedBuildingsOnCell[cell].Count.ToString()
            };

            foreach(var build in exploretedBuildingsOnCell[cell ])
            {
                infoBuildings.Add(build.Name);
            }

            result.Add(infoBuildings.ToArray());
        }

        if (!exploretedSquadsOnCell.ContainsKey(cell) || exploretedSquadsOnCell[cell] == null)
        {
            result.Add(null);
        }
        else
        {
            List<string> infoSquad = new List<string>
            {
                exploretedSquadsOnCell[cell].Count.ToString()
            };

            foreach (var squad in exploretedSquadsOnCell[cell])
            {
                infoSquad.Add(squad.Name);
            }

            result.Add(infoSquad.ToArray());
        }

        return result;
    }
}
