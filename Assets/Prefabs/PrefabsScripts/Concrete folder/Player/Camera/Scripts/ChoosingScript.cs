using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ChoosingScript;

public class ChoosingScript : MonoBehaviour
{
    public enum ChoosingMode
    {
        None,
        Action,
        Exploration,
        Revoke
    }

    [SerializeField] private GameObject[] panelsToControll;

    private bool[] elementToRecover;

    [Header("Панель выделения")]
    [SerializeField] private GameObject choosingWindow_Action;
    [SerializeField] private GameObject choosingWindow_Exploration;

    [SerializeField] private ExplorationChoosingMode explorationChoosingMode;


    protected InteractableScript interactableScript;
    protected Vector3 _cameraStartPosition;

    protected GameObject currentWorkingWindow;

    private ChoosingMode currentChoosingMode;

    private void Start()
    {
        elementToRecover = Enumerable.Range(0, panelsToControll.Length).Select(el => false).ToArray();
        interactableScript = GetComponent<InteractableScript>();
    }
    public void CreateChoiseState(ChoosingMode choosingMode)
    {
        for(int i =0; i < panelsToControll.Length; i++)
        {
            if (panelsToControll[i].activeSelf)
            {
                elementToRecover[i] = true;
                panelsToControll[i].SetActive(false);
            }
        }

        CameraMovementScript.UnBlockMovement();

        switch (choosingMode)
        {
            case ChoosingMode.Action: choosingWindow_Action.SetActive(true); currentWorkingWindow = choosingWindow_Action;
                currentChoosingMode = ChoosingMode.Action;
                break;
            case ChoosingMode.Exploration: choosingWindow_Exploration.SetActive(true); currentWorkingWindow = choosingWindow_Exploration;
                currentChoosingMode = ChoosingMode.Exploration;
                explorationChoosingMode.EnableChoosingMode();
                break;
            case ChoosingMode.Revoke: choosingWindow_Action.SetActive(true); currentWorkingWindow = choosingWindow_Action;
                currentChoosingMode = ChoosingMode.Revoke;
                ServiceRegistry.WorkWithController<HoverHandler>().SeeTextTips = true;
                break;
        }

        _cameraStartPosition = transform.position;
    }

    public void ExitChoiseState()
    {
        for (int i = 0; i < panelsToControll.Length; i++)
        {
            if (elementToRecover[i])
            {
                elementToRecover[i] = false;
                panelsToControll[i].SetActive(true);
            }
        }

        CameraMovementScript.BlockMovement();

        if (currentWorkingWindow != null) 
        { 
            currentWorkingWindow.SetActive(false); 
            currentWorkingWindow = null; }

        if(currentChoosingMode == ChoosingMode.Exploration)
        {
            ServiceRegistry.WorkWithService<EventBus>().Publish<ChoosingScript, SkillController>(this, null);
        }

        switch (currentChoosingMode)
        {
            case ChoosingMode.Exploration:
                explorationChoosingMode.DisableChoosingMode();
                break;
            case ChoosingMode.Revoke:
                ServiceRegistry.WorkWithController<HoverHandler>().SeeTextTips = false;
                break;
        }

        currentChoosingMode = ChoosingMode.None;

        transform.position = _cameraStartPosition;
    }
    public void ExitChoiseState_RevokeSquad()
    {
        for (int i = 0; i < panelsToControll.Length; i++)
        {
            if (elementToRecover[i])
            {
                elementToRecover[i] = false;
                panelsToControll[i].SetActive(true);
            }
        }

        CameraMovementScript.BlockMovement();

        if (currentWorkingWindow != null)
        {
            currentWorkingWindow.SetActive(false);
            currentWorkingWindow = null;
        }


        switch (currentChoosingMode)
        {
            case ChoosingMode.Exploration:
                explorationChoosingMode.DisableChoosingMode();
                break;
        }

        currentChoosingMode = ChoosingMode.None;

        transform.position = _cameraStartPosition;
    }

    public void GetOutEarly(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentChoosingMode == ChoosingMode.None) return;

            switch (currentChoosingMode)
            {
                case ChoosingMode.Exploration:
                    if (explorationChoosingMode.inChoosingMode)
                    {
                        explorationChoosingMode.DisableStateAreaMode();return;
                    }
                    break;
            }

            interactableScript.ChoosingCanceled();


            ExitChoiseState();
        }
    }
}
