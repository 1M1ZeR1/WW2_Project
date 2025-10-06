using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeadquartersPanel : MonoBehaviour
{
    
    [SerializeField] private Transform contentTaker_Categories;
    private List<Transform> categoriesButtons = new();

    [SerializeField] private Transform contentTaker_CellPanel;

    [SerializeField] private Transform contentTaker_SquadPanel;

    [SerializeField] private Transform contentTaker_BuildPanel;
    private List<Transform> buildPanels = new();

    [SerializeField] private GameObject headquartersPanel;

    protected GameObject _currentWorkingCell;

    protected Dictionary<GameObject, GameObject> cellToCellPanel = new();
    protected Dictionary<HeadquartersBuild, bool> headquartersToCellsPanelCreated = new();

    private enum Categories
    {
        None,
        Squads,
        Builds,
        Headquarters
    }

    private Categories currentCategory = Categories.None;

    private void Start()
    {
        ServiceRegistry.WorkWithController<ObjectsFactory>().SaveObject<HeadquartersPanel>(contentTaker_CellPanel.transform.GetChild(0).gameObject);

        SavePanels();
        BindButtons();

        headquartersPanel.SetActive(false);
    }
    private void SavePanels()
    {
        foreach (Transform categoriesBut in contentTaker_Categories){ categoriesButtons.Add(categoriesBut); }
        //foreach (Transform buildPanel in contentTaker_BuildPanel){ buildPanels.Add(buildPanel); }
    }
    private void BindButtons()
    {
        categoriesButtons[0].GetComponent<Button>().onClick.AddListener(SquadsCategories_Bind);
    }



    public void OpenWindowForCell(GameObject cell)
    {
        if (!Switcher()) return;

        ServiceRegistry.WorkWithController<CellUIScript>().HideInformationPanel();

        DisableContentTakers();

        if (_currentWorkingCell != cell) { _currentWorkingCell = cell; CreateCellPanels(); CloseCellPanels(); currentCategory = Categories.None; }

        ShowCellPanels();
    }

    private bool Switcher()
    {
        if (headquartersPanel.activeSelf) { headquartersPanel.SetActive(false); CameraMovementScript.UnBlockMovement(); }
        else { headquartersPanel.SetActive(true); CameraMovementScript.BlockMovement(); }

        return headquartersPanel.activeSelf;
    }
    private void DisableContentTakers()
    {
        contentTaker_CellPanel.gameObject.SetActive(true);
        contentTaker_SquadPanel.gameObject.SetActive(false);
        //contentTaker_BuildPanel.gameObject.SetActive(false);
    }

    private void WorkByCategory(GameObject cell = null)
    {
        switch (currentCategory)
        {
            case Categories.Squads:
                contentTaker_SquadPanel.gameObject.SetActive(true);

                ServiceRegistry.WorkWithController<CellUIScript>().ClearList(contentTaker_SquadPanel);

                if (cell == null)
                {
                    ServiceRegistry.WorkWithController<CellUIScript>().SetParent(contentTaker_SquadPanel, _currentWorkingCell);
                    ServiceRegistry.WorkWithController<CellUIScript>().UpdatePanelInfo(_currentWorkingCell);
                }
                else
                {
                    ServiceRegistry.WorkWithController<CellUIScript>().SetParent(contentTaker_SquadPanel, cell);
                    ServiceRegistry.WorkWithController<CellUIScript>().UpdatePanelInfo(cell);
                }
                break;
            case Categories.None:
                DisableContentTakers();
                break;
        }       
    }


    //Buttons binds
    private void CreateCellPanels()
    {
        HeadquartersBuild headquartersBuild = (HeadquartersBuild)ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(_currentWorkingCell).
            GetParameter<CellBuildings>().builds[BuildsEnum.HeadQuarters];

        if (headquartersToCellsPanelCreated.ContainsKey(headquartersBuild) && headquartersToCellsPanelCreated[headquartersBuild]) return;

        foreach (var cell in headquartersBuild.cellsInArea)
        {
            
            if(!cellToCellPanel.ContainsKey(cell))cellToCellPanel.Add(cell,ServiceRegistry.WorkWithController<ObjectsFactory>().CreateObject<HeadquartersPanel>(contentTaker_CellPanel));

            string name = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellDiscription>().GetCellName();

            cellToCellPanel[cell].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;

            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick,
            };
            entry.callback.AddListener((data) =>
            {
                WorkByCategory(cell);
            });

            cellToCellPanel[cell].GetComponent<EventTrigger>().triggers.Add(entry);


            cellToCellPanel[cell].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                ServiceRegistry.WorkWithController<FocusOnCellScript>().FocusToCell(cell,headquartersPanel);
            });
        }

        headquartersToCellsPanelCreated.Add(headquartersBuild,true);
    }
    private void ShowCellPanels()
    {
        HeadquartersBuild headquartersBuild = (HeadquartersBuild)ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(_currentWorkingCell).
            GetParameter<CellBuildings>().builds[BuildsEnum.HeadQuarters];

        foreach (var cell in headquartersBuild.cellsInArea)
        {
            if (!ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(cell)) continue;
            cellToCellPanel[cell].SetActive(true);
        }
    }
    private void CloseCellPanels()
    {
        foreach(var panel in cellToCellPanel.Values) { panel.SetActive(false); }
    }

    private void SquadsCategories_Bind()
    {
        currentCategory = Categories.Squads;

        WorkByCategory();
    }
}
