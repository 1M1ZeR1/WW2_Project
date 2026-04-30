using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellsInCameraAreaScript
{
    public List<GameObject> visiableCells { get; private set; } = new();

    private Camera mainCamera;

    public void Start()
    {
        mainCamera = Camera.main;

        GameObject.FindObjectsByType<CellVisiableMode>(FindObjectsSortMode.None).ToList().ForEach(s => s.StartTracking());

        ForceInitializeVisibleCells();
    }

    private void ForceInitializeVisibleCells()
    {
        foreach (var cell in GameObject.FindGameObjectsWithTag("Interactable Cell"))
        { 
            if(IsVisibleToCamera(cell))visiableCells.Add(cell);
        }
    }
    private bool IsVisibleToCamera(GameObject cell)
    {
        if (mainCamera == null) return false;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            MeshRenderer mr = cell.GetComponent<MeshRenderer>();
            if (mr == null) return false;
            return GeometryUtility.TestPlanesAABB(planes, mr.bounds);
        }
        return GeometryUtility.TestPlanesAABB(planes, sr.bounds);
    }

    public void ChangedVisiableState(GameObject cell)
    {
        if (visiableCells.Contains(cell)) visiableCells.Remove(cell);
        else visiableCells.Add(cell);
    }
}
