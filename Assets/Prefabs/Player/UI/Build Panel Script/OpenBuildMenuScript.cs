using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBuildMenuScript : MonoBehaviour
{
    [Header("Окно построек")]
    [SerializeField] private GameObject buildMenuPanel;
    private BuildMenuUpgradeScript buildUpgradeScript;

    [Header("Контроллер взаимодействия")]
    [SerializeField] private GameObject interactControllerObject;
    private InteractableScript interactableScript;

    [Header("Окно уведомлений")]
    [SerializeField] private GameObject messagePanel;
    private MessageScript messageScript;

    private GameObject currentCell;

    private void Start()
    {
        interactControllerObject.TryGetComponent(out interactableScript);
        messagePanel.TryGetComponent(out messageScript);
        buildMenuPanel.TryGetComponent(out buildUpgradeScript);

        ServiceRegistry.WorkWithController<BuilderController>().RequestUIUpdate += UpdateInfo;

        interactableScript.playerIsInteract += ChangeCurrentCell;
    }
    private void ChangeCurrentCell(GameObject cell)
    {
        buildMenuPanel.SetActive(false);
        currentCell = cell;
    }
    public void OpenMenu()
    {
        if (ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(currentCell))
        {
            buildMenuPanel.SetActive(true);
            buildUpgradeScript.SetCurrentCell(currentCell);
            buildUpgradeScript.ShowAllBuildings();
        }
        else 
        {
            messagePanel.SetActive(true);
            messageScript.SendMessage(4);
        }
    }
    private void UpdateInfo(GameObject cell)
    {
        if(cell == currentCell && buildMenuPanel.activeSelf)
        {
            buildUpgradeScript.SetCurrentCell(currentCell);
            buildUpgradeScript.ShowAllBuildings();
        }
    }
}
