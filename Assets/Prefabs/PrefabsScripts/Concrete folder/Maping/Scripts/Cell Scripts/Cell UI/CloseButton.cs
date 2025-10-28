using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseButton : MonoBehaviour
{
    [SerializeField] private GameObject panelInformation;

    public void ClosePanel()
    {
        panelInformation.SetActive(false);
    }
}
