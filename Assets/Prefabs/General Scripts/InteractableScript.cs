using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InteractableScript : MonoBehaviour
{
    protected enum ChoiseHelper
    {
        None,
        Attack,
        Movement
    }
    protected ChoiseHelper _choiseHelper = ChoiseHelper.None;

    [Header("Контроллер битв")]
    [SerializeField] private GameObject battleControllerObject;
    private BattleController battleController;

    [Header("Контроллер передвижения")]
    [SerializeField] private GameObject squadMovementCotroller;
    private SquadMovementMonoBehaviour squadMovementScript;

    [Header("Главный контроллер")]
    [SerializeField] private GameObject gameControllerObject;
    private GameController gameController;

    private ChoosingScript choosingScript;
    private SmartSelectionSquadsScript smartSelectionSquadsScript;


    protected GameObject cellWhatStartChoise;
    protected List<AbstractSquad> squadsWhatStartChoise = new List<AbstractSquad>();


    public delegate void IneractWith(GameObject interacableGameObject);
    public event IneractWith playerIsInteract;

    private bool NextClickIsChoice = false;


    [Header("Кнопка спавна отряда")]
    [SerializeField] private GameObject debugButton;//Debug Field
    private DebugButton debugButtonScript;


    /// <summary>
    /// 
    /// </summary>


    [Header("Всплывающее окно")]
    [SerializeField] private GameObject messagePanel;
    private MessageScript messageScript;

    private void Start()
    {
        debugButton.TryGetComponent(out debugButtonScript);
        messagePanel.TryGetComponent(out messageScript);

        battleControllerObject.TryGetComponent(out battleController);

        squadMovementCotroller.TryGetComponent(out squadMovementScript);

        gameControllerObject.TryGetComponent(out gameController);

        choosingScript = GetComponent<ChoosingScript>();
        smartSelectionSquadsScript = GetComponent<SmartSelectionSquadsScript>();
    }
    public void InteractWithGameObject(GameObject interacableGameObject)
    {
        if (NextClickIsChoice)
        {
            if (ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(interacableGameObject)) 
            { 
                SendInformation(interacableGameObject); 
            }
            else { StartBattle(interacableGameObject); }

            NextClickIsChoice = false;

            choosingScript.ExitChoiseState();

            return;
        }
        debugButtonScript.SetSelectedCell(interacableGameObject);
        playerIsInteract.Invoke(interacableGameObject);

        cellWhatStartChoise = interacableGameObject;

        ResetSelectedSquads();
    }
    private void ResetSelectedSquads() { squadsWhatStartChoise.Clear(); }
    public void AddToSelectedSquads(AbstractSquad squad) { if (squadsWhatStartChoise.Contains(squad)) squadsWhatStartChoise.Remove(squad);
        else squadsWhatStartChoise.Add(squad);
    }

    public void WantToChooseCell()
    {
        NextClickIsChoice = true;

        choosingScript.CreateChoiseState();
    }
    private void SendInformation(GameObject finishCell)
    {
        if (!ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(finishCell))
        {
            messagePanel.SetActive(true);

            messageScript.SendMessage(0);
            return;
        }
        Debug.Log(squadsWhatStartChoise, cellWhatStartChoise);
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
                ServiceRegistry.WorkWithController<MovementController>().AddMovementForSquad(cellWhatStartChoise, finishCell, squad);
            }

            foreach(var squad in squadsToDelete) { squadsWhatStartChoise.Remove(squad); }
        }); 

        _choiseHelper = ChoiseHelper.None;
    }
    private void StartBattle(GameObject cell)
    {
        if(!ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().IsCellNeighbor(cellWhatStartChoise)) 
        {
            messagePanel.SetActive(true);

            messageScript.SendMessage(1);
            return; 
        }

        GameController.AddActionToQueue(
            () => 
            {
                List<AbstractSquad> squadsToDelete = squadsWhatStartChoise.ToList();

                battleController.TryStartBattle(cell, cellWhatStartChoise, squadsWhatStartChoise);
                foreach (var squad in squadsToDelete) { squadsWhatStartChoise.Remove(squad); }
            });

        
        _choiseHelper = ChoiseHelper.None;
    }
    public void SetStartingChoisingParameters(GameObject cell, AbstractSquad squad) { cellWhatStartChoise = cell; if (!squadsWhatStartChoise.Contains(squad)) squadsWhatStartChoise.Add(squad); }
    public void ChoosingCanceled() { NextClickIsChoice = false; }
}
