using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using UnityEngine.UI;


public class SquadAttribute : Attribute
{
    public SquadEnum SquadType { get; set; }// Временное решение
    public string Id { get; set; }

    public SquadAttribute(SquadEnum squadType)=>SquadType = squadType;
    public SquadAttribute(string id) => Id = id;
}
public enum SquadEnum
{
    None,
    Infantry,
    Engineer,
    Scouts,
    Artillery,
    Tanks
}
public enum SkillType
{
    None,
    Buff,
    Action
}


public interface ISquad_Master : ISquad_Dead, ISquad_Parameters, ISquad_Skill, ISquad_Tools { }
public interface ISquad_Dead
{
    bool IsDead { get;set; }
}
public interface ISquad_Parameters
{
    float Speed { get; set; }
    int Attack {  get; set; }
    int Protection { get; set; }
    int PeopleCount { get; set; }
    int TrainingTime { get; set; }
    int BuildingSkill { get; set; }
}
public interface ISquad_Skill
{
    int SkillCooldown { get; set; }
    bool IsSkillInCooldown { get; set; }
    string SkillName { get; set; }
}
public interface ISquad_Tools
{
    SquadWeapon Weapon { get; set; }
    SquadTransport Transport { get; set; }
}


public abstract class AbstractSquad : ISquad_Master, IType<SquadEnum>, IName, ISide,IClone
{
    public List<AbstractBuffs> buffs { get; set; } = new();

    public SideEnum Side { get; set; }
    private SquadActions _action { get; set; } = SquadActions.None;
    public SquadActions SquadAction
    {
        get { return _action; }
        set
        {
            if(value == SquadActions.None) { ServiceRegistry.WorkWithService<EventBus>().Publish<string, AbstractSquad, bool>("ChangeState_BlockAction", this, false); }
            else { ServiceRegistry.WorkWithService<EventBus>().Publish<string, AbstractSquad, bool>("ChangeState_BlockAction", this, true); }

            _action = value;
        }
    }

    public SquadWeapon Weapon { get; set; }
    public SquadTransport Transport { get; set; }

    public bool IsDead { get; set; }

    public SquadEnum Type { get; set; }
    public string Name { get; set; }

    public float Speed { get; set; }
    public int Attack { get; set; }
    public int Protection { get; set; }
    public int PeopleCount { get; set; }

    public int TrainingTime { get; set; }

    public int BuildingSkill { get; set; }

    public int SkillCooldown { get; set; }
    public bool IsSkillInCooldown { get; set; }
    public string SkillName { get; set; }

    public abstract object Clone();
    public abstract void Initialize(float speedOfMovement, int countOfPeople, SquadTransport typeTransport, SquadWeapon typeWeapon);

    public void AddBuff(AbstractBuffs buff)
    {
        buff.ChangeCharacteristics(this);

        buffs.Add(buff);
    }
    public void AddBuffWithTimer(AbstractBuffs buff)
    {
        buff.ChangeCharacteristics(this);

        buffs.Add(buff);

        buff.RequestToClear += BuffRemoverListener;
    }
    private void BuffRemoverListener(AbstractBuffs buff)
    {
        buffs.Remove(buff);
    }
    public void RemoveBuff(AbstractBuffs buff)
    {
        buffs.Remove(buff);
    }
    public List<AbstractBuffs> GetAllSquadBuffs() { return buffs; }

    public float GetAllSpeedOfMovement() 
    {
        float currentSpeed = Speed;

        if(buffs.Count != 0)
        {
            foreach(var buff in buffs)
            {
                currentSpeed += ((IChangeSquadParameters)buff).GetSAP()[0];
            }
        }

        return currentSpeed;
    }
    public int GetAllAttack() 
    {
        int currentAttack = Attack;
        
        if(buffs.Count != 0)
        {
            foreach(var buff in buffs)
            {
                currentAttack += (int)((IChangeSquadParameters)buff).GetSAP()[1];
            }
        }

        return currentAttack;
    }
    public int GetAllProtection() 
    {
        int currentProtection = Protection;

        if(buffs.Count != 0)
        {
            foreach(var buff in buffs)
            {
                currentProtection += (int)((IChangeSquadParameters)buff).GetSAP()[2];
            }
        }

        return currentProtection;
    }


