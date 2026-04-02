using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CellInfo : MonoBehaviour
{
    private DevelopMode DevelopMode;

    private void Start()
    {
        DevelopMode = GetComponent<DevelopMode>();
    }

    public void GetInfoBuilding(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            var cellBuildings = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(DevelopMode.currentInteractedCell).GetParameter<CellBuildings>().builds.ToList();

            if (cellBuildings.Count == 0) { CustomLog.YellowText($"On cell {DevelopMode.currentInteractedCell} no buildings"); }
            else
            {
                foreach (var build in cellBuildings)
                {
                    CustomLog.TextWithWordHighlight($"On cell {DevelopMode.currentInteractedCell.name} founded build", build.Key);
                }
            }
        }
    }
}
