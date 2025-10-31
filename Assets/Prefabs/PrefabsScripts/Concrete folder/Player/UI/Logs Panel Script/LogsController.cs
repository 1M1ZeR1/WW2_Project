using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogsController : MonoBehaviour
{
    public enum AccessLevel
    {
        Normal,
        Admin
    }
    [SerializeField]private static AccessLevel accessLevel = AccessLevel.Admin;
    public static LogsController Instance { get; private set;}

    [SerializeField] private GameObject logPanel;

    private GameObject logContent;

    private void Awake()
    {
        logContent = logPanel.transform.parent.gameObject;
        Instance = this;
    }
    public static void AddLogElement(string inputLogText, SideEnum sideEnum)
    {
        if(Instance != null)
        {
            if (sideEnum == SideEnum.Enemys && accessLevel == AccessLevel.Normal) return;

            if(sideEnum == SideEnum.None){Instance.AddToLog_Console(inputLogText);return;}
            Instance.AddToLog(inputLogText);
        }
    }
    public void AddToLog(string input)
    {
        GameObject newLogPanel = Instantiate(logPanel, logContent.transform);

        newLogPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = input;

        newLogPanel.SetActive(true);
    }
    public void AddToLog_Console(string input) 
    {
        GameObject newLogPanel = Instantiate(logPanel, logContent.transform);

        newLogPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = input;

        newLogPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.green;

        newLogPanel.SetActive(true);
    }
}
