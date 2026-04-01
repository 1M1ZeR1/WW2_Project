using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevelopMode : MonoBehaviour
{
    [SerializeField] private bool DevelopModeSwitcher_Inspector;
    public static bool DevelopModeSwitcher = false;

    private void Start()
    {
        DevelopModeSwitcher = DevelopModeSwitcher_Inspector;
    }
}
