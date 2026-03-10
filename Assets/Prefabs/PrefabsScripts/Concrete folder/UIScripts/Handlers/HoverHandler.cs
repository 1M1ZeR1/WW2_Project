using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverHandler : MonoBehaviour
{
    private GameObject currentHoveredCell;

    [SerializeField] private TextMeshProUGUI textTip;

    public bool enableMaterials { private get; set; } = true;
    public bool SeeTextTips { private get; set; } = false;
    
    private void Update()
    {
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
                if (SeeTextTips)
                {
                    SetTextTip(hit.collider.gameObject);
                }
                if (enableMaterials)
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

    private void SetTextTip(GameObject hoveredCell)
    {
        if (hoveredCell == null && textTip.text != "") { textTip.text = ""; return; }

        switch (ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(hoveredCell).GetParameter<CellArea>().Side)
        {
            case SideEnum.Allies: textTip.text = "Под вашим контролем";break;
            case SideEnum.Enemys: textTip.text = "Под контролем противника"; break;
            case SideEnum.None: textTip.text = ""; break;
        }
    }
}
