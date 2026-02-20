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

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject>((sender, cell) =>
        {
            switch (buttonType) 
            {
                case ButtonInteraction.ButtonType.Build:
                    StateSwitcher(ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuildings>().CheckBuildIsBuilt("HeadquartersBuild"));
                    break;
                case ButtonInteraction.ButtonType.Headquarters:
                    StateSwitcher(ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuildings>().CheckBuildIsBuilt("HeadquartersBuild"));
                    break;
                case ButtonInteraction.ButtonType.Training:
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
