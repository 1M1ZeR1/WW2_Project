using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class JSON_Importer : MonoBehaviour
{
    [SerializeField] private bool needToImport;

    [SerializeField] private string JSON_FileName;
    [SerializeField] private string Asset_FileName;

#if UNITY_EDITOR
    private void Start()
    {
        if (needToImport)
        {
            string jsonData = File.ReadAllText($@"C:\MapFolder\{JSON_FileName}.json");

            ObjectDataHolder importedData = ScriptableObject.CreateInstance<ObjectDataHolder>();
            JsonUtility.FromJsonOverwrite(jsonData, importedData);

            AssetDatabase.CreateAsset(importedData, $"Assets/MapFolder/Map V1/{Asset_FileName}.asset");
            AssetDatabase.SaveAssets();
        }
    }
#endif
}
