using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GeneralUiWindowsManager : MonoBehaviour
{
    private List<GameObject> panels;

    [Header("Окно лагеря")]
    [SerializeField] private GameObject campPanel;
    private TrainingPanelScript trainingPanelScript;

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
