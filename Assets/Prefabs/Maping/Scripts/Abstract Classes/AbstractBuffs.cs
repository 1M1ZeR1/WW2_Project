using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum BuffTypes
{
    None,
    Territory,
    Building,
    Event
}
public abstract class AbstractBuffs
{
    private BuffTypes buffType;

    private int _scale;

    private DateTime timeToExpire;
    public delegate void ClearBuff(AbstractBuffs buffs);
    public event ClearBuff RequestToClear;

    public virtual void InvokeClearEvent(AbstractBuffs buff) { RequestToClear.Invoke(buff); }

    public void SetBuffType( BuffTypes buffType) { this.buffType = buffType; }
    public BuffTypes GetBuffTypes() { return buffType; }

    public virtual AbstractBuffs SetBuffScale(int scale)
    {
        _scale = scale;
        return this;
    }
    public int GetBuffScale()
    {
        return _scale;
    }
    virtual public string GetBuffDescription()
    {
        return "";
    }

    public void CreateTimeToExpire(DateTime currentTime, int hours)
    {
        timeToExpire = currentTime.AddHours(hours);
    }
    public DateTime GetTimeToExpire() { return  timeToExpire; }
    public bool NeedToExpire(DateTime currentTime)
    {
        if(currentTime == timeToExpire) { RequestToClear.Invoke(this);return true; }
        return false;
    }

    virtual public void ChangeCharacteristics(AbstractSquad squad) { }
}

public class RandomEventBuff : AbstractBuffs, IChangeSquadParameters
{
    private readonly List<string> DiscriptionSAP;
    private readonly float[] hardSAP;

    public float[] sap_scale { set; get; }

    public RandomEventBuff(float[] hardSAP, DateTime dateTime, int hours)
    {
        DiscriptionSAP = new List<string>();

        CreateTimeToExpire(dateTime, hours);
        SetBuffType(BuffTypes.Event);

        if (hardSAP != null)
        {
            this.hardSAP = hardSAP;

            if (hardSAP[0] != 0) { DiscriptionSAP.Add($"Скорость:{hardSAP[0]}%\n"); }
            if (hardSAP[1] != 0) { DiscriptionSAP.Add($"Атака:{hardSAP[1]}%\n"); }
            if (hardSAP[2] != 0) { DiscriptionSAP.Add($"Защита:{hardSAP[2]}%\n"); }

            var dateToExpire = GetTimeToExpire();

            DiscriptionSAP.Add($"До: {dateToExpire.Hour}:{dateToExpire.Minute}   {dateToExpire.Day}.{dateToExpire.Month}.{dateToExpire.Year}");
        }
    }
    public override string GetBuffDescription()
    {
        string result = "";

        foreach (var item in DiscriptionSAP)
        {
            result += item;
        }

        return result;
    }
    public override void ChangeCharacteristics(AbstractSquad squad)
    {
        float attack = 0;
        float speed = 0;
        float protection = 0;

        if (hardSAP[0] != 0) { speed = squad.Speed / 100f * hardSAP[0]; }
        if (hardSAP[1] != 0) { attack = squad.Attack / 100f * hardSAP[1]; }
        if (hardSAP[2] != 0) { protection = squad.Protection / 100f * hardSAP[2]; }

        SetSAP(speed, attack, protection);
    }

    public void SetSAP(float speed, float attack, float protection)
    {
        sap_scale = new float[]
        {
            speed, attack, protection
        };
    }
    public void AddBuff(AbstractSquad squad)
    {
        squad.AddBuff(this);
    }
    public float[] GetSAP() { return sap_scale; }
}

public enum UnReachableBuffType
{
    None,
    Fire,
    River
}
public class UnReachableBuff : AbstractBuffs,IEventBuff
{
    private readonly UnReachableBuffType unReachableBuffType;

    private GameObject cell;
    public UnReachableBuff(DateTime dateTime, int hours, UnReachableBuffType unReachableBuffType, GameObject cell) 
    {
        CreateTimeToExpire(dateTime, hours);
        SetBuffType(BuffTypes.Event);

        this.unReachableBuffType = unReachableBuffType;

        this.cell = cell;
    }
    public override void InvokeClearEvent(AbstractBuffs buff)
    {
       base.InvokeClearEvent(buff);
    }

    public void Execute()
    {
        //ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
        //    GetParameter<CellMovementParameters>().
    }
    public void Remove()
    {

    }

    public override string GetBuffDescription()
    {
        DateTime dateToExpire = GetTimeToExpire();

        switch (unReachableBuffType)
        {
            case UnReachableBuffType.Fire: return $"Клетка недостижима\nДо: {dateToExpire.Hour}:{dateToExpire.Minute}   {dateToExpire.Day}.{dateToExpire.Month}.{dateToExpire.Year}";
            case UnReachableBuffType.River: return $"Клетка недостижима\nДо: {dateToExpire.Hour}:{dateToExpire.Minute}   {dateToExpire.Day}.{dateToExpire.Month}.{dateToExpire.Year}";
        }

        return "";
    }
}