    public virtual void KillSomePerson(int count)
    {
        PeopleCount -= count;

        CalculateParam();
        AddModifications();
    }
    public virtual void CalculateParam()
    {
        Attack = PeopleCount * 2;
        Protection = PeopleCount * 2;
    }
    public void AddModifications()
    {
        Attack = TypesConverter.ConvertWeaponType(Weapon, Attack);
        Protection = TypesConverter.ConvertTransportTypeProtection(Transport,Protection);
    }
    public void RecalculateBuffsEffects()
    {
        foreach(var buff in buffs)
        {
            buff.ChangeCharacteristics(this);
        }
    }

    public virtual Action UseSkill(int time, SkillType skillType, Action action, AbstractBuffs buff) 
    {

        if(skillType == SkillType.Action)
        {
            GameController.AddActionToQueue(action);
            return null;
        }
        else { return null; }
    }
    public virtual IEnumerator SkillCooldownCoroutine()
    {
        int currentSkillCooldownCoroutine = SkillCooldown;
        ServiceRegistry.WorkWithService<EventBus>().Publish<string,AbstractSquad,bool>("ChangeState_BlockSkill",this,true);

        void oneSecondPassed()
        {
            currentSkillCooldownCoroutine--;

            ServiceRegistry.WorkWithService<EventBus>().Publish<AbstractSquad, int, int>(this, currentSkillCooldownCoroutine, SkillCooldown);

            if (currentSkillCooldownCoroutine <= 0)
            {
                IsSkillInCooldown = false;
                ServiceRegistry.WorkWithController<GameController>().oneSecondPassed -= oneSecondPassed;

                Debug.Log($"Cooldown is over:{Name}");
            }
        }

        ServiceRegistry.WorkWithController<GameController>().oneSecondPassed += oneSecondPassed;

        while (currentSkillCooldownCoroutine > 0)
        {
            yield return null;
        }

        ServiceRegistry.WorkWithController<GameController>().oneSecondPassed -= oneSecondPassed;

        ServiceRegistry.WorkWithService<EventBus>().Publish<string, AbstractSquad, bool>("ChangeState_BlockSkill", this, false);
        ServiceRegistry.WorkWithService<EventBus>().Publish<AbstractSquad, int, int>(this, 0, SkillCooldown);
    }

    public abstract Action UseClassSkill();
    public bool SkillWithChoise { get; set; } = false;
}

[Squad("InfantrySquad")]
public class InfantrySquad : AbstractSquad
{
    protected int _skillScale;

    public override object Clone() { return new InfantrySquad(); }
    public override void Initialize(float speedOfMovement, int countOfPeople, SquadTransport typeTransport, SquadWeapon typeWeapon) 
    {
        TrainingTime = 1;

        PeopleCount = countOfPeople;
        Speed = TypesConverter.ConvertTransportTypeSpeed(typeTransport, speedOfMovement);

        Weapon = typeWeapon;
        Transport = typeTransport;

        CalculateParam();

        Name = "Пехота";
        SkillName = "Боевой крик";

        _skillScale = 10;

        SkillCooldown = 40;

        BuildingSkill = 1;

        Type = SquadEnum.Infantry;
    }
    public override void CalculateParam()
    {
        Attack = PeopleCount * 3;
        Protection = PeopleCount * 1;
    }
    public override Action UseClassSkill()
    {
        if (IsSkillInCooldown) {return null;}
        IsSkillInCooldown = true;

        InfantrySkill infantrySkillBuff = new InfantrySkill(20, TimeControllerScript.GetCurrentDateTime(),5);

        ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(base.SkillCooldownCoroutine());

        return ()=> { 
            base.UseSkill(20, SkillType.Buff, null,infantrySkillBuff); 
            ServiceRegistry.WorkWithController<BuffsController>().AddBuffToList(infantrySkillBuff);
            AddBuffWithTimer(infantrySkillBuff);
        };
    }
}

