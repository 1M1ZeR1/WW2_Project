using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[CreateAssetMenu(menuName = ("Data objects/Build data"))]
[Preserve]
public class BuildData : ScriptableObject
{
    public string Id;

    public int MaxLevel;
    public int TimeToBuild;
    public int TimeToUpgrade;

    public int CostToBuild;
    public int CostToUpgrade;
    public int CostMultiplier;
}
