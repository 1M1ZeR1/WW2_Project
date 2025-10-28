using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellWorkingScript : MonoBehaviour
{
    private void OnMouseEnter()
    {
        //Debug.Log($"Cell:{gameObject}||Side:{ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(gameObject).GetParameter<CellArea>().GetSide()}");

        ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic
            (
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(gameObject).GetParameter<CellArea>().Side,
            gameObject,
            true
            );
    }
    private void OnMouseExit() 
    {
        ServiceRegistry.WorkWithController<CellInteraction>().SetMaterialBySide_Basic
            (
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(gameObject).GetParameter<CellArea>().Side,
            gameObject,
            false
            );
    }
}
