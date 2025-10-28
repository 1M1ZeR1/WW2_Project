using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuUpgradeScript : MonoBehaviour
{
    //[SerializeField] private Transform listPanelTaker;

    //private Dictionary<string, (GameObject, GameObject)> buttonsByType = new();

    //[Header("Затемнение построеек")]
    //[SerializeField] private GameObject[] blackPanels;

    //protected GameObject _currentCell;

    //public void SetCurrentCell(GameObject cell) { _currentCell = cell; }
    //public void ShowAllBuildings()
    //{
    //    EnableInteractionAllButtons();

    //    CellBuildings cellBuildingsScript = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(_currentCell).GetParameter<CellBuildings>();

    //    var listOfTypes = buttonsByType.Keys.ToList();

    //    if (!cellBuildingsScript.beenBuildingBuilt[listOfTypes[0]])
    //    {
    //        buttonsByType[listOfTypes[0]].Item1.SetActive(true);
    //        buttonsByType[listOfTypes[0]].Item2.SetActive(false);

    //        foreach (var blackPanel in blackPanels) { blackPanel.SetActive(true); }
    //        return;
    //    }

    //    buttonsByType[listOfTypes[0]].Item1.SetActive(false);
    //    buttonsByType[listOfTypes[0]].Item2.SetActive(true);

    //    for (int i = 1;i < listOfTypes.Count; i++)
    //    {
    //        if (cellBuildingsScript.beenBuildingBuilt[listOfTypes[i]])
    //        {
    //            buttonsByType[listOfTypes[0]].Item1.SetActive(false);
    //            buttonsByType[listOfTypes[0]].Item2.SetActive(true);
    //        }
    //        else
    //        {
    //            buttonsByType[listOfTypes[0]].Item1.SetActive(true);
    //            buttonsByType[listOfTypes[0]].Item2.SetActive(false);
    //        }
    //    }
    //}

    //private void EnableInteractionAllButtons()
    //{
    //    foreach(var karteg in buttonsByType.Values)
    //    {
    //        karteg.Item1.GetComponent<UnityEngine.UI.Button>().interactable = true;
    //        karteg.Item2.GetComponent<UnityEngine.UI.Button>().interactable = true;
    //    }

    //    if (ServiceRegistry.WorkWithController<BuilderController>().cellsToBuildingObjects.ContainsKey(_currentCell))
    //    {
    //       var buttons = buttonsByType[ServiceRegistry.WorkWithController<BuilderController>().cellsToBuildingObjects[_currentCell].Item1];

    //        buttons.Item1.GetComponent<UnityEngine.UI.Button>().interactable = false;
    //        buttons.Item2.GetComponent<UnityEngine.UI.Button>().interactable = false;
    //    }
    //}
}
