using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBattlesPanelScript : MonoBehaviour
{
    [Header("Панель всех битв")]
    [SerializeField] private GameObject battlesPanel;

    public void OpenClose()
    {
        if (battlesPanel.activeSelf) { battlesPanel.SetActive(false); }
        else { battlesPanel.SetActive(true);}
    }
}
