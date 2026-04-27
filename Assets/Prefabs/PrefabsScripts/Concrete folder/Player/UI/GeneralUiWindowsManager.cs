using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GeneralUiWindowsManager : MonoBehaviour
{
    private List<GameObject> panels;

    [Header("Окно лагеря")]
    [SerializeField] private GameObject campPanel;
    private TrainingPanelScript trainingPanelScript;

    [SerializeField] private GameObject[] smartInteractionPanels;

    private GameObject currentSmartInteracted;

    public void Start()
    {
        trainingPanelScript = campPanel.GetComponent<TrainingPanelScript>();
        panels = new();

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<BuildItemPanel, string, GameObject>((sender, id,cell) =>
        {
            switch (id)
            {
                case "CampBuild":trainingPanelScript.OpenWindowsWitchCell(cell); break;
            }
        });

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<GeneralUiWindowsManager, PanelStateSaver, GameObject>((key, sender, panel) =>
        {
            panels.Add(panel);
        });

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<GeneralUiWindowsManager, PanelSmartInteraction, GameObject>((key, sender, panel) =>
        {
            if (currentSmartInteracted == panel) return;

            if(smartInteractionPanels.Contains(panel))
            {
                currentSmartInteracted.SetActive(false);
                currentSmartInteracted = panel;
            }
        });
    }


    public void CloseWindow(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (panels.Count == 0) return;

            panels[panels.Count-1].SetActive(false);
            panels.RemoveAt(panels.Count - 1);
        }
    }
}
