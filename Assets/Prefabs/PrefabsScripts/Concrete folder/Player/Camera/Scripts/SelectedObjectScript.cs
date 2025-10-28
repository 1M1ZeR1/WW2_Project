using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SelectedObjectScript : MonoBehaviour
{
    protected GameObject _selectedGameObject;
    [SerializeField]protected Camera _camera;

    protected InteractableScript interactableScript;

    [Header("Обработка кликов на UI")]
    [SerializeField] private GameObject PlayerCanvas;
    private GraphicRaycaster graphicRaycaster;

    [SerializeField] private GameObject ClickControll;
    private EventSystem eventSystem;

    private PointerEventData pointerEventData;

    public delegate void SelectingCellsForConfigure(GameObject cell);
    public event SelectingCellsForConfigure selectingCellsForConfigure;

    protected bool _inConfigureMode = false;

    private void Start()
    {
        interactableScript = GetComponent<InteractableScript>();

        ClickControll.TryGetComponent(out eventSystem);
        PlayerCanvas.TryGetComponent(out graphicRaycaster);
    }
    private void Update()
    {

    }
    public void SelectGameObject(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            pointerEventData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> resultsHitUI = new List<RaycastResult> ();
            graphicRaycaster.Raycast(pointerEventData, resultsHitUI);

            if (resultsHitUI.Count == 0)
            {

                RaycastHit hit;
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider)
                    {
                        if (hit.collider.CompareTag("Interactable Cell"))
                        {
                            _selectedGameObject = hit.collider.gameObject;
                            if(interactableScript != null && !_inConfigureMode) { interactableScript.InteractWithGameObject(_selectedGameObject); return; }
                            if (selectingCellsForConfigure != null) { selectingCellsForConfigure.Invoke(_selectedGameObject); }
                        }
                        if (hit.collider.CompareTag("Interactable"))
                        {
                            _selectedGameObject = hit.collider.gameObject;
                            interactableScript.InteractWithGameObject(_selectedGameObject);
                        }
                    }
                }
            }
        }
    }
    public GameObject GetSelectedObject() { return _selectedGameObject; }
    public void SetInConfigureMode() { _inConfigureMode = true; }
}
