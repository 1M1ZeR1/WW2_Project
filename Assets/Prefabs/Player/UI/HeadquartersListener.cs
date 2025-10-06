using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadquartersListener : MonoBehaviour
{
    private Button _thisButton;

    [SerializeField] private HeadquartersPanel headquartersPanel;

    private GameObject currentCell;

    private void Start()
    {
        _thisButton = GetComponent<Button>();
        ServiceRegistry.WorkWithController<InteractableScript>().playerIsInteract += SwithButtonState;

        _thisButton.onClick.AddListener(() => { headquartersPanel.OpenWindowForCell(currentCell); });
    }
    private void SwithButtonState(GameObject cell)
    {
        if (ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(cell))
        {
            currentCell = cell;

            if (ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuildings>().beenBuildingBuilt[BuildsEnum.HeadQuarters])
            {
                _thisButton.interactable = true;
            }
            else { _thisButton.interactable = false; }
        }
    }
}
