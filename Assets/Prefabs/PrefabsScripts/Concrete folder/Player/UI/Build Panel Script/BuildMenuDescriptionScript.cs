using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildMenuDescriptionScript : MonoBehaviour
{
    [Header("Информационное окно")]
    [SerializeField] private GameObject messagePanel;
    private MessageScript messageScript;
    void Start()
    {
        messagePanel.TryGetComponent<MessageScript>(out  messageScript);
    }

    public void PushMessage(int buildType)
    {
        if (buildType == 1)
        {
            messagePanel.SetActive(true);

            messageScript.SendMessage(10);
        }
        if(buildType == 2)
        {
            messagePanel.SetActive(true);

            messageScript.SendMessage(11);
        }
        if(buildType == 3)
        {
            messagePanel.SetActive(true);

            messageScript.SendMessage(12);
        }
    }
}
