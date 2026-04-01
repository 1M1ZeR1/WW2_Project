using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CaptureCell_Develop : MonoBehaviour
{
    private GameObject currentInteractedCell;
    private void Start()
    {
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<DevelopMode, GameObject>((key, cell) => { currentInteractedCell = cell; });
    }

    public void CaptureCell_ByEnemys(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CustomLog.GreenText($"Trying to change the side on the cell({currentInteractedCell.name}) to Enemys");
            CellParametersHandler cellParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(currentInteractedCell);

            if (cellParametersHandler.GetParameter<CellArea>().Side == SideEnum.Enemys) return;
            cellParametersHandler.GetParameter<CellArea>().Side = SideEnum.Enemys;

            if(cellParametersHandler.GetParameter<CellSquadsOnArea>().squadsOnCell.Count != 0)
            {
                cellParametersHandler.GetParameter<CellSquadsOnArea>().squadsOnCell.ToList().ForEach(
                    squad => ServiceRegistry.WorkWithController<GameController>().SingleThrasher_Squad(squad));
            }
        }
    }
    public void CaptureCell_ByAllies(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CustomLog.GreenText($"Trying to change the side on the cell({currentInteractedCell.name}) to Allies");
            CellParametersHandler cellParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(currentInteractedCell);

            if (cellParametersHandler.GetParameter<CellArea>().Side == SideEnum.Allies) return;
            cellParametersHandler.GetParameter<CellArea>().Side = SideEnum.Allies;

            if (cellParametersHandler.GetParameter<CellSquadsOnArea>().squadsOnCell.Count != 0)
            {
                cellParametersHandler.GetParameter<CellSquadsOnArea>().squadsOnCell.ToList().ForEach(
                    squad => ServiceRegistry.WorkWithController<GameController>().SingleThrasher_Squad(squad));
            }
        }
    }
}
