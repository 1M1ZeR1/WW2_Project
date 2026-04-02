using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;



[CreateAssetMenu(fileName = "BuildsOnCells", menuName = "Custom/Create builds on cells taker")]
public class BuildsOnCellsDataHolder : ScriptableObject
{
    [SerializeField] private List<BuildsOnCells> buildsOnCells;

    public BuildsOnCells GetPresetByGameObject(GameObject cell)
    {
        if (buildsOnCells.Count != 0)
        {
            return buildsOnCells.FirstOrDefault(entry => entry.currentCellName == cell.name);
        }

        return null;
    }
}
[Serializable]
public class BuildsOnCells 
{
    public string currentCellName;

    public List<string> buildingsId = new List<string>();

}
