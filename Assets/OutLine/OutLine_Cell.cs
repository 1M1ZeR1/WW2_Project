using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityFx.Outline;

public class OutLine_Cell : MonoBehaviour
{
    [SerializeField] private OutlineResources outlineResources;
    [SerializeField] private OutlineSettings outlineSettings;
    protected OutlineBehaviour outline;
    private void Awake()
    {
        outline = gameObject.AddComponent<OutlineBehaviour>();
        outline.RenderEvent = UnityEngine.Rendering.CameraEvent.AfterForwardOpaque;
        outline.Camera = Camera.main;
        outline.OutlineSettings = outlineSettings;
        outline.OutlineResources = outlineResources;
    }

    private void OnMouseEnter()
    {
        Debug.Log("Навёл на куб");
        outline.enabled = true;
    }
    private void OnMouseExit()
    {
        outline.enabled = false;
    }
}
