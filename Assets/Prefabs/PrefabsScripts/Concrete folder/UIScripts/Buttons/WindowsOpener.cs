using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowsOpener : MonoBehaviour
{
    [SerializeField] private GameObject windowToOpen;

    public bool freeClickBlocked { private get; set; } = false;

    private void Start() { windowToOpen.SetActive(false); }

    public void WindowStateSwitch()
    {
        if(!freeClickBlocked)windowToOpen.SetActive(!windowToOpen.activeSelf);
    }
}