[Squad("EngineersSquad")]
public class EngineerSquad : AbstractSquad
{
    public override object Clone() { return new EngineerSquad(); }
    public override void Initialize(float speedOfMovement, int countOfPeople, SquadTransport typeTransport, SquadWeapon typeWeapon)
    {
        TrainingTime = 3;

        PeopleCount = countOfPeople;

        Speed = TypesConverter.ConvertTransportTypeSpeed(typeTransport, speedOfMovement);
        Weapon = typeWeapon;
        Transport = typeTransport;

        CalculateParam();

        Name = "Инженеры";

        SkillCooldown = 120;

        BuildingSkill = 3;

        Type = SquadEnum.Engineer;
    }
    public override void CalculateParam()
    {
        Attack = PeopleCount * 2;
        Protection = PeopleCount * 3;
    }
    public override Action UseClassSkill()
    {
        if (IsSkillInCooldown) { return null; }

        if (ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(
            ServiceRegistry.WorkWithController<GameController>().GetCellWithThisSquad(this)).GetParameter<CellBuildings>().CheckBuildIsBuilt("FoxholeBuild")) { return null; }
        IsSkillInCooldown = true;

        ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(base.SkillCooldownCoroutine());
        return () => { base.UseSkill(0, SkillType.Action, () => EngeenerSkill(
            ServiceRegistry.WorkWithController<GameController>().GetCellWithThisSquad(this)), null); }; 
        
    }
    private void EngeenerSkill(GameObject currentCell)
    {
        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(currentCell).GetParameter<CellBuildings>().AddToBuildsList(new FoxholeBuild(), "FoxholeBuild");
    }
}

[Squad("ScoutsSquad")]
public class ScoutSquad : AbstractSquad
{
    protected GameObject _selectedCell;
    public override object Clone() {

        ScoutSquad newSquad = new ScoutSquad();

        newSquad.SkillWithChoise = true;

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<SkillController, AbstractSquad, GameObject>((sender, squad, selectedCell) =>
        {
            if (squad == newSquad) { newSquad._selectedCell = selectedCell; }
        });

        return newSquad; }
    public override void Initialize(float speedOfMovement, int countOfPeople, SquadTransport typeTransport, SquadWeapon typeWeapon)
    {
        TrainingTime = 4;

        PeopleCount = countOfPeople;

        Speed = TypesConverter.ConvertTransportTypeSpeed(typeTransport, speedOfMovement);
        Weapon = typeWeapon;
        Transport = typeTransport;

        CalculateParam();

        Name = "Разведчики";

        SkillCooldown = 30;

        BuildingSkill = 2;

        Type = SquadEnum.Scouts;
    }
    public override void CalculateParam()
    {
        Attack = PeopleCount * 2;
        Protection = PeopleCount * 2;
    }
    public override Action UseClassSkill()
    {
        if (IsSkillInCooldown) { return null; }

        if (!ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(_selectedCell))
        {
            IsSkillInCooldown = true;

            ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(base.SkillCooldownCoroutine());

            return () => { base.UseSkill(0, SkillType.Action, () => ServiceRegistry.WorkWithController<ExplorationController>().StartExploration(
                ServiceRegistry.WorkWithController<GameController>().GetCellWithThisSquad(this),_selectedCell, this), null); };
        }
        return null;
    }
}

[Squad("ArtillerySquad")]
public class ArtillerySquad : AbstractSquad
{
    private int gunsCount;

    protected GameObject _selectedCell;

