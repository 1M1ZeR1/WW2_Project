using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GeneralUiWindowsManager : MonoBehaviour
{
    private Stack<GameObject> panels;
    private Stack<GameObject> cellsInteracted;

    [Header("Окно лагеря")]
    [SerializeField] private GameObject campPanel;
    private TrainingPanelScript trainingPanelScript;

    [SerializeField] private GameObject[] smartInteractionPanels;

    private Dictionary<GameObject,PanelStateSaver> panelStateSavers = new();

    private GameObject currentSmartInteracted;
    private GameObject currentCellInteracted;

    public void Start()
    {
        foreach (var item in GameObject.FindObjectsByType<PanelStateSaver>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            panelStateSavers[item.gameObject] = item;
        }

        trainingPanelScript = campPanel.GetComponent<TrainingPanelScript>();
        panels = new();
        cellsInteracted = new();

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<BuildItemPanel, string, GameObject>((sender, id,cell) =>
        {
            switch (id)
            {
                case "CampBuild":trainingPanelScript.OpenWindowsWitchCell(cell); break;
            }
        });

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<WindowsOpener, GameObject, bool>((sender, panel, state) =>
        {
            if (state) 
            { 
                panelStateSavers[panel].OpenWindowByGeneralUI();

                if (currentSmartInteracted != null) { panelStateSavers[currentSmartInteracted].NeedSaveNotifier(); panelStateSavers[currentSmartInteracted].CloseWindowByGeneralUI(); }

                currentSmartInteracted = panel;
            }

            else
            {
                panelStateSavers[panel].CloseWindowByGeneralUI();

                currentSmartInteracted = null;

                panels.Clear();
            }


        });

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<GeneralUiWindowsManager, PanelStateSaver, GameObject, bool>((key, sender, panel, stateSaver) =>
        {
            if(!stateSaver || panels.Contains(panel))return;

            panels.Push(panel);

            cellsInteracted.Push(currentCellInteracted);
        });

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject, bool>((sender, cell, state) =>
        {
            if (state)
            {
                currentCellInteracted = cell;
            }
        });
    }


    public void PopFromStack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (panels.Count == 0)
            {
                if(currentSmartInteracted != null) { panelStateSavers[currentSmartInteracted].CloseWindowByGeneralUI(); currentSmartInteracted = null; }
                return;
            }

            var takenPanel = panels.Pop();

            panelStateSavers[takenPanel].OpenWindowByGeneralUI();

            ServiceRegistry.WorkWithService<EventBus>().Publish<GeneralUiWindowsManager, GameObject, GameObject>(this, takenPanel.gameObject, cellsInteracted.Pop());

            if (currentSmartInteracted != null) panelStateSavers[currentSmartInteracted].CloseWindowByGeneralUI();

            currentSmartInteracted = takenPanel;
        }
    }
}
