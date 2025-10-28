using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameLoader : MonoBehaviour
{
    private void Awake()
    {
        ServiceRegistry.Initialize();
    }
    private async void AddressableInitialization()
    {
        await Addressables.InitializeAsync().Task;
    }
    private void Start()
    {
        ServiceRegistry.Start();
    }
}
