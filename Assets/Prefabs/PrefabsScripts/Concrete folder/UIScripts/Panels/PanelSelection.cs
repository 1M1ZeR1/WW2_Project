using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSelection
{
    private SelectionMode selectionMode;
    private OutLineMode lineMode;
    public PanelSelection(SelectionMode selectionMode, OutLineMode outLineMode = OutLineMode.None)
    {
        this.selectionMode = selectionMode;
        lineMode = outLineMode;
    }

    public enum SelectionMode
    {
        Sizing
    }
    public enum OutLineMode
    {
        None,
        ColorLine
    }

    private List<GameObject> selectedPanels = new();
    private Color outlineColor;
    
    public void SelectPanel(GameObject panel)
    {
        selectedPanels.Add(panel);

        switch (selectionMode) 
        {
            case SelectionMode.Sizing:
                panel.transform.localScale = new Vector3(0.9f, 0.9f, 1);
                break;
        }
    }
    public void RemoveSelection(GameObject panel) 
    {
        selectedPanels.Remove(panel);

        switch (selectionMode)
        {
            case SelectionMode.Sizing:
                panel.transform.localScale = new Vector3(1f, 1f, 1f);
                break;
        }
    }
    public void ClearSelections()
    {
        if(selectedPanels.Count != 0) { while(selectedPanels.Count > 0) { RemoveSelection(selectedPanels[0]); } }
    }
}
