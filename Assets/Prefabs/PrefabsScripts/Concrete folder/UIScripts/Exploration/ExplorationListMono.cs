using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorationListMono : MonoBehaviour
{
    private void OnEnable()
    {
        ServiceRegistry.WorkWithService<EventBus>().Publish<ExplorationSquadList, ExplorationListMono>(null, this);       
    }
}
