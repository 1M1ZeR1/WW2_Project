using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    private void Awake()
    {
        ServiceRegistry.Initialize();
    }
    private void Start()
    {
        ServiceRegistry.Start();
    }
}
