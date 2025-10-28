using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonobehaviourMaster : MonoBehaviour
{
    public List<System.Action> actionsToUpdate { get; set; } = new();
    public void CoroutineStarter(IEnumerator function)
    {
        StartCoroutine(function);
    }
    private void Update()
    {
        foreach(var action in actionsToUpdate)
        {
            action.Invoke();
        }
    }
}
