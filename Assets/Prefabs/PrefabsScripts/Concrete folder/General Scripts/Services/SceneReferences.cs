using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;


public abstract class PanelSO : ScriptableObject
{
    public string title;
    [TextArea] public string description;
}


