using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellAccessibilityValidator
{
    public bool InteractWithAlliesCell(GameObject cell) 
    { 
        return ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().IsAllies();
    }
    public bool InteractWithNoEnemysCell(GameObject cell)
    {
        return !(ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().Side == SideEnum.Enemys);
    }
}
