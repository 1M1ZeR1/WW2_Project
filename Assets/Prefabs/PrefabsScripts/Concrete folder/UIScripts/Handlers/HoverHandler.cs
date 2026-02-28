using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverHandler : MonoBehaviour
{
    private GameObject currentHoveredCell;

    public bool enableMaterials { private get; set; } = true;
    
    private void Update()
    {
        if (!enableMaterials) return;

        if (IsPointerOverUI_ByTag("Interactable UI")) 
        {
            if (currentHoveredCell != null) 
            {
                ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic
                    (
                    ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(currentHoveredCell).GetParameter<CellArea>().Side,
                    currentHoveredCell,
                    false
                    );

                currentHoveredCell = null;
                return;
            }
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {
            if (hit.collider == null) return;
            if(hit.collider.CompareTag("Interactable Cell"))
            {
                if (currentHoveredCell != null)
                {
                    ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic
                        (
                        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(currentHoveredCell).GetParameter<CellArea>().Side,
                        currentHoveredCell,
                        false
                        );
                }
                currentHoveredCell = hit.collider.gameObject;

                ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic
                    (
                    ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(currentHoveredCell).GetParameter<CellArea>().Side,
                    currentHoveredCell,
                    true
                    );
            }
        }
    }

    private bool IsPointerOverUI_ByTag(string tag)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new ();
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (var ray in results)
        {
            if(ray.gameObject.CompareTag(tag))return true;
        }
        return false;
    }
}
