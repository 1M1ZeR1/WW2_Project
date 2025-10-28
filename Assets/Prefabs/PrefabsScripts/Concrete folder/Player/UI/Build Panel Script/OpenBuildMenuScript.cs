using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBuildMenuScript : MonoBehaviour
{
    [Header("Окно построек")]
    [SerializeField] private GameObject buildMenuPanel;


    [Header("Окно уведомлений")]
    [SerializeField] private GameObject messagePanel;
    private MessageScript messageScript;

    private GameObject currentCell;

    private void Start()
    {
        messagePanel.TryGetComponent(out messageScript);

        //ServiceRegistry.WorkWithController<BuilderController>().RequestUIUpdate += UpdateInfo;

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject>((sender, cell) =>
        {
            buildMenuPanel.SetActive(false);
            currentCell = cell;
        });

        buildMenuPanel.SetActive(false);
    }
    public void OpenMenu()
    {
        if (ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(currentCell))
        {
            ServiceRegistry.WorkWithService<EventBus>().Publish<OpenBuildMenuScript>(this);

            buildMenuPanel.SetActive(true);
        }
        else 
        {
            messagePanel.SetActive(true);
            messageScript.SendMessage(4);
        }
    }
}
