using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestPanelUIOpen : MonoBehaviour
{
    [SerializeField] private GameObject questPanelUI;

    public void OpenClose()
    {
        if (questPanelUI.activeSelf) { questPanelUI.SetActive(false); CameraMovementScript.UnBlockMovement(); }
        else { questPanelUI.SetActive(true); CameraMovementScript.BlockMovement(); }
    }
}
