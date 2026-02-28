using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelStateSaver : MonoBehaviour
{
    private void Awake()
    {
        this.enabled = false;
        this.enabled = true;
    }

    private void OnEnable()
    {
        ServiceRegistry.WorkWithService<EventBus>().Publish<GeneralUiWindowsManager,PanelStateSaver,GameObject>(null,this,gameObject);
    }
}
