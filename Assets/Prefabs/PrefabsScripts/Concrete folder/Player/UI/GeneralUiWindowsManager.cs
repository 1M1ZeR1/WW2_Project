using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GeneralUiWindowsManager : MonoBehaviour
{
    [Header("Окно тренировки")]
    [SerializeField] private GameObject trainingPanel;

    [Header("Окно постройки")]
    [SerializeField] private GameObject buildPanel;

    [Header("Окно клетки")]
    [SerializeField] private GameObject cellPanel;

    [Header("Окно лагеря")]
    [SerializeField] private GameObject campPanel;
    private TrainingPanelScript trainingPanelScript;

    [Header("Окно информации о отряде")]
    [SerializeField] private GameObject squadInfoPanel;

    public void Start()
    {
        trainingPanelScript = campPanel.GetComponent<TrainingPanelScript>();

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<BuildItemPanel, string, GameObject>((sender, id,cell) =>
        {
            switch (id)
            {
                case "CampBuild":trainingPanelScript.OpenWindowsWitchCell(cell); break;
            }
        });
    }
    public void CloseWindow(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (squadInfoPanel.activeSelf) { squadInfoPanel.SetActive(false); }
            if (trainingPanel.activeSelf) { trainingPanel.SetActive(false); return; }
            if (campPanel.activeSelf) { campPanel.SetActive(false); PauseScript.SetGameState(GameState.Play); return; }
            if (buildPanel.activeSelf) {  buildPanel.SetActive(false); return; }

            if (cellPanel.activeSelf) { cellPanel.SetActive(false);CameraMovementScript.UnBlockMovement(); return; }
        }
    }
}
