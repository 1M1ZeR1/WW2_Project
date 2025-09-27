using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogsPanelScript : MonoBehaviour
{
    [SerializeField] private GameObject logsPanel;

    public void OpenClosePanel()
    {
        if (logsPanel.activeSelf)
        {
            logsPanel.SetActive(false);
        }
        else { logsPanel.SetActive(true); }
    }
}
