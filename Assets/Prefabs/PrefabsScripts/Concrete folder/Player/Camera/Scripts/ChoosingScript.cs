using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChoosingScript : MonoBehaviour
{
    [SerializeField] private GameObject[] panelsToControll;

    private bool[] elementToRecover;

    [Header("Панель выделения")]
    [SerializeField] private GameObject choosingWindow;


    protected InteractableScript interactableScript;
    protected Vector3 _cameraStartPosition;

    private void Start()
    {
        elementToRecover = Enumerable.Range(0, panelsToControll.Length).Select(el => false).ToArray();
        interactableScript = GetComponent<InteractableScript>();
    }

    public void CreateChoiseState()
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

        choosingWindow.SetActive(true);

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

        choosingWindow.SetActive(false);

        transform.position = _cameraStartPosition;
    }
    public void GetOutEarly(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            interactableScript.ChoosingCanceled();

            ExitChoiseState();
        }
    }

    private static bool inChoosing = false;
    public static void ChangeChooseState() { if (inChoosing) inChoosing = false;
        else inChoosing = true;
    }
    public static bool CheckInChoosing() { return  inChoosing; }
}
