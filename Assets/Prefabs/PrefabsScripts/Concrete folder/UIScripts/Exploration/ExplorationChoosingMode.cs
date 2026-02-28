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

    public bool inChoosingMode { private get; set; } = false;

    private int searchDepth = 0;

    public bool stateArea {  get;private set; } = false;
    private GameObject cellSelected;

    private List<GameObject> cellToExplorate = new();

    private void Start()
    {
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject, bool>((sender, cell, UI_resolution) =>
        {
            if (inChoosingMode)
            {
                stateArea = true;

                cellSelected = cell;

                CameraMovementScript.BlockMovement();

                hoverHandler.enableMaterials = false;

                ServiceRegistry.WorkWithController<CellInteraction>().SetMaterial_Exploration(cell);
            }
        });
    }

    public void DisableStateAreaMode()
    {
        stateArea = false;

        if (cellSelected != null)
        {
            ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic_Force(
                    ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellSelected).GetParameter<CellArea>().Side,
                    cellSelected,
                    false
                    );
        }

        cellSelected = null;

        UnColorCells();

        cellToExplorate.Clear();

        CameraMovementScript.UnBlockMovement();
    }

    public void EnableChoosingMode() { inChoosingMode = true; ServiceRegistry.WorkWithService<EventBus>().Publish<InteractableScript, ChoosingScript>(null, null); }
    public void DisableChoosingMode() { inChoosingMode = false; if (cellSelected != null) 
        {
            ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic(
                    ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cellSelected).GetParameter<CellArea>().Side,
                    cellSelected,
                    false
                    );
        }

        ServiceRegistry.WorkWithService<EventBus>().Publish<InteractableScript, ChoosingScript>(null, null);
    
    }

    public void AddSizeToArea(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!inChoosingMode || !stateArea) return;

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
            if (!inChoosingMode || !stateArea) return;

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
