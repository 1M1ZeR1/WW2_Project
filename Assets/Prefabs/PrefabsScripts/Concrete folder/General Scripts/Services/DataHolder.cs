using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
