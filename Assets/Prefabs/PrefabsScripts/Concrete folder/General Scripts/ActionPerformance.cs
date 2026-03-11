using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPerformance
{
    public List<Action<GameObject>> singleSubscribers { get; private set; } = new();

    public Action<GameObject> interceptorAction {private get; set; }

    public void ActionOnCell(GameObject cell)
    {
        if(interceptorAction != null) { interceptorAction?.Invoke(cell); interceptorAction = null;return; }

        if (!ServiceRegistry.WorkWithService<CellAccessibilityValidator>().InteractWithAlliesCell(cell)) { StartBattle(cell); }
        else { SendInformation(cell); }
    }

    private void SendInformation(GameObject finishCell)
    {
        if (!ServiceRegistry.WorkWithService<CellAccessibilityValidator>().InteractWithAlliesCell(finishCell))
        {
            ServiceRegistry.WorkWithController<MessageScript>().SendMessage(0);return;
        }
        //gameController.UpdateSquadInformation(squadWhatStartChoise, cellWhatStartChoise);

        var countSquadsOnCell = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(finishCell).GetParameter<CellSquadsOnArea>().GetCountCurrentMax();

        Debug.LogError($"{finishCell},{countSquadsOnCell.Item1},{ServiceRegistry.WorkWithService<SelectedSquadsBuffer>().GetCount()}");
        if (countSquadsOnCell.Item1 +
            ServiceRegistry.WorkWithService<SelectedSquadsBuffer>().GetCount() > countSquadsOnCell.Item2)
        {
            ServiceRegistry.WorkWithService<SelectedSquadsBuffer>().Clear();

            return;
        }

        GameController.AddActionToQueue(() =>
        {
            ServiceRegistry.WorkWithService<SelectedSquadsBuffer>().GetCopyOfList().ForEach(squad => {
                ServiceRegistry.WorkWithController<MovementController>().AddMovementForSquad(
                    ServiceRegistry.WorkWithController<InteractableScript>().currentInteractableCell, finishCell, squad);
            });

            ServiceRegistry.WorkWithService<SelectedSquadsBuffer>().Clear();
        });
    }
    private void StartBattle(GameObject cell)
    {
        if (!ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().IsCellNeighbor(
            ServiceRegistry.WorkWithController<InteractableScript>().currentInteractableCell))
        {
            ServiceRegistry.WorkWithController<MessageScript>().SendMessage(1);
            return;
        }

        GameController.AddActionToQueue(
            () =>
            {
                ServiceRegistry.WorkWithController<BattleController>().TryStartBattle(cell, 
                    ServiceRegistry.WorkWithController<InteractableScript>().currentInteractableCell,
                    ServiceRegistry.WorkWithService<SelectedSquadsBuffer>().GetCopyOfList());

                ServiceRegistry.WorkWithService<SelectedSquadsBuffer>().Clear();
            });
    }
}
