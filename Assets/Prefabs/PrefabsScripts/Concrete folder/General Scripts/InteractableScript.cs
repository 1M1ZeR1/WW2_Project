using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InteractableScript : MonoBehaviour
{
    private InteractionCellState interactionCellState = InteractionCellState.None;

    private ChoosingScript choosingScript;
    private SmartSelectionSquadsScript smartSelectionSquadsScript;

    public GameObject currentInteractableCell { get; private set; }

    public List<Action<GameObject>> singleSubscribers { get; private set; } = new();

    private bool subscribedOnHoverEvent = false;

    private void Start()
    {
        choosingScript = GetComponent<ChoosingScript>();
        smartSelectionSquadsScript = GetComponent<SmartSelectionSquadsScript>();

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, ChoosingScript>((key1, key2) => {
            if (interactionCellState == InteractionCellState.Choosing) interactionCellState = InteractionCellState.None;
            else { interactionCellState = InteractionCellState.Choosing; }
        });
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, SelectedObjectScript>((key1, key2) => {

            if (interactionCellState == InteractionCellState.Blocked) interactionCellState = InteractionCellState.None;
            else { interactionCellState = InteractionCellState.Blocked; }
        });
    }
    public void InteractWithGameObject(GameObject interacableGameObject)
    {
        if (interactionCellState == InteractionCellState.Blocked) return;

        if (interactionCellState == InteractionCellState.Choosing) {
            ServiceRegistry.WorkWithService<EventBus>().Publish<InteractableScript, GameObject, bool>(this, interacableGameObject, false);

            currentInteractableCell = interacableGameObject;

            if (subscribedOnHoverEvent) HoverSubscriber(false);
        }

        if (interactionCellState == InteractionCellState.Choise)
        {
            ServiceRegistry.WorkWithService<ActionPerformance>().ActionOnCell(interacableGameObject);

            choosingScript.ExitChoiseState();
        }

        if (interactionCellState == InteractionCellState.None) 
        {
            ServiceRegistry.WorkWithService<EventBus>().Publish<InteractableScript, GameObject, bool>(this, interacableGameObject, true);

            currentInteractableCell = interacableGameObject;

            ServiceRegistry.WorkWithService<SelectedSquadsBuffer>().ResetSelectedSquads();
        }

        interactionCellState = InteractionCellState.None;

        if (singleSubscribers.Count != 0) { singleSubscribers.ForEach(action => { action?.Invoke(interacableGameObject); });singleSubscribers.Clear(); }
    }

    public void WantToChooseCell()
    {
        interactionCellState = InteractionCellState.Choise;

        choosingScript.CreateChoiseState(ChoosingScript.ChoosingMode.Action);
    }

    public void RevokeSquad()
    {
        HoverSubscriber(true);

        interactionCellState = InteractionCellState.Choise;

        choosingScript.CreateChoiseState(ChoosingScript.ChoosingMode.Revoke);
    }
    public void ChoosingCanceled() { interactionCellState = InteractionCellState.None; }


    private void HoverSubscriber(bool needToSub)
    {
        subscribedOnHoverEvent = needToSub;

        if(needToSub) ServiceRegistry.WorkWithController<HoverHandler>().SendCurrentHoveredCell += BlockInteractionByHover;
        else ServiceRegistry.WorkWithController<HoverHandler>().SendCurrentHoveredCell -= BlockInteractionByHover;
    }
    private void BlockInteractionByHover(GameObject cell)
    {
        if (ServiceRegistry.WorkWithService<CellAccessibilityValidator>().InteractWithNoEnemysCell(cell)) { interactionCellState = InteractionCellState.Blocked; }

        else { interactionCellState = InteractionCellState.Choosing; }
    }
}

public enum InteractionCellState
{
    None,
    Blocked,
    UI,
    Choosing,
    Choise,
    Exploration
}