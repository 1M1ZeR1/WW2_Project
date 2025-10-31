using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[CreateAssetMenu(fileName = "BuildPanel", menuName = "Panels/BuildPanel")]
[Preserve]
public class BuildPanelSO : PanelSO
{
    public Sprite buildSprite;
    public string buildName;

    [TextArea] public string buildDescription;

    public string buildId;
}
