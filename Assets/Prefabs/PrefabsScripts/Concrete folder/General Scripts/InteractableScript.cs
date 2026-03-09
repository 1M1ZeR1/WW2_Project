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

    private List<Action<GameObject>> singleSubscribers = new();

    public GameObject currentInteractableCell { get; private set; }
    protected List<AbstractSquad> squadsWhatStartChoise = new List<AbstractSquad>();


    /// <summary>
    /// 
    /// </summary>


    [Header("Всплывающее окно")]
    [SerializeField] private GameObject messagePanel;
    private MessageScript messageScript;

    private void Start()
    {
        messagePanel.TryGetComponent(out messageScript);

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
        }

        if (interactionCellState == InteractionCellState.Choise)
        {
            if (ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(interacableGameObject)) 
            { 
                SendInformation(interacableGameObject); 
            }
            else { StartBattle(interacableGameObject); }

            choosingScript.ExitChoiseState();
        }

        if (interactionCellState == InteractionCellState.None) 
        {
            ServiceRegistry.WorkWithService<EventBus>().Publish<InteractableScript, GameObject, bool>(this, interacableGameObject, true);

            currentInteractableCell = interacableGameObject;

            ResetSelectedSquads();
        }

        interactionCellState = InteractionCellState.None;

        if(singleSubscribers.Count > 0) { singleSubscribers.ForEach(action => action.Invoke(interacableGameObject)); singleSubscribers.Clear(); }
    }

    private void ResetSelectedSquads() { squadsWhatStartChoise.Clear(); }
    public void AddToSelectedSquads(AbstractSquad squad) { if (squadsWhatStartChoise.Contains(squad)) squadsWhatStartChoise.Remove(squad);
        else squadsWhatStartChoise.Add(squad);
    }

    public void WantToChooseCell()
    {
        interactionCellState = InteractionCellState.Choise;

        choosingScript.CreateChoiseState(ChoosingScript.ChoosingMode.Action);
    }

    public void RevokeSquad(AbstractSquad squadToRevoke,Action<GameObject> subToNextCell = null)
    {
        interactionCellState = InteractionCellState.Choosing;

        choosingScript.CreateChoiseState(ChoosingScript.ChoosingMode.Revoke);

        if (subToNextCell != null) { singleSubscribers.Add(subToNextCell); }
    }
    private void SendInformation(GameObject finishCell)
    {
        if (!ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(finishCell))
        {
            messagePanel.SetActive(true);

            messageScript.SendMessage(0);
            return;
        }
        //gameController.UpdateSquadInformation(squadWhatStartChoise, cellWhatStartChoise);

        var countSquadsOnCell = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(finishCell).GetParameter<CellSquadsOnArea>().GetCountCurrentMax();

        if (countSquadsOnCell.Item1 + squadsWhatStartChoise.Count > countSquadsOnCell.Item2) {
            squadsWhatStartChoise.Clear();

            return;
        }

        GameController.AddActionToQueue(() => 
        {
            List<AbstractSquad> squadsToDelete = squadsWhatStartChoise.ToList();

            foreach(var squad in squadsWhatStartChoise)
            {
                ServiceRegistry.WorkWithController<MovementController>().AddMovementForSquad(currentInteractableCell, finishCell, squad);
            }

            foreach(var squad in squadsToDelete) { squadsWhatStartChoise.Remove(squad); }
        }); 
    }
    private void StartBattle(GameObject cell)
    {
        if(!ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().IsCellNeighbor(currentInteractableCell)) 
        {
            messagePanel.SetActive(true);

            messageScript.SendMessage(1);
            return; 
        }

        GameController.AddActionToQueue(
            () => 
            {
                List<AbstractSquad> squadsToDelete = squadsWhatStartChoise.ToList();

                ServiceRegistry.WorkWithController<BattleController>().TryStartBattle(cell, currentInteractableCell, squadsWhatStartChoise);
                foreach (var squad in squadsToDelete) { squadsWhatStartChoise.Remove(squad); }
            });
    }
    public void SetStartingChoisingParameters(GameObject cell, AbstractSquad squad) { currentInteractableCell = cell; if (!squadsWhatStartChoise.Contains(squad)) squadsWhatStartChoise.Add(squad); }
    public void ChoosingCanceled() { interactionCellState = InteractionCellState.None; }

    public void AddSingleSubscriber(Action<GameObject> action)
    {
        singleSubscribers.Add(action);
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