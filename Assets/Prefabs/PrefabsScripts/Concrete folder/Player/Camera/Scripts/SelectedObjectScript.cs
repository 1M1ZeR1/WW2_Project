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
    [SerializeField] private GraphicRaycaster graphicRaycaster_PlayerCanvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster_ExplorationCanvas;


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
            graphicRaycaster_PlayerCanvas.Raycast(pointerEventData, resultsHitUI);
            graphicRaycaster_ExplorationCanvas.Raycast(pointerEventData, resultsHitUI);

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
                            if(interactableScript != null && !_inConfigureMode) {
                                if (DevelopMode.DevelopModeSwitcher) ServiceRegistry.WorkWithService<EventBus>().Publish<DevelopMode, GameObject>(null,_selectedGameObject);

                                interactableScript.InteractWithGameObject(_selectedGameObject); return; }
                            if (selectingCellsForConfigure != null) { selectingCellsForConfigure.Invoke(_selectedGameObject); }
                        }
                        if (hit.collider.CompareTag("Interactable"))
                        {
                            _selectedGameObject = hit.collider.gameObject;
                            if (DevelopMode.DevelopModeSwitcher) ServiceRegistry.WorkWithService<EventBus>().Publish<DevelopMode, GameObject>(null, _selectedGameObject);
                            interactableScript.InteractWithGameObject(_selectedGameObject);
                            return;
                        }
                        if( hit.collider.CompareTag("Interactable UI")) { return; }
                    }
                }
            }
        }
    }

    public void SpawnParticles(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
           
        }
    }
    public GameObject GetSelectedObject() { return _selectedGameObject; }
    public void SetInConfigureMode() { _inConfigureMode = true; }
}
