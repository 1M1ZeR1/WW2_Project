using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class AddingToLayer : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsToAdd;

    private void Start()
    {
        var outLineEffect = GetComponent<OutlineEffect>();
        foreach (GameObject obj in objectsToAdd)
        {
            outLineEffect.AddGameObject(obj);
        }
    }
}
