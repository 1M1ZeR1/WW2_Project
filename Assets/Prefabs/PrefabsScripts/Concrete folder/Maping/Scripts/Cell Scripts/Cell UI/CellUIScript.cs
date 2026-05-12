using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CellUIScript : MonoBehaviour
{
    [Header("Главное UI окно")]
    [SerializeField] private GameObject informationPanel;

    [Header("Контроллер навыков")]
    [SerializeField] private GameObject skillControllerObject;
    private SkillController skillControllerScript;

    private CameraMovementScript cameraMovementScript;

    [Header("Панель тренировок")]
    [SerializeField] private GameObject trainingPanel;
    protected TrainingPanelScript _trainingPanelScript;
    [SerializeField] private SquadsPresetPanelScript _squadsPresetPanelScript;


    [Header("Держатель типовых бафов")]
    [SerializeField] private GameObject buffsTaker;

    [Header("Типовые баффы")]
    [SerializeField] private GameObject[] buffsCirclesObjects;

    [Header("Изображение типов")]
    [SerializeField] private GameObject[] typeImages;

    [SerializeField] private TextMeshProUGUI countCurrentMaxSquads;

    public PanelSelection PanelSelection { get; private set; }

    void Start()
    {
        PanelSelection = new PanelSelection(PanelSelection.SelectionMode.Sizing);

        foreach(var cell in ServiceRegistry.WorkWithController<GameController>().GetGlobalList())
        {
            var newCellPanelObject = new CellPanel(cell, this);

            cellsAndTheirPanels.Add(cell, newCellPanelObject);
            
        }

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject, bool>((sender, cell, UI_resolution) =>
        {
            if (!UI_resolution) return;
            ShowInformation(cell);
        });

        trainingPanel.TryGetComponent(out _trainingPanelScript);

        cameraMovementScript = GetComponent<CameraMovementScript>();

        skillControllerObject.TryGetComponent(out skillControllerScript);

        ServiceRegistry.WorkWithController<BuilderController>().RequestUIUpdate += UpdateByBuildingsController;

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<GeneralUiWindowsManager, GameObject, GameObject>((sender, taker, cell) =>
        {
            if(taker == informationPanel.gameObject)_currentInteractionCell = cell;
        });
    }

    private TextMeshProUGUI nameCell;

    private TextMeshProUGUI typeCell;

    private GameObject scrollViewerSquads;

    public void ShowInformation(GameObject cell)
    {
        HideTrainingPanel();

        if (!ServiceRegistry.WorkWithService<CellAccessibilityValidator>().InteractWithAlliesCell(cell)) 
        {
            if (informationPanel.activeSelf) { informationPanel.SetActive(false);  }

            return; 
        }

        if (!informationPanel.activeSelf){informationPanel.SetActive(true);  }
        if (nameCell == null || typeCell == null){ GetInformationComponents();}

        if(_currentInteractionCell == cell) { informationPanel.SetActive(false);  nameCell.text = "";_currentInteractionCell = null; return; }

        PanelSelection.ClearSelections();

        SetParent(panelSquad.transform.parent, cell);

        PrintInformation(nameCell, $"{ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellDiscription>().GetCellName()}");
        PrintInformation(typeCell, $"Тип: {ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellDiscription>().GetCellType()}");
        SetTypeImage(ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellType>().CellTypeObject);

        GetBuffsCell(cell);

        ClearList(scrollViewerSquads.transform);

        if (ServiceRegistry.WorkWithService<CellAccessibilityValidator>().InteractWithAlliesCell(cell))
        {
            scrollViewerSquads.SetActive(true);

            ShowSquadPanels(cell);
        }
        else
        {
            if (scrollViewerSquads.activeSelf == true)
            {
                scrollViewerSquads.SetActive(false);
            }
        }

        UpdateCountCurrentMaxSquads(cell);

        _currentInteractionCell = cell;
    }
    public void UpdateByMovementController(GameObject cell)
    {
        if(cell == _currentInteractionCell) 
        {
            UpdateCountCurrentMaxSquads(_currentInteractionCell);

            ShowSquadPanels(_currentInteractionCell);
        }
    }

    public void UpdateByBuildingsController(GameObject cell)
    {
        if (cell != _currentInteractionCell) return;

        ClearList(scrollViewerSquads.transform);

        if (ServiceRegistry.WorkWithService<CellAccessibilityValidator>().InteractWithAlliesCell(_currentInteractionCell))
        {
            if (scrollViewerSquads.activeSelf == false)//Возможна ошибка
            {
                scrollViewerSquads.SetActive(true);
            }
            ShowSquadPanels(_currentInteractionCell);
        }
        else
        {
            if (scrollViewerSquads.activeSelf == true)
            {
                scrollViewerSquads.SetActive(false);
            }
        }
    }
    private void GetInformationComponents()
    {
        nameCell = informationPanel.transform.Find("Name/Cell Name").GetComponent<TextMeshProUGUI>();

        typeCell = informationPanel.transform.Find("TypeAndBuffsPanel/Type").transform.GetComponent<TextMeshProUGUI>();
        scrollViewerSquads = informationPanel.transform.Find("Squads/Viewport/Content").gameObject;
    }
    private void PrintInformation(TextMeshProUGUI textMesh, string information)
    {
        textMesh.text = information;
    }
    private void SetTypeImage(CellTypes_Enum type)
    {
        foreach (var item in typeImages){item.SetActive(false);}

        switch (type)
        {
            case CellTypes_Enum.Plain:
                typeImages[0].SetActive(true); break;
            case CellTypes_Enum.Forest:
                typeImages[1].SetActive(true); break;
        }
    }


    private void ShowSquadPanels(GameObject cell)
    {
        cellsAndTheirPanels[cell].ShowInformation();
    }
    public void ClearList(Transform transformToClean)
    {
        foreach (Transform child in transformToClean)
        {
            if (child.name.Contains("(Clone)"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }
    private void GetBuffsCell(GameObject cell)
    {
        for (int i = buffsTaker.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = buffsTaker.transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                Destroy(child.gameObject);
            }
        }

        var cellBuffs = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuffs>();

        foreach(var buff in cellBuffs.GetAllCellBuffs())
        {
            if(buff.GetBuffTypes() == BuffTypes.Territory)
            {
                GameObject newCircle = Instantiate(buffsCirclesObjects[0],buffsTaker.transform);

                newCircle.GetComponent<BuffCircleScript>().SetBuffForDiscription(buff);

                newCircle.SetActive(true);
            }
            if (buff.GetBuffTypes() == BuffTypes.Building)
            {
                GameObject newCircle = Instantiate(buffsCirclesObjects[1], buffsTaker.transform);

                newCircle.GetComponent<BuffCircleScript>().SetBuffForDiscription(buff);

                newCircle.SetActive(true);
            }
            if(buff.GetBuffTypes() == BuffTypes.Event)
            {
                GameObject newCircle = Instantiate(buffsCirclesObjects[2], buffsTaker.transform);

                newCircle.GetComponent<BuffCircleScript>().SetBuffForDiscription(buff);

                newCircle.SetActive(true);
            }
        }
    }

    public void HideInformationPanel() { informationPanel.SetActive(false); }

    //Панели с отрядами

    [Header("Главная UI панель отряда")]
    [SerializeField]private GameObject panelSquad;

    protected Dictionary<GameObject, CellPanel> cellsAndTheirPanels = new Dictionary<GameObject, CellPanel>();
    public GameObject CreateSquadPanel()
    {
        if (nameCell == null || typeCell == null)
        {
            GetInformationComponents();
        }
        GameObject otherPanel = Instantiate(panelSquad,scrollViewerSquads.transform);

        return otherPanel;
    }

    public void HelpToSwipeSquadPanel(SquadPanel squadPanel,GameObject toCell)
    {
        cellsAndTheirPanels[toCell].SwipeSquadPanel(squadPanel);
    }

    public void HelpToSwipeSquadPanel_Outside(AbstractSquad squad,GameObject fromCell,GameObject toCell)
    {
        cellsAndTheirPanels[fromCell].SwipeSquad_Outside(squad, cellsAndTheirPanels[toCell]);
    }

    public void UpdatePanelInfo(GameObject cell)
    {
        cellsAndTheirPanels[cell].ShowInformation();
    }

    protected GameObject _currentInteractionCell;
    public void OpenTrainingPanel()
    {
        trainingPanel.SetActive(true);

        //_trainingPanelScript.GetAllTrainingSquads(_currentInteractionCell);
        //_squadsPresetPanelScript.ShowList();
    }
    public void HideTrainingPanel()
    {
        if(trainingPanel.activeSelf == true)
        {
            PauseScript.SetGameState(GameState.Play);
            trainingPanel.SetActive(false);
        }
    }

    private void UpdateCountCurrentMaxSquads(GameObject cell)
    {
        countCurrentMaxSquads.text = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellDiscription>().GetCurrentMaxCountOfSquads_String();
    }

    public void SetParent(Transform parent, GameObject cell)
    {
        cellsAndTheirPanels[cell].SwipeParent(parent);
    }
}

public class CellPanel
{
    protected GameObject _currentCell;

    private Dictionary<AbstractSquad, SquadPanel> squadsObjects = new Dictionary<AbstractSquad, SquadPanel>();
    private Dictionary<SquadPanel, bool> squadsObjectsStateUse = new Dictionary<SquadPanel, bool>();

    private Func<GameObject> createSquadPanelFromController;

    private Action<SquadPanel,GameObject> swipeHelperFromController;
    private Action<GameObject> invokeUpdateEvent;

    public CellPanel(GameObject cell, CellUIScript cellUIScript)
    {
        _currentCell = cell;

        GameController gameController = ServiceRegistry.WorkWithController<GameController>();



        gameController.dictionaryUpdatedEvent += UpdateSquadsPanels;
        gameController.dictionaryIncreased += AddSquadsPanels;

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<GameController, int, AbstractSquad>((sender, var, squad) =>
        {
            if (var == 1) { SwipeStateSquad(squad); }
            if (var == 2) { RemoveSquadFromListCell(squad); }
            if (var == 3) { RemoveSquadWithSave(squad); }
        });

        createSquadPanelFromController = cellUIScript.CreateSquadPanel;
        invokeUpdateEvent = cellUIScript.UpdatePanelInfo;
        swipeHelperFromController = (SquadPanel squadPanel, GameObject cellTo) => { cellUIScript.HelpToSwipeSquadPanel(squadPanel,cellTo); };
    }

    protected void AddSquadToListCell(AbstractSquad squad)
    {
        var newCreatedPanel = new SquadPanel(createSquadPanelFromController.Invoke(), _currentCell, squad, this);

        squadsObjects.Add(squad,newCreatedPanel);
        squadsObjectsStateUse.Add(newCreatedPanel, true);

        newCreatedPanel.InstantiatePanel();
    }

    protected void RemoveSquadFromListCell(AbstractSquad squad)
    {
        if (squadsObjects.ContainsKey(squad))
        {
            squadsObjectsStateUse.Remove(squadsObjects[squad]);
            squadsObjects.Remove(squad);
        }
    }

    protected void RemoveSquadWithSave(AbstractSquad squad)
    {
        if (squadsObjects.ContainsKey(squad))
        {
            SwipeStateSquad(squad);

            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(_currentCell).GetParameter<CellSquadsOnArea>().TryRemoveSquad(squad);
        }
    }

    protected void AddSquadToListCell_Simple(SquadPanel squadPanel) 
    { 
        squadsObjects.Add(squadPanel.SquadOfThisPanel, squadPanel); 
        squadsObjectsStateUse.Add(squadPanel, true); 
        squadPanel.ChangeCurrentCell(_currentCell);
    }

    private void UpdateSquadsPanels(AbstractSquad squad, GameObject cellFrom, GameObject cellTo)
    {
        if (squadsObjects.Keys.Contains(squad) && cellFrom == _currentCell)
        {
            swipeHelperFromController(squadsObjects[squad], cellTo);
            RemoveSquadFromListCell(squad);
        }
    }
    private void AddSquadsPanels(AbstractSquad squad, GameObject cell)
    {
        if(cell == _currentCell)
        {
            AddSquadToListCell(squad);
        }
    }

    public void SwipeSquadPanel(SquadPanel squadPanel)
    {
        AddSquadToListCell_Simple(squadPanel);
    }

    public void ShowInformation()
    {
        foreach(var squad in squadsObjects)
        {
            squad.Value.SwitchActiveState(squadsObjectsStateUse[squad.Value]);
        }
    }

    private void SwipeStateSquad(AbstractSquad squad)
    {
        if (squadsObjects.ContainsKey(squad))
        {
            if (squadsObjectsStateUse[squadsObjects[squad]]) DisenableSquadPanel(squadsObjects[squad]);
            else { EnableSquadPanel(squadsObjects[squad]); }

            invokeUpdateEvent.Invoke(_currentCell);   
        }
    }

    public void EnableSquadPanel(SquadPanel squadPanel)
    {
        squadsObjectsStateUse[squadPanel] = true;
    }
    public void DisenableSquadPanel(SquadPanel squadPanel)
    {
        squadsObjectsStateUse[squadPanel] = false;
    }

    public void SwipeParent(Transform transform)
    {
        foreach (var squad in squadsObjects)
        {
            squad.Value.SwipeParent(transform);
        }
    }

    public void SwipeSquad_Outside(AbstractSquad squad, CellPanel toCellPanel)
    {
        toCellPanel.SwipeSquadPanel(squadsObjects[squad]);

        RemoveSquadFromListCell(squad);
    }
}
public class SquadPanel : IUIConstructor
{
    private GameObject squadPanel, currentCell, actionPanelBlock;

    public AbstractSquad SquadOfThisPanel { get; private set; }

    private SquadPanelScript squadPanelScript;

    private UnityEngine.UI.Button skillButton, actionButton, informationButton;

    private UnityEngine.UI.Image blackPanelSkill;

    public SquadPanel(GameObject createdPanel, GameObject currentCell, AbstractSquad squadForPanel, CellPanel cellPanel)
    {
        squadPanel = createdPanel;
        this.currentCell = currentCell;
        SquadOfThisPanel = squadForPanel;

        createdPanel.TryGetComponent(out squadPanelScript);

        ChangeInformation(squadForPanel);
    }

    public void InstantiatePanel()
    {
        skillButton = squadPanel.transform.Find("SquadSkillPanel/SkillButton").GetComponent<UnityEngine.UI.Button>();
        actionButton = squadPanel.transform.Find("Action").GetComponent<UnityEngine.UI.Button>();
        informationButton = squadPanel.transform.Find("ShowMore").GetComponent<UnityEngine.UI.Button>();

        var eventTrigger_Sprite = squadPanel.transform.Find("Sprite").GetComponent<EventTrigger>();
        squadPanel.transform.Find("Sprite").GetComponent<UnityEngine.UI.Image>().sprite = ServiceRegistry.WorkWithService<MonobehaviourHandler>().
            WorkWithSpriteDatabase("squads").GetSprite(SquadOfThisPanel.Name);


        skillButton.onClick.AddListener(() => { squadPanelScript.BintButton_UseSkill(SquadOfThisPanel); });
        actionButton.onClick.AddListener(() => {
            squadPanelScript.BindButton_Interaction(currentCell, SquadOfThisPanel);
        });
        informationButton.onClick.AddListener(() => { squadPanelScript.BindButton_ShowInformation(SquadOfThisPanel); });


        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((eventData) => { squadPanelScript.BindImageEvent_AddToSelectedList(SquadOfThisPanel); });
        eventTrigger_Sprite.triggers.Add(entry);

        blackPanelSkill = squadPanel.transform.Find("SquadSkillPanel/CooldownPanel").GetComponent<UnityEngine.UI.Image>();
        actionPanelBlock = squadPanel.transform.Find("BlockPanel").gameObject;

        ServiceRegistry.WorkWithService<EventBus>().Subscribe< AbstractSquad, int, int>((squad, timer, skillCooldown) =>
        {
            if(squad == SquadOfThisPanel)
            {
                blackPanelSkill.fillAmount = (float)timer/skillCooldown;
            }
        });
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<string,AbstractSquad,bool>((command,squad,state) =>
        {
            if (squad == SquadOfThisPanel) 
            { 
                if (command == "ChangeState_BlockSkill") { blackPanelSkill.gameObject.SetActive(state); return; } 
                if (command == "ChangeState_BlockAction") { actionPanelBlock.gameObject.SetActive(state); return; }
            }
        });

    }
    public void ChangeCurrentCell(GameObject cell) { currentCell = cell;}
    public void ChangeInformation(AbstractSquad squad)
    {
        TextMeshProUGUI nameSquad = squadPanel.transform.Find("Name").transform.GetComponent<TextMeshProUGUI>();

        nameSquad.text = squad.Name;
    }

    public void SwitchActiveState(bool state) { if (SquadOfThisPanel.Side != SideEnum.Allies) return; squadPanel.SetActive(state); }
    public void SwipeParent(Transform parent) { squadPanel.transform.SetParent(parent); }
}

public interface IUIConstructor
{
    public void InstantiatePanel();
}
