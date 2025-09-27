using System.Collections;
using System.Collections.Generic;

public abstract class AbsctractResource
{
    private int _count;

    public void SetCount(int count)
    {
        _count = count;
    }
    public int GetCount()
    {
        return _count;
    } 
    public void ChangeCount(int count)
    {
        _count += count;
    }
}
public class WeaponResource : AbsctractResource
{
    public WeaponResource(int count)
    {
        SetCount(count);
    }
}
public class BuildingResource : AbsctractResource
{
    public BuildingResource(int count)
    {
        SetCount(count);
    }
}
public class PeopleResource : AbsctractResource
{
    public PeopleResource(int count)
    {
        SetCount(count);
    }
}
public class TransportResource : AbsctractResource
{
    public TransportResource(int count)
    {
        SetCount(count);
    }
}
public class InfluenceResource : AbsctractResource
{
    public InfluenceResource(int count)
    {
        SetCount(count);
    }
}
