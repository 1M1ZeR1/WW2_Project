using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NUnit.Framework;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasRenderer))]
public class UIDragLine : Graphic
{
    private Vector2 startPoint;
    private Vector2 endPoint;
    private bool drawing;

    public float thickness = 5f;

    private bool inHardCellMode = false;

    public void EnableHardCellMode() { inHardCellMode = true; }
    public void DisableHardCellMode() { inHardCellMode = false; }

    public void BeginLineFromEventTrigger()
    {
        var data = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        BeginLine(data);
    }
    private void BeginLine(PointerEventData data)
    {
        if (inHardCellMode)
        {
            drawing = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, data.position, Camera.main, out startPoint);
            endPoint = startPoint;
            SetVerticesDirty();
        }
    }

    public void UpdateLineFromEventTrigger()
    {
        var data = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        UpdateLine(data);
    }
    private void UpdateLine(PointerEventData data)
    {
        if (!drawing) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, data.position, Camera.main, out endPoint);
        SetVerticesDirty();
    }

    public void EndLineFromEventTrigger()
    {
        var data = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        EndLine(data);
    }
    private void EndLine(PointerEventData data)
    {
        drawing = false;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (!drawing) return;

        Vector2 dir = (endPoint - startPoint).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * thickness * 0.5f;

        Vector2 v1 = startPoint - normal;
        Vector2 v2 = startPoint + normal;
        Vector2 v3 = endPoint + normal;
        Vector2 v4 = endPoint - normal;

        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;

        vert.position = v1; vh.AddVert(vert);
        vert.position = v2; vh.AddVert(vert);
        vert.position = v3; vh.AddVert(vert);
        vert.position = v4; vh.AddVert(vert);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}
