using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildItemPanel : MonoBehaviour
{
    public string id { set; get; }

    [SerializeField] private GameObject buildPanel;

    [SerializeField] private Button buildButton;
    private GameObject buildButtonObject;
    [SerializeField] private Button upgradeButton;
    private GameObject upgradeButtonObject;

    [SerializeField] private Button descriptionButton;

    private GameObject currentCell;

    public void InstantiateComponent()
    {
        buildButtonObject = buildButton.gameObject;
        upgradeButtonObject = upgradeButton.gameObject;

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<GeneralUiWindowsManager, GameObject, GameObject>((sender, taker, cell) =>
        {
            currentCell = cell;

            CheckBuildForButtonState();
        });

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject, bool>((sender, cell, UI_resolution) =>
        {
            if (!UI_resolution) return;

            if (ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().Side != SideEnum.Allies) return;
            currentCell = cell;
        });

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<ButtonInteraction,BuildItemPanel>((sender,key) =>
        {
            CheckBuildForButtonState();
        });
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<CellBuildings, GameObject, AbstractBuildings>((sender, cell, build) =>
        {
            if(currentCell == cell)
            {
                CheckBuildForButtonState(cell);
            }
        });
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject, bool>((sender, cell, UI_resolution) =>
        {
            if (UI_resolution)
            {
                CheckBuildForButtonState(cell);
            }
        });


        buildButton.onClick.AddListener(() =>
        {
            buildButton.interactable = false;

            var coroutine = ServiceRegistry.WorkWithController<BuilderController>().StartBuildProccess(currentCell, id, 1);

            ServiceRegistry.WorkWithController<FocusOnCellScript>().FocusToCell(currentCell);

            if (coroutine != null)
            {
                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(currentCell).GetParameter<CellBuildings>().buildInBuilding = id;
                GameController.AddActionToQueue(() => StartCoroutine(coroutine));
            }
        });

        upgradeButton.onClick.AddListener(() =>
        {
            upgradeButton.interactable = false;

            var coroutine = ServiceRegistry.WorkWithController<BuilderController>().StartBuildProccess(currentCell, id, 2);

            ServiceRegistry.WorkWithController<FocusOnCellScript>().FocusToCell(currentCell);

            if (coroutine != null)
            {
                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(currentCell).GetParameter<CellBuildings>().buildInBuilding = id;
                GameController.AddActionToQueue(() => StartCoroutine(coroutine));
            }
        });

        if (descriptionButton != null)
        {
            descriptionButton.onClick.AddListener(() =>
            {
                ServiceRegistry.WorkWithController<DescriptionScript>().SendDescription(id);
            });
        }

        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };

        entry.callback.AddListener((eventData) =>
        {
            if (ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(currentCell).GetParameter<CellBuildings>().CheckBuildIsBuilt(id))
            {
                ServiceRegistry.WorkWithService<EventBus>().Publish<BuildItemPanel, string,GameObject>(this, id,currentCell);
            }
        });

        GetComponent<EventTrigger>().triggers.Add(entry);
    }

    public void CheckBuildForButtonState(GameObject cell = null)
    {
        buildButtonObject.SetActive(true);
        upgradeButtonObject.SetActive(true);

        buildButton.interactable = true;
        upgradeButton.interactable = true;

        CellBuildings cellBuildings;

        if (cell != null) {  cellBuildings = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuildings>(); }
        else { cellBuildings = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(currentCell).GetParameter<CellBuildings>(); }

        if (cellBuildings.CheckBuildIsBuilt(id))
        {
            buildButtonObject.SetActive(false);
            upgradeButtonObject.SetActive(true);
        }
        else { upgradeButtonObject.SetActive(false); }

        if(cellBuildings.buildInBuilding == id)
        {
            buildButton.interactable = false;
            upgradeButton.interactable = false;
        }

        if (!cellBuildings.CheckBuildIsBuilt("HeadquartersBuild"))
        {
            if(id == "HeadquartersBuild") { return; }
            buildButton.interactable = false;
            upgradeButton.interactable = false;
        }
    }
}