    public override object Clone()
    {
        ArtillerySquad newSquad = new ArtillerySquad();

        newSquad.SkillWithChoise = true;

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<SkillController, AbstractSquad, GameObject>((sender, squad, selectedCell) =>
        {
            if (squad == newSquad) { newSquad._selectedCell = selectedCell; }
        });

        return newSquad;
    }
    public override void Initialize(float speedOfMovement, int countOfPeople, SquadTransport typeTransport, SquadWeapon typeWeapon)
    {
        TrainingTime = 20;

        PeopleCount = countOfPeople;

        gunsCount = countOfPeople / 5;

        Speed = TypesConverter.ConvertTransportTypeSpeed(typeTransport, speedOfMovement);
        Weapon = typeWeapon;
        Transport = typeTransport;

        CalculateParam();

        Name = "Артиллеристы";

        SkillCooldown = 120;

        BuildingSkill = 0;

        Type = SquadEnum.Artillery;
    }
    public override void CalculateParam()
    {
        Attack = 0;
        Protection = gunsCount*50;

        Attack = TypesConverter.ConvertWeaponType(Weapon, Attack);
        Protection = TypesConverter.ConvertTransportTypeProtection(Transport, Protection);
    }
    public override Action UseClassSkill()
    {
        if (!ServiceRegistry.WorkWithController<ResourcesController>().CheckReourcesToSkill(ResourcesController.SkillType_ForCost.Artillary)) return null;
        if (IsSkillInCooldown) { return null; }

        if (Vector3.Distance(
            ServiceRegistry.WorkWithController<GameController>().GetCellWithThisSquad(this).transform.position, _selectedCell.transform.position) <= 700)
        {
            IsSkillInCooldown = true;

            ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(base.SkillCooldownCoroutine());

            return base.UseSkill(2, SkillType.Action, () => ClearSomeSquads(_selectedCell), null);
        }
        else { return null; } 
    }
    private void ClearSomeSquads(GameObject cell)
    {
        var squads = ServiceRegistry.WorkWithController<GameController>().GetAllSquadsOnCell(cell);

        List<AbstractSquad> squadsToDelete = new List<AbstractSquad>();

        foreach (var squad in squads)
        {
            var count = squad.PeopleCount;

            if(count <= 5) { squadsToDelete.Add(squad);continue; }

            int peopleToKill = (int)(count * (gunsCount/100f)) + gunsCount;

            squad.KillSomePerson(peopleToKill);
        }

        foreach(var squad in squadsToDelete)
        {
            ServiceRegistry.WorkWithController<GameController>().SingleThrasher_Squad(squad);
        }

        _selectedCell = null;
    }
    public override void KillSomePerson(int count)
    {
        gunsCount = (PeopleCount - count) / 5;

        CalculateParam();
    }
}

[Squad("TanksSquad")]
public class TankSquad : AbstractSquad
{
    private int tanksCount;

    private int minimalCountOfCrew = 3;

    public override object Clone() { return new TankSquad(); }
    public override void Initialize(float speedOfMovement, int countOfPeople, SquadTransport typeTransport, SquadWeapon typeWeapon)
    {
        TrainingTime = 20;

        PeopleCount = countOfPeople;

        tanksCount = countOfPeople / 4;

        Speed = TypesConverter.ConvertTransportTypeSpeed(typeTransport, speedOfMovement);
        Weapon = typeWeapon;
        Transport = typeTransport;

        CalculateParam();

        Name = "Танкисты";

        SkillCooldown = 60;

        BuildingSkill = 0;

        Type = SquadEnum.Tanks;
    }
    public override void CalculateParam()
    {
        Attack = tanksCount*30;
        Protection = tanksCount*30;

        Attack = TypesConverter.ConvertWeaponType(Weapon,Attack);
        Protection = TypesConverter.ConvertTransportTypeProtection(Transport,Protection);
    }
    public override Action UseClassSkill()
    {
        if (IsSkillInCooldown) { return null; }
        IsSkillInCooldown = false;

        ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(base.SkillCooldownCoroutine());

        return ()=> { BuffAllSquadsInCell(ServiceRegistry.WorkWithController<GameController>().GetCellWithThisSquad(this)); };
    }
    private void BuffAllSquadsInCell(GameObject currentCell)
    {
        TanksSkill newBuff = new TanksSkill(tanksCount);

        foreach (var squad in ServiceRegistry.WorkWithController<GameController>().GetAllSquadsOnCell(currentCell))
        {
            squad.AddBuffWithTimer(newBuff);
            ServiceRegistry.WorkWithController<BuffsController>().AddBuffToList(newBuff);
        }
    }
    public override void KillSomePerson(int count)
    {
        tanksCount = 0;

        var currentCountCrew = PeopleCount - count;
        while (true)
        {
            if(currentCountCrew == minimalCountOfCrew) { tanksCount++; break; }
            if(currentCountCrew < minimalCountOfCrew) { break; }

            currentCountCrew -= 5;
            tanksCount++;
        }
        PeopleCount -= count;

        CalculateParam();
    }
}