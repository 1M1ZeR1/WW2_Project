using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellVisiableMode : MonoBehaviour
{
    private bool inTracking = false;
    public void StartTracking()=>inTracking = true;

    private void OnBecameVisible()
    {
        if (inTracking) ServiceRegistry.WorkWithService<CellsInCameraAreaScript>().ChangedVisiableState(gameObject);
    }
    private void OnBecameInvisible()
    {
        if (inTracking) ServiceRegistry.WorkWithService<CellsInCameraAreaScript>().ChangedVisiableState(gameObject);
    }
}
