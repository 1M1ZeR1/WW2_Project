using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CellsParameters", menuName = "Custom/Create cells parameters taker")]
public class ParametersCellsDataHolder : ScriptableObject
{
    [SerializeField] private List<Parameters> cellsParameters;

    public Parameters GetPresetByGameObject(GameObject cell)
    {
        if (cellsParameters.Count != 0 && cellsParameters[0].currentCell == null)
        {
            foreach (Parameters param in cellsParameters)
            {
                GameObject proccesingCell = GameObject.Find(param.currentCellName);
                param.currentCell = proccesingCell;

                param.neighboresCells.Clear();
                foreach (string names in param.neighboresCellsNames)
                {
                    param.neighboresCells.Add(GameObject.Find(names));
                }
            }
        }

            return cellsParameters.FirstOrDefault(entry => entry.currentCell == cell);
    }
    public List<Parameters> GetCellsParameters() { return cellsParameters; }
    public void SetNewParameters(List<Parameters> newParameters) { 
        cellsParameters = newParameters; Debug.Log("Успешно");
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }
}

[Serializable]
public class Parameters
{
    public string currentCellName;
    public GameObject currentCell;

    public string cellNameWhatPlayerSee = null;

    public List<string> neighboresCellsNames = new List<string>();
    public List<GameObject> neighboresCells = new List<GameObject>();

    public float height;
    public CellTypes_enum cellType;

    public bool isBase;

    public ControlSide controlSide;

    public List<BuildData> buildings = new();

    public bool isFront = false;
}
