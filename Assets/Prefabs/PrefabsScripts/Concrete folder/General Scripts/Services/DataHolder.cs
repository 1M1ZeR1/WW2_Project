using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DataHolder
{
    private Dictionary<string, object> idScriptableData = new();

    public void AddDataToHolder(string id, object data)
    {
        idScriptableData.Add(id, data);
    }

    public object GetDataFromHolder(string id)
    {
        Debug.Log($"{this}:Trying to get data by id:{id}");
        if(idScriptableData.TryGetValue(id, out object data))
        {
            return data;
        }
        else { Debug.LogError($"{this}:Cannot find data in data dictionary by id:{id}"); return null; }
    }
}
public static class DataLoader
{
    public static Action<object, List<object>> LoadedArrayData;

    public static void LoadArrayData<T>(object sender, string label,string ided_id=null) where T : ScriptableObject
    {
        var results = new List<object>();

        Addressables.LoadAssetsAsync<T>(label, asset =>
        {
            results.Add(asset);
        }).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                LoadedArrayData?.Invoke(sender, results);
            }
            else
            {
                Debug.LogError($"Не удалось загрузить Addressables по label {label}");
                LoadedArrayData?.Invoke(sender, null);
            }
        };
    }
}
