using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeadquartersPanel : MonoBehaviour
{
    [SerializeField]private UIDragLine dragLine;

    [SerializeField] private Button buttonOpenPanel;

    [SerializeField] private Transform buttonShowCellsInArea;

    [SerializeField] private Transform contentTaker_Categories;
    private List<Transform> categoriesButtons = new();

    [SerializeField] private Transform contentTaker_CellPanel;

    [SerializeField] private Transform contentTaker_SquadPanel;
    private Transform squadPanelList;

    [SerializeField] private Transform contentTaker_BuildPanel;
    private Transform buildPanelList;
    private List<BuildItemPanel> buildPanels = new();

    [SerializeField] private Transform contentTaker_HardCellPanel;
    private Transform hardCellPanelList;
    [SerializeField] private TextMeshProUGUI text_countOfHardCells;

    [SerializeField] private GameObject headquartersPanel;

    protected GameObject _currentWorkingCell;
    protected HeadquartersBuild _currentWorkingHeadquarters = null;

    protected bool _inHardMode = false;

    protected Dictionary<GameObject, GameObject> cellToCellPanel = new();
    protected Dictionary<HeadquartersBuild, bool> headquartersToCellsPanelCreated = new();
    protected Dictionary<HeadquartersBuild, List<GameObject>> headquartersToHardCellsPanel = new();

    private enum Categories
    {
        None,
        Squads,
        Builds,
        Headquarters
    }

    private Categories currentCategory = Categories.None;
    private bool cellIsChanged = false;

    private void Start()
    {
        ServiceRegistry.WorkWithService<ObjectsFactory>().SaveObject<HeadquartersPanel>(contentTaker_CellPanel.transform.GetChild(0).gameObject);

        SavePanels();
        squadPanelList = contentTaker_SquadPanel.transform.parent.parent;
        buildPanelList = contentTaker_BuildPanel.transform.parent.parent;
        hardCellPanelList = contentTaker_HardCellPanel.transform.parent.parent;

        BindButtons();

        headquartersPanel.SetActive(false);

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject>((sender, cell) =>
        {
            ChangeWorkingCell(cell);

            if (_currentWorkingHeadquarters != null)
            {
                if (headquartersToHardCellsPanel.ContainsKey(_currentWorkingHeadquarters)) { 
                    foreach (var panel in headquartersToHardCellsPanel[_currentWorkingHeadquarters])
                    {
                        panel.SetActive(false);
                    }
                }
                _currentWorkingHeadquarters = null;
            }

            if (ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(_currentWorkingCell))
            {
                buttonOpenPanel.interactable = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(_currentWorkingCell).GetParameter<CellBuildings>().CheckBuildIsBuilt("HeadquartersBuild");
            }
            else { buttonOpenPanel.interactable = false; }
        });

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<CellBuildings, GameObject, AbstractBuildings>((sender, cell, build) =>
        {
            Debug.Log($"{this}: Get event from {sender}({cell}) with {build}");
            if(build.GetType() == typeof(HeadquartersBuild)) CreateCellPanels((HeadquartersBuild)build);
        });


        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown,
        };
        entry.callback.AddListener((data) =>
        {
            ServiceRegistry.WorkWithService<EventBus>().Publish<HeadquartersPanel, List<GameObject>, List<GameObject>>(this,
                _currentWorkingHeadquarters.CellsInArea, _currentWorkingHeadquarters.hardCells);
        });

        buttonShowCellsInArea.GetComponent<EventTrigger>().triggers.Add(entry);
    }
    private void SavePanels()
    {
        foreach (Transform categoriesBut in contentTaker_Categories){ categoriesButtons.Add(categoriesBut); }
    }
    private void BindButtons()
    {
        categoriesButtons[0].GetComponent<Button>().onClick.AddListener(() => { Button_Bind(Categories.Squads); });
        categoriesButtons[1].GetComponent<Button>().onClick.AddListener(() => { Button_Bind(Categories.Builds); });
        categoriesButtons[2].GetComponent<Button>().onClick.AddListener(() => { Button_Bind(Categories.Headquarters); });

        buttonOpenPanel.onClick.AddListener(() =>
        {
            if (buildPanels.Count == 0) { foreach (Transform panel in contentTaker_BuildPanel) { buildPanels.Add(panel.gameObject.GetComponent<BuildItemPanel>()); } }
            OpenWindow();
        });
    }



    public void ChangeWorkingCell(GameObject cell)
    {
        _currentWorkingCell = cell; cellIsChanged = true;
    }
    public void OpenWindow()
    {
        if (!Switcher()) return;

        ServiceRegistry.WorkWithController<CellUIScript>().HideInformationPanel();

        DisableContentTakers();

        if (cellIsChanged) { CloseCellPanels(); currentCategory = Categories.None; cellIsChanged = false; }
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
        squadPanelList.gameObject.SetActive(false);
        buildPanelList.gameObject.SetActive(false);
        hardCellPanelList.gameObject.SetActive(false);
    }

    private void WorkByCategory(GameObject cell = null)
    {
        DisableContentTakers();

        dragLine.DisableHardCellMode();
        _inHardMode = false;

        switch (currentCategory)
        {
            case Categories.Squads:
                squadPanelList.gameObject.SetActive(true);

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
            case Categories.Builds:
                buildPanelList.gameObject.SetActive(true);

                if(cell == null) {
                    ServiceRegistry.WorkWithController<AlliesSpawner>().currentWorkingCell = _currentWorkingCell;
                    foreach (var panel in buildPanels) { panel.GetComponent<BuildItemPanel>().CheckBuildForButtonState(_currentWorkingCell); } }
                else {
                    ServiceRegistry.WorkWithController<AlliesSpawner>().currentWorkingCell = cell;
                    foreach (var panel in buildPanels) { panel.GetComponent<BuildItemPanel>().CheckBuildForButtonState(cell); } }


                break;
            case Categories.Headquarters:
                hardCellPanelList.gameObject.SetActive(true);

                dragLine.EnableHardCellMode();
                _inHardMode = true;

                OpenHardCategory();

                break;
            case Categories.None:
                DisableContentTakers();
                break;
        }       
    }

    private void CreateHardCellPanel(GameObject cellToHard)
    {
        if (!headquartersToHardCellsPanel.ContainsKey(_currentWorkingHeadquarters)) headquartersToHardCellsPanel[_currentWorkingHeadquarters] = new();

        GameObject newPanel = Instantiate(cellToCellPanel[cellToHard], contentTaker_HardCellPanel);

        EventTrigger triggersPanel = newPanel.GetComponent<EventTrigger>();

        for(int i = triggersPanel.triggers.Count-1; i > 0; i--)
        {
            if (triggersPanel.triggers[i].eventID == EventTriggerType.EndDrag)
            {
                triggersPanel.triggers.RemoveAt(i);
                break;
            }
        }

        EventTrigger.Entry endDrag = new EventTrigger.Entry
        {
            eventID = EventTriggerType.EndDrag,
        };
        endDrag.callback.AddListener((data) => {
            if (!RectTransformUtility.RectangleContainsScreenPoint(hardCellPanelList.GetComponent<RectTransform>(), Input.mousePosition, Camera.main))
            {
                if (TryRemoveFromHardCell(cellToHard))
                {
                    UpdateText();
                    Destroy(newPanel);
                }

            }
        });
        triggersPanel.triggers.Add(endDrag);

        BindFocus(cellToHard, newPanel.transform.GetChild(1).GetComponent<Button>());

        headquartersToHardCellsPanel[_currentWorkingHeadquarters].Add(newPanel);
    }
    private bool TryAddToHardCell(GameObject cellToHard)
    {

        if (_currentWorkingHeadquarters.TryAddToHardCells(cellToHard))
        {
            UpdateText();
            CreateHardCellPanel(cellToHard);

            return true;
        }
        return false;
    }
    private bool TryRemoveFromHardCell(GameObject cellToRemove)
    {
        if(_currentWorkingHeadquarters.TryRemoveFromHardCells(cellToRemove))return true;

        return false;
    }
    private void UpdateText()
    {
        text_countOfHardCells.text = $"{_currentWorkingHeadquarters.GetCountOfHardCells()}/{_currentWorkingHeadquarters.MaxCountOfHardCells}";
    }
    private void OpenHardCategory()
    {
        UpdateText();

        foreach (var panel in headquartersToHardCellsPanel[_currentWorkingHeadquarters])
        {
            panel.SetActive(true);
        }
    }

    //Buttons binds
    private void CreateCellPanels(HeadquartersBuild build)
    {
        foreach (var cell in build.CellsInArea)
        {
            
            if(!cellToCellPanel.ContainsKey(cell))cellToCellPanel.Add(cell,ServiceRegistry.WorkWithService<ObjectsFactory>().CreateObject<HeadquartersPanel>(contentTaker_CellPanel));

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


            EventTrigger.Entry endDrag = new EventTrigger.Entry
            {
                eventID = EventTriggerType.EndDrag,
            };
            endDrag.callback.AddListener((data) => {
                if (!_inHardMode) return;
                if (RectTransformUtility.RectangleContainsScreenPoint(hardCellPanelList.GetComponent<RectTransform>(),Input.mousePosition,Camera.main)) 
                {
                    TryAddToHardCell(cell);
                } });


            cellToCellPanel[cell].GetComponent<EventTrigger>().triggers.Add(entry);
            cellToCellPanel[cell].GetComponent<EventTrigger>().triggers.Add(endDrag);

            BindFocus(cell,cellToCellPanel[cell].transform.GetChild(1).GetComponent<Button>());
        }

        headquartersToCellsPanelCreated.Add(build,true);

    }

    private void ShowCellPanels()
    {
        _currentWorkingHeadquarters = (HeadquartersBuild)ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(_currentWorkingCell).
            GetParameter<CellBuildings>().builds["HeadquartersBuild"];

        foreach (var cell in _currentWorkingHeadquarters.CellsInArea)
        {
            if (!ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(cell)) continue;
            cellToCellPanel[cell].SetActive(true);
        }
    }
    private void CloseCellPanels()
    {
        foreach(var panel in cellToCellPanel.Values) { panel.SetActive(false); }
    }

    private void Button_Bind(Categories category)
    {
        currentCategory = category;

        WorkByCategory();
    }

    private void BindFocus(GameObject cell, Button buttonToBind)
    {
        buttonToBind.onClick.AddListener(() =>
        {
            ServiceRegistry.WorkWithController<FocusOnCellScript>().FocusToCell(cell, headquartersPanel);
        });
    }
}
