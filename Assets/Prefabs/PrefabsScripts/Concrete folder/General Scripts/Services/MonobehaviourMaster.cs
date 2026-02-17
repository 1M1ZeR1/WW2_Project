using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonobehaviourMaster : MonoBehaviour
{
    public List<System.Action> actionsToUpdate { get; set; } = new();
    public Coroutine CoroutineStarter(IEnumerator function)
    {
        return StartCoroutine(function);
    }
    public void CoroutineStopper(Coroutine coroutineToStop) { StopCoroutine(coroutineToStop); }
    private void Update()
    {
        foreach(var action in actionsToUpdate)
        {
            action.Invoke();
        }
    }
}
