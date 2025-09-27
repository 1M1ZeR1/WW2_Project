using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildItemPanel : MonoBehaviour
{
    [SerializeField] private BuildsEnum buildType;
    public BuildsEnum GetBuidType() { return buildType; }
}
