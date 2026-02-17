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

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject>((sender, cell) =>
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

            windowsOpener.WindowStateSwitch();

            windowsOpener.freeClickBlocked = false;
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

            windowsOpener.WindowStateSwitch();

            windowsOpener.freeClickBlocked = false;
        }
    }
}
