using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusOnCellScript : MonoBehaviour
{
    private Vector3 focusPosition;
    private bool inFocus = false;
    public void FocusToCell(GameObject cell, GameObject panelWhatInvoke = null)
    {
        Vector3 positionToFocus = cell.transform.position;
        positionToFocus.y = transform.position.y;

        focusPosition = positionToFocus;

        inFocus = true;

        if (panelWhatInvoke != null) panelWhatInvoke.SetActive(false);
    }

    void Update()
    {
        if (inFocus)
        {
            float distance = Vector3.Distance(transform.position, focusPosition);

            float speed = distance * 5;

            float step = speed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, focusPosition, step);

            if(Vector3.Distance(transform.position, focusPosition) < 10) { inFocus = false;focusPosition = Vector3.zero; }
        }
    }
}
