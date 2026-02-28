using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInteraction : MonoBehaviour
{
    [SerializeField] private MessageScript messageScript;

    private WindowsOpener windowsOpener;

    public enum ButtonType
    {
        None,
        Build,
        Training,
        Headquarters,
        Quests,
        Battles
    }

    [SerializeField] private ButtonType buttonType;

    private GameObject currentInteractedCell;

    void Start()
    {
        windowsOpener = GetComponent<WindowsOpener>();

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject, bool>((sender, cell, UI_resolution) =>
        {
            currentInteractedCell = cell;

            windowsOpener.freeClickBlocked = true;
        });
    }

    public void ButtonInteraction_Build()
    {
        if (ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(currentInteractedCell))
        {
            ServiceRegistry.WorkWithService<EventBus>().Publish<ButtonInteraction,BuildItemPanel>(this,null);

            ServiceRegistry.WorkWithService<EventBus>().Publish<OpenBuildMenuScript>(null);

            windowsOpener.freeClickBlocked = false;

            windowsOpener.WindowStateSwitch();
        }
        else
        {
            messageScript.SendMessage(4);
        }
    }
    public void ButtonInteraction_Headquarters()
    {
        if (ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(currentInteractedCell))
        {
            ServiceRegistry.WorkWithService<EventBus>().Publish<ButtonInteraction,HeadquartersPanel>(this,null);

            windowsOpener.freeClickBlocked = false;

            windowsOpener.WindowStateSwitch();
        }
    }

    public void ButtonInteraction_Training()
    {
        if (ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(currentInteractedCell))
        {
            ServiceRegistry.WorkWithService<EventBus>().Publish<ButtonInteraction, TrainingPanelScript, GameObject>(this, null, currentInteractedCell);

            windowsOpener.freeClickBlocked = false;

            windowsOpener.WindowStateSwitch();
        }
    }
}