public class PlainBuff : AbstractBuffs,IChangeSquadParameters
{
    public float[] sap_scale { get; set; }

    public PlainBuff() { SetBuffType(BuffTypes.Territory); }
    public override string GetBuffDescription()
    {
        return $"Защита:-{GetBuffScale()}%\nАтака:+{GetBuffScale()}%";
    }
    public override void ChangeCharacteristics(AbstractSquad squad)
    {
        float attack = squad.Attack / 100f * GetBuffScale();
        float protection = squad.Protection / 100f * GetBuffScale();

        SetSAP(0,attack,-protection);
    }

    public void AddBuff(AbstractSquad squad)
    {
        squad.AddBuff(this);
    }

    public void SetSAP(float speed, float attack, float protection)
    {
        sap_scale = new float[]
        {
            speed, attack, protection
        };
    }

    public float[] GetSAP()
    {
        return sap_scale;
    }
}
public class ForestBuff : AbstractBuffs,IChangeSquadParameters
{
    public float[] sap_scale { get; set; }

    public ForestBuff() { SetBuffType(BuffTypes.Territory); }
    public override string GetBuffDescription()
    {
        return $"Защита:+{GetBuffScale()}%\nСкорость:-{GetBuffScale()}%";
    }
    public override void ChangeCharacteristics(AbstractSquad squad)
    {
        float speed = squad.Speed / 100f * GetBuffScale();
        float protection = squad.Protection / 100f * GetBuffScale();

        SetSAP(-speed,0,protection);
    }

    public void AddBuff(AbstractSquad squad)
    {
        squad.AddBuff(this);
    }

    public void SetSAP(float speed, float attack, float protection)
    {
        sap_scale = new float[]
        {
            speed, attack, protection
        };
    }

    public float[] GetSAP()
    {
        return sap_scale;
    }
}
public class CityBuff : AbstractBuffs,IChangeSquadParameters
{
    private int protectionScale = 0;
    private float speedScale = 0f;

    public float[] sap_scale { get; set; }

    public CityBuff() { SetBuffType(BuffTypes.Territory); }
    public override string GetBuffDescription()
    {
        return $"Защита:+{protectionScale}%\nСкорость:-{speedScale}%";
    }
    public override void ChangeCharacteristics(AbstractSquad squad)
    {
        float speed = squad.Speed / 100f * speedScale;
        float protection = squad.Protection / 100f * protectionScale;

        SetSAP(-speed,0,protection);
    }
    public override AbstractBuffs SetBuffScale(int scale)
    {
        protectionScale = scale * 4;
        speedScale = scale;

        return this;
    }

    public void AddBuff(AbstractSquad squad)
    {
        squad.AddBuff(this);
    }

    public void SetSAP(float speed, float attack, float protection)
    {
        sap_scale = new float[]
        {
            speed, attack, protection
        };
    }

    public float[] GetSAP()
    {
        return sap_scale;
    }
}

//Squads buffs
public class Mativation : AbstractBuffs,IChangeSquadParameters
{
    public float[] sap_scale { get; set; }

    public override string GetBuffDescription()
    {
        return $"Атака:+{GetBuffScale()}\nСкорость:+{GetBuffScale()}%";
    }
    public override void ChangeCharacteristics(AbstractSquad squad)
    {
        SetBuffScale(20);

        float speed = squad.Speed / 100f * GetBuffScale();
        float attack = squad.Attack / 100f * GetBuffScale();

        SetSAP(speed, attack, 0);
    }

    public void AddBuff(AbstractSquad squad)
    {
        squad.AddBuff(this);
    }

    public void SetSAP(float speed, float attack, float protection)
    {
        sap_scale = new float[]
        {
            speed, attack, protection
        };
    }

