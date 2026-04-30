using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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
    [SerializeField] private GameObject logList;

    private GameObject logContent;

    private void Awake()
    {
        logContent = logPanel.transform.parent.gameObject;
        Instance = this;
    }
    public static void AddLogElement(GlobalLogElement logElement, SideEnum sideEnum)
    {
        if(Instance != null)
        {
            if (sideEnum == SideEnum.Enemys && accessLevel == AccessLevel.Normal) return;

            //if(sideEnum == SideEnum.None){Instance.AddToLog(inputLogText);return;}
            Instance.AddToLog(logElement,logElement.ConnectedCell);
        }
    }
    public void AddToLog(GlobalLogElement logElement, GameObject cellThisEvent = null)
    {
        GameObject newLogPanel = Instantiate(logPanel, logContent.transform);

        newLogPanel.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = logElement.TextTitle;

        if (cellThisEvent != null)
        {
            var trigger = newLogPanel.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) =>
            {
                ServiceRegistry.WorkWithController<FocusOnCellScript>().FocusToCell(cellThisEvent);
                logList.SetActive(false);

            });

            trigger.triggers.Add(entry);

            trigger.transform.parent.gameObject.SetActive(true);

            newLogPanel.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 0.5f);
        }

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

public class GlobalLogElement
{
    public string TextTitle { set; get; }
    public string TextContent { set; get; }

    public GameObject ConnectedCell {private set; get; }
    public GlobalLogElement(GameObject cell){ ConnectedCell = cell; }
    public GlobalLogElement(string textTitle, string textContent)
    {
        TextTitle = textTitle;
        TextContent = textContent;
    }
}
