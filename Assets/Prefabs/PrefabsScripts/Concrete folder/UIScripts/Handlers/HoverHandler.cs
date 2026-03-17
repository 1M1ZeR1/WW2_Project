using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverHandler : MonoBehaviour
{
    private GameObject currentHoveredCell;

    [SerializeField] private TextMeshProUGUI textTip;

    public Action<GameObject> SendCurrentHoveredCell;

    public bool enableMaterials { private get; set; } = true;

    private bool seeTextTips_AreasInfo = false;
    public bool SeeTextTips_AreasInfo { 
        get { return seeTextTips_AreasInfo; } 
        
        set { 
            seeTextTips_AreasInfo = value; 
            if (!seeTextTips_AreasInfo)
            {
                textTip.text = "";
            }
        }
    }

    private GameObject actionStartedCell;

    public bool SeeHoveredCell { private get; set; } = false;
    
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
                if (SeeTextTips_AreasInfo)
                {
                    SetTextTip_CellsInfo(hit.collider.gameObject);
                }
                if (actionStartedCell)
                {
                    SetTextTip_Action(hit.collider.gameObject);
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
                if (SeeHoveredCell)
                {
                    SendCurrentHoveredCell?.Invoke(hit.collider.gameObject);
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

    private void SetTextTip_CellsInfo(GameObject hoveredCell)
    {
        if (hoveredCell == null && textTip.text != "") { textTip.text = ""; return; }

        switch (ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(hoveredCell).GetParameter<CellArea>().Side)
        {
            case SideEnum.Allies: textTip.text = "Под вашим контролем";break;
            case SideEnum.Enemys: textTip.text = "Под контролем противника"; break;
            case SideEnum.None: textTip.text = ""; break;
        }
    }
    private void SetTextTip_Action(GameObject hoveredCell)
    {
        if (hoveredCell == null && textTip.text != "") { textTip.text = ""; return; }

        switch (ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(hoveredCell).GetParameter<CellArea>().Side)
        {
            case SideEnum.Enemys: 
                {
                    if (ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(hoveredCell).GetParameter<CellArea>().IsCellNeighbor(actionStartedCell))
                    {
                        textTip.text = "Вы можете начать боевые действия.";
                    }
                    else { textTip.text = "Клетка слишком далеко, чтобы начать боевые действия."; }
                    break; 
                }
            default:
                {
                    var count = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(hoveredCell).GetParameter<CellSquadsOnArea>().GetCountCurrentMax();
                    textTip.text = $"{count.Item1}/{count.Item2}";

                    break;
                }
        }
    }

    public void ActionTipMode(GameObject actionStartedCell = null)
    {
        this.actionStartedCell = actionStartedCell;
        if (actionStartedCell == null) { textTip.text = ""; }
    }
}
