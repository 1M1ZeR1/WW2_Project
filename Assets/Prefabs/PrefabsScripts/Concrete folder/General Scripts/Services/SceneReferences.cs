using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public abstract class PanelSO : ScriptableObject
{
    public string title;
    [TextArea] public string description;
}

[CreateAssetMenu(fileName = "BuildPanel", menuName = "Panels/BuildPanel")]
public class BuildPanelSO : PanelSO
{
    public Sprite buildSprite;
    public string buildName;

    [TextArea]public string buildDescription;

    public string buildId;
}
