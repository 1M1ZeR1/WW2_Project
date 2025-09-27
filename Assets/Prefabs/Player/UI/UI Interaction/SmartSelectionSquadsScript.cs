using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartSelectionSquadsScript : MonoBehaviour
{
    private InteractableScript interactableScript;

    protected GameObject _currentInteractableCell;
    private List<GameObject> selectedSquadsToAction = new List<GameObject>();

    private void Start()
    {
        interactableScript = GetComponent<InteractableScript>();

        interactableScript.playerIsInteract += CellInteractionListener;
    }

    private void CellInteractionListener(GameObject cell) { _currentInteractableCell = cell; selectedSquadsToAction.Clear(); }

    public void SwitchInSquadList(GameObject squadPanel)
    {
        if (selectedSquadsToAction.Contains(squadPanel)) { selectedSquadsToAction.Remove(squadPanel); }
        else { selectedSquadsToAction.Add(squadPanel); }
    }

    public bool CheckIsListNotEmpty() { return selectedSquadsToAction.Count ==  0; }
}
