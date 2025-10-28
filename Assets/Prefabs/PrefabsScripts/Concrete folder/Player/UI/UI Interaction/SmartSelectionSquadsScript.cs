using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartSelectionSquadsScript : MonoBehaviour
{
    protected GameObject _currentInteractableCell;
    private List<GameObject> selectedSquadsToAction = new List<GameObject>();

    private void Start()
    {
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject>((sender, cell) => { CellInteractionListener(cell); });
    }

    private void CellInteractionListener(GameObject cell) { _currentInteractableCell = cell; selectedSquadsToAction.Clear(); }

    public void SwitchInSquadList(GameObject squadPanel)
    {
        if (selectedSquadsToAction.Contains(squadPanel)) { selectedSquadsToAction.Remove(squadPanel); }
        else { selectedSquadsToAction.Add(squadPanel); }
    }

    public bool CheckIsListNotEmpty() { return selectedSquadsToAction.Count ==  0; }
}
