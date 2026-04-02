using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonState : MonoBehaviour
{
    [SerializeField]private ButtonInteraction.ButtonType buttonType;

    private bool enabled = true;

    private Image button;
    private EventTrigger eventTrigger;

    private void Start()
    {
        button = GetComponent<Image>();
        eventTrigger = GetComponent<EventTrigger>();

        StateSwitcher(false);

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject, bool>((sender, cell, UI_resolution) =>
        {
            if (!UI_resolution) return;
            switch (buttonType) 
            {
                case ButtonInteraction.ButtonType.Build:
                    if (ServiceRegistry.WorkWithController<CellController>().FastDrop_CellSide(cell) == SideEnum.Enemys) { StateSwitcher(false); return; }
                    StateSwitcher(ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuildings>().CheckBuildIsBuilt("HeadquartersBuild"));
                    break;
                case ButtonInteraction.ButtonType.Headquarters:
                    if (ServiceRegistry.WorkWithController<CellController>().FastDrop_CellSide(cell) == SideEnum.Enemys) { StateSwitcher(false); return; }
                    StateSwitcher(ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuildings>().CheckBuildIsBuilt("HeadquartersBuild"));
                    break;
                case ButtonInteraction.ButtonType.Training:
                    if (ServiceRegistry.WorkWithController<CellController>().FastDrop_CellSide(cell) == SideEnum.Enemys) { StateSwitcher(false); return; }
                    StateSwitcher(ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuildings>().CheckBuildIsBuilt("CampBuild"));
                    break;
            }
        });
    }

    private void ButtonState_Build()
    {

    }
    private void ButtonState_Headquarters()
    {

    }
    private void ButtonState_Training()
    {

    }
    private void StateSwitcher(bool beEnabled)
    {
        if (beEnabled != enabled) 
        {
            if (beEnabled) { button.color = new Color(button.color.r,button.color.g,button.color.b,1f); eventTrigger.enabled = true; }
            else { button.color = new Color(button.color.r, button.color.g, button.color.b, .6f); eventTrigger.enabled = false; }
            enabled = beEnabled;
        }
    }
}
