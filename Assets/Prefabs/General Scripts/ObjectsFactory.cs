using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsFactory
{
    private Dictionary<Type, List<GameObject>> savedObjects = new();

    public void SaveObject<T>(GameObject objectToSave)
    {
        if (savedObjects.ContainsKey(typeof(T)))
        {
            savedObjects[typeof(T)].Add(objectToSave);
        }
        else
        {
            savedObjects.Add(typeof(T), new() { objectToSave});
        }
    }
    public GameObject CreateObject<T>(Transform? parent = null)
    {
        if (!savedObjects.ContainsKey(typeof(T))) { return null; }
        else
        {
            if(parent == null) return GameObject.Instantiate(savedObjects[typeof(T)][0]);
            else { return GameObject.Instantiate(savedObjects[typeof(T)][0],parent); }
        }
    }
    public GameObject CreateObject<T>(int index)
    {
        if (!savedObjects.ContainsKey(typeof(T))) { return null; }
        else
        {
            return GameObject.Instantiate(savedObjects[typeof(T)][index]);
        }
    }
}
