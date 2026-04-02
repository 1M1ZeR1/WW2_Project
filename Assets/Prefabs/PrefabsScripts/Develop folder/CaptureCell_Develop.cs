using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CaptureCell_Develop : MonoBehaviour
{
    private DevelopMode DevelopMode;
    private void Start()
    {
        DevelopMode = GetComponent<DevelopMode>();
    }

    public void CaptureCell_ByEnemys(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CustomLog.GreenText($"Trying to change the side on the cell({DevelopMode.currentInteractedCell.name}) to Enemys");
            CellParametersHandler cellParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(DevelopMode.currentInteractedCell);

            if (cellParametersHandler.GetParameter<CellArea>().Side == SideEnum.Enemys) return;
            cellParametersHandler.GetParameter<CellArea>().RequestToControlCell(SideEnum.Enemys);

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
            CustomLog.GreenText($"Trying to change the side on the cell({DevelopMode.currentInteractedCell.name}) to Allies");
            CellParametersHandler cellParametersHandler = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(DevelopMode.currentInteractedCell);

            if (cellParametersHandler.GetParameter<CellArea>().Side == SideEnum.Allies) return;
            cellParametersHandler.GetParameter<CellArea>().RequestToControlCell(SideEnum.Allies);

            if (cellParametersHandler.GetParameter<CellSquadsOnArea>().squadsOnCell.Count != 0)
            {
                cellParametersHandler.GetParameter<CellSquadsOnArea>().squadsOnCell.ToList().ForEach(
                    squad => ServiceRegistry.WorkWithController<GameController>().SingleThrasher_Squad(squad));
            }
        }
    }
}
