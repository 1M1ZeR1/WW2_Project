using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellTypes_enum
{
    None,
    Plain,
    Forest,
    River,
    Beach,
    City,
    Village
}
public abstract class AbstractCell
{
    private List<AbstractSquad> _squads = new List<AbstractSquad>();
    private List<AbstractBuildings> _buildings = new List<AbstractBuildings>();

    private AbstractBuffs _buff;

    private string _type;
    public void SetBuff(AbstractBuffs buff)
    {
        _buff = buff;
    }
    public string GetBuffDiscription() { return _buff.GetBuffDescription();}
    public List<AbstractSquad> GetSquads() { return _squads; }

    virtual public string GetTypeCell()
    {
        return _type;
    }

    public void AddSquad(AbstractSquad squad)
    {
        _squads.Add(squad);
    }
    public void RemoveSquad(AbstractSquad squad)
    {
        _squads.Remove(squad);
    }
    public AbstractBuffs GetBuff() { return _buff; }
}

public class CommonCell:AbstractCell
{
    
}
public class PlainCell : AbstractCell
{
    private readonly string _type = "Равнина";

    public PlainCell(int scale) 
    {
        PlainBuff newPlainBuff = new PlainBuff();
        newPlainBuff.SetBuffScale(scale);

        SetBuff(newPlainBuff);
    }
    override public string GetTypeCell()
    {
        return this._type;
    }
}
public class ForestCell : AbstractCell
{
    private readonly string _type = "Лес";

    public ForestCell(int scale)
    {
        ForestBuff newForestBuff = new ForestBuff();
        newForestBuff.SetBuffScale(scale);

        SetBuff(newForestBuff);
    }
    override public string GetTypeCell()
    {
        return this._type;
    }
}
public class LakeCell: AbstractCell
{
    private readonly string _type = "Озеро";

    public LakeCell(AbstractBuffs cellBuff)
    {
        SetBuff(cellBuff);
    }
    override public string GetTypeCell()
    {
        return this._type;
    }
}
public class RiverCell : AbstractCell
{
    private readonly string _type = "Река";

    public RiverCell()
    {

    }
    override public string GetTypeCell()
    {
        return this._type;
    }
}
public class BeachCell : AbstractCell
{
    private readonly string _type = "Берег";

    public BeachCell()
    { 
    }
    override public string GetTypeCell()
    {
        return _type;
    }
}
public class CityCell : AbstractCell
{
    private readonly string _type = "Город";

    public CityCell(int scale)
    {
        CityBuff newCityBuff = new CityBuff();
        newCityBuff.SetBuffScale(scale);

        SetBuff(newCityBuff);
    }

    override public string GetTypeCell()
    {
        return _type;
    }
}
