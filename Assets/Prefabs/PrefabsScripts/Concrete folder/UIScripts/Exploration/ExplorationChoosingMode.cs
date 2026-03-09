using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ChoosingScript;

public class ExplorationChoosingMode : MonoBehaviour
{
    [SerializeField] private HoverHandler hoverHandler;

    public bool inChoosingMode { get;private set; } = false;

    private int searchDepth = 0;

    private GameObject cellSelected;

    private List<GameObject> cellToExplorate = new();

    [SerializeField] private GameObject buttonConfirm;
    [SerializeField] private GameObject areaTips;

    [SerializeField] private GameObject exitSkillTip;

    public void SendSquad()
    {
        if(cellToExplorate.Count == 0) { cellToExplorate.Add(cellSelected); }

        ServiceRegistry.WorkWithService<EventBus>().Publish<ExplorationChoosingMode, SkillController, List<GameObject>>(this,null,cellToExplorate.ToList());

        UnColorCells();
        cellToExplorate.Clear();

        DisableStateAreaMode(true);

        ServiceRegistry.WorkWithService<EventBus>().Publish<SkillController, bool>(null, true);

        cellSelected = null;
    }

    private void UISwitcher(bool state)
    {
        buttonConfirm.SetActive(state);
        areaTips.SetActive(state);
        exitSkillTip.SetActive(!state);
    }

    private void EnableStateAreaMode(GameObject cell)
    {
        cellSelected = cell;

        CameraMovementScript.BlockMovement();

        hoverHandler.enableMaterials = false;

        ServiceRegistry.WorkWithController<CellInteraction>().SetMaterial_Exploration(cell);
        ServiceRegistry.WorkWithService<EventBus>().Publish<InteractableScript, SelectedObjectScript>(null, null);

        UISwitcher(true);
    }

    public void DisableStateAreaMode(bool endAction = false)
    {
        if(!endAction)ServiceRegistry.WorkWithController<InteractableScript>().AddSingleSubscriber(EnableStateAreaMode);

        if (cellSelected != null)
        {
            ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic_Force(
                    ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellSelected).GetParameter<CellArea>().Side,
                    cellSelected,
                    false
                    );

            cellSelected = null;
        }

        UnColorCells();

        cellToExplorate.Clear();

        CameraMovementScript.UnBlockMovement();
        ServiceRegistry.WorkWithService<EventBus>().Publish<InteractableScript, SelectedObjectScript>(null, null);

        UISwitcher(false);

        hoverHandler.enableMaterials = true;

        searchDepth = 0;
    }

    public void EnableChoosingMode() { inChoosingMode = true; 
        ServiceRegistry.WorkWithService<EventBus>().Publish<InteractableScript, ChoosingScript>(null, null);
        ServiceRegistry.WorkWithController<InteractableScript>().AddSingleSubscriber(EnableStateAreaMode);
    }
    public void DisableChoosingMode() { inChoosingMode = false; if (cellSelected != null) 
        {
            ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic(
                    ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellSelected).GetParameter<CellArea>().Side,
                    cellSelected,
                    false
                    );
            cellSelected = null;
        }

        ServiceRegistry.WorkWithService<EventBus>().Publish<InteractableScript, ChoosingScript>(null, null);
    
    }

    public void AddSizeToArea(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!inChoosingMode || cellSelected == null) return;

            if (searchDepth >= 3) return;

            searchDepth++;

            cellToExplorate = ServiceRegistry.WorkWithController<CellController>().
            WorkWithCell<CellParametersHandler>(cellSelected).GetParameter<CellArea>().NeighboresSearcher(searchDepth).ToList();

            ColorCells();
        }
    }
    public void RemoveSizeFromArea(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!inChoosingMode || cellSelected == null) return;

            if (searchDepth <= 0) return;

            searchDepth--;

            UnColorCells();

            if (searchDepth <= 0) { ServiceRegistry.WorkWithController<CellInteraction>().SetMaterial_Exploration(cellSelected); return; }

            cellToExplorate = ServiceRegistry.WorkWithController<CellController>().
            WorkWithCell<CellParametersHandler>(cellSelected).GetParameter<CellArea>().NeighboresSearcher(searchDepth).ToList();

            ColorCells();
        }
    }

    private void UnColorCells()
    {
        foreach (var cell in cellToExplorate)
        {
            ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic(
                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().Side,
                cell,
                false
                );
        }
    }

    private void ColorCells()
    {
        foreach(var cell in cellToExplorate)
        {
            ServiceRegistry.WorkWithController<CellInteraction>().SetMaterial_Exploration(cell);
        }
    }

}
