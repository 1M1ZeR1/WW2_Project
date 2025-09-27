using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluenceController
{
    protected InfluenceResource influenceResource  = new InfluenceResource(0);

    public void AddInfluenceResource(int count)
    {
        influenceResource.ChangeCount(count);
    }
    public void RemoveInfluenceResource(int count)
    {
        influenceResource.ChangeCount(-count);
    }
    public int GetInfluenceCount() { return influenceResource.GetCount(); }
}