    public float[] GetSAP()
    {
        return sap_scale;
    }
}
public class Rested : AbstractBuffs,IChangeSquadParameters
{
    public float[] sap_scale { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override string GetBuffDescription()
    {
        return $"Атака:+{GetBuffScale()}%\nЗащита:+{GetBuffScale()}%\nСкорость:+{GetBuffScale()}%";
    }

    public override void ChangeCharacteristics(AbstractSquad squad)
    {
        SetBuffScale(10);

        float speed = squad.Speed /100f * GetBuffScale();
        float attack = squad.Attack / 100f * GetBuffScale();
        float protection = squad.Protection /100f * GetBuffScale();

        SetSAP(speed,attack,protection);
    }

    public void AddBuff(AbstractSquad squad)
    {
        squad.AddBuff(this);
    }

    public void SetSAP(float speed, float attack, float protection)
    {
        sap_scale = new float[]
        {
            speed, attack, protection
        };
    }

    public float[] GetSAP()
    {
        return sap_scale;
    }
}
public class InfantrySkill : AbstractBuffs,IChangeSquadParameters
{
    public float[] sap_scale { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public InfantrySkill(int buffScale, DateTime dateTime, int hours)
    {
        SetBuffScale(buffScale);
        CreateTimeToExpire(dateTime, hours);
    }
    public override string GetBuffDescription()
    {
        return $"Атака:+{GetBuffScale()}";
    }
    public override void ChangeCharacteristics(AbstractSquad squad)
    {
        SetSAP(0, 20, 0);
    }

    public void AddBuff(AbstractSquad squad)
    {
        squad.AddBuff(this);
    }

    public void SetSAP(float speed, float attack, float protection)
    {
        sap_scale = new float[]
        {
            speed, attack, protection
        };
    }

    public float[] GetSAP()
    {
        return sap_scale;
    }
}
public class TanksSkill : AbstractBuffs,IChangeSquadParameters
{
    public float[] sap_scale { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public TanksSkill(int buffScale)
    {
        SetBuffScale(buffScale);
        CreateTimeToExpire(TimeControllerScript.GetCurrentDateTime(), 24);
    }
    public override string GetBuffDescription()
    {
        return $"Атака:+{GetBuffScale()}%\nЗащита:+{GetBuffScale()}%";
    }
    public override void ChangeCharacteristics(AbstractSquad squad)
    {
        float attack = squad.Attack/100f*GetBuffScale();
        float protection = squad.Protection/100f*GetBuffScale();

        SetSAP(0,attack,protection);
    }

    public void AddBuff(AbstractSquad squad)
    {
        squad.AddBuff(this);
    }

    public void SetSAP(float speed, float attack, float protection)
    {
        sap_scale = new float[]
        {
            speed, attack, protection
        };
    }

    public float[] GetSAP()
    {
        return sap_scale;
    }
}

//Building buffs
public class FoxholeBuff : AbstractBuffs,IChangeSquadParameters
{
    public float[] sap_scale { get; set; }

    public FoxholeBuff() { SetBuffType(BuffTypes.Building); }
    public override string GetBuffDescription()
    {
        return $"Атака:+5\nЗащита:+20";
    }
    public override void ChangeCharacteristics(AbstractSquad squad)
    {
        SetSAP(0,5,20);
    }

    public void AddBuff(AbstractSquad squad)
    {
        squad.AddBuff(this);
    }

    public void SetSAP(float speed, float attack, float protection)
    {
        sap_scale = new float[]
        {
            speed, attack, protection
        };
    }

    public float[] GetSAP()
    {
        return sap_scale;
    }
}

//public class CampBuff : AbstractBuffs
//{
//    public CampBuff(int buffScale) { SetBuffType(BuffTypes.None);SetBuffScale(buffScale); }

//    public override void ChangeCharacteristics(AbstractSquad squad)
//    {
//        return;
//    }
//    public void ChangeBuffScale(int level)
//    {
//        if(level == 1) { SetBuffScale(2);}
//        if(level == 2) { SetBuffScale(4);}
//        if(level == 3) { SetBuffScale(6);}
//    }
//}
public class FortBuff : AbstractBuffs,IChangeSquadParameters
{
    public float[] sap_scale { get; set; }

    public FortBuff() { SetBuffScale(20); SetBuffType(BuffTypes.Building); }
    public override string GetBuffDescription()
    {
        return $"Защита:+{GetBuffScale()}%";
    }
    public override void ChangeCharacteristics(AbstractSquad squad)
    {
        float protection = squad.Protection/100f * GetBuffScale();

        SetSAP(0,0,protection);
    }

    public void AddBuff(AbstractSquad squad)
    {
        squad.AddBuff(this);
    }

    public void SetSAP(float speed, float attack, float protection)
    {
        sap_scale = new float[]
        {
            speed, attack, protection
        };
    }

    public float[] GetSAP()
    {
        return sap_scale;
    }
}
public class MilitaryAcademyBuff : AbstractBuffs
{
    public MilitaryAcademyBuff() { SetBuffType(BuffTypes.None); }

    public override void ChangeCharacteristics(AbstractSquad squad)
    {

        return;
    }
}

public interface IEventBuff
{
    public void Execute();
    public void Remove();
}

public interface IChangeSquadParameters
{
    public float[] sap_scale { set; get; }

    public void AddBuff(AbstractSquad squad);
    public void SetSAP(float speed, float attack, float protection);
    public float[] GetSAP();
    public void ChangeCharacteristics(AbstractSquad squad);

}
