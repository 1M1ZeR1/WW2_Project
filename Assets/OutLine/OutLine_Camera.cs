using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class OutLine_Camera : MonoBehaviour
{
    [SerializeField] private OutlineResources outlineResources;
    [SerializeField] private OutlineLayerCollection outlineLayer;

    private void Awake()
    {
        var newEffect = gameObject.AddComponent<OutlineEffect>();
        newEffect.OutlineResources = outlineResources;
        newEffect.RenderEvent = UnityEngine.Rendering.CameraEvent.AfterForwardOpaque;
        newEffect.OutlineLayers = outlineLayer;
    }
}
