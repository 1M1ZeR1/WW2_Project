using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class PanelStateSaver : MonoBehaviour
{
    [SerializeField] private bool StateSaverEnable = true;

    private void Awake()
    {
        this.enabled = false;
        this.enabled = true;

    }

    public void OpenWindowByGeneralUI()
    {
        gameObject.SetActive(true);
    }
    public void NeedSaveNotifier()
    {
        ServiceRegistry.WorkWithService<EventBus>().Publish<GeneralUiWindowsManager, PanelStateSaver, GameObject, bool>(null, this, gameObject, StateSaverEnable);
    }

    public void CloseWindowByGeneralUI()
    {
        gameObject.SetActive(false);
    }
}
