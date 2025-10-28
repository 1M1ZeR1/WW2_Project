using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Data objects/Squad data")]
public class SquadData:ScriptableObject
{
    public Sprite squadSprite;

    public string id;
    public string squadName;

    [Header("Weapons and transports for this squad")]
    public List<SquadWeapon> weapons;
    public List<SquadTransport> transports;

    [Header("Squad cost of people(By 1 human)")]
    public int squadCostPeople;

    [Header("Squad need academy to train")]
    public bool needAcademy = false;

    [Header("Squad parameters")]

    [Header("Squad speed")] public int squadSpeedOfMovement;
}


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
    public SquadActions Action { get; set; } = SquadActions.None;

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

    public abstract void SetMasters( GameObject buffsControllerObject, GameObject explorationControllerObject);

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

        void oneSecondPassed()
        {
            currentSkillCooldownCoroutine--;

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
    }

    public abstract Action UseClassSkill(MonoBehaviour monoBehaviour, GameObject currentCell);
}

[Squad("InfantrySquad")]
public class InfantrySquad : AbstractSquad
{
    private BuffsController buffsControllerScript;

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

    public override void SetMasters(GameObject buffsControllerObject, GameObject explorationControllerObject)
    {
        if(buffsControllerScript!= null) { return; }
        buffsControllerObject.TryGetComponent(out buffsControllerScript);
    }
    public override Action UseClassSkill(MonoBehaviour userMonoBehaviour, GameObject currentCell)
    {
        if (IsSkillInCooldown) {return null;}
        IsSkillInCooldown = true;

        InfantrySkill infantrySkillBuff = new InfantrySkill(20, TimeControllerScript.GetCurrentDateTime(),5);

        userMonoBehaviour.StartCoroutine(base.SkillCooldownCoroutine());

        return ()=> { 
            base.UseSkill(20, SkillType.Buff, null,infantrySkillBuff); 
            buffsControllerScript.AddBuffToList(infantrySkillBuff);
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
    public override void SetMasters(GameObject buffsControllerObject, GameObject explorationControllerObject)
    {

    }
    public override Action UseClassSkill(MonoBehaviour userMonoBehaviour, GameObject currentCell)
    {
        if (IsSkillInCooldown) { return null; }

        if (currentCell.GetComponent<CellBuildings>().CheckBuildIsBuilt("FoxholeBuild")) { return null; }
        IsSkillInCooldown = true;

        userMonoBehaviour.StartCoroutine(base.SkillCooldownCoroutine());
        return () => { base.UseSkill(0, SkillType.Action, () => EngeenerSkill(currentCell), null); }; 
        
    }
    private void EngeenerSkill(GameObject currentCell)
    {
        currentCell.GetComponent<CellBuildings>().AddToBuildsList(new FoxholeBuild(), "FoxholeBuild");
    }
}

[Squad("ScoutsSquad")]
public class ScoutSquad : AbstractSquad
{
    protected ExplorationController explorationControllerScript;
    private GameObject selectedCell = null;

    public override object Clone() { return new ScoutSquad(); }
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
    public override void SetMasters(GameObject buffsControllerObject, GameObject explorationControllerObject)
    {
        if (explorationControllerScript != null) { return; }
        explorationControllerObject.TryGetComponent(out explorationControllerScript);
    }
    public void SetSelectedCell(GameObject cell) { selectedCell = cell; }
    public override Action UseClassSkill(MonoBehaviour userMonoBehaviour, GameObject currentCell)
    {
        if (IsSkillInCooldown) { return null; }

        if (!explorationControllerScript.CheckCellInExplorationing(selectedCell) && !ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(selectedCell))
        {
            IsSkillInCooldown = true;

            userMonoBehaviour.StartCoroutine(base.SkillCooldownCoroutine());

            return () => { base.UseSkill(0, SkillType.Action, () => explorationControllerScript.RequestToStartExploration(selectedCell, 20 * PeopleCount), null); };
        }
        return null;
    }
}

[Squad("ArtillerySquad")]
public class ArtillerySquad : AbstractSquad
{
    private int gunsCount;

    private GameObject selectedCell = null;

    public override object Clone() { return new ArtillerySquad(); }
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
    public override void SetMasters(GameObject buffsControllerObject, GameObject explorationControllerObject)
    {

    }
    public void SetSelectedCell(GameObject cell) { selectedCell = cell; }
    public override Action UseClassSkill(MonoBehaviour userMonoBehaviour, GameObject currentCell)
    {
        if (IsSkillInCooldown) { return null; }

        if (Vector3.Distance(currentCell.transform.position, selectedCell.transform.position) <= 700)
        {
            IsSkillInCooldown = true;

            userMonoBehaviour.StartCoroutine(base.SkillCooldownCoroutine());

            return base.UseSkill(2, SkillType.Action, () => ClearSomeSquads(selectedCell), null);
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

        selectedCell = null;
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
    private BuffsController buffsControllerScript;
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
    public override void SetMasters(GameObject buffsControllerObject, GameObject explorationControllerObject)
    {
        if(buffsControllerScript != null) { return; }
        buffsControllerObject.TryGetComponent(out buffsControllerScript);
    }
    public override void CalculateParam()
    {
        Attack = tanksCount*30;
        Protection = tanksCount*30;

        Attack = TypesConverter.ConvertWeaponType(Weapon,Attack);
        Protection = TypesConverter.ConvertTransportTypeProtection(Transport,Protection);
    }
    public override Action UseClassSkill(MonoBehaviour userMonoBehaviour, GameObject currentCell)
    {
        if (IsSkillInCooldown) { return null; }
        IsSkillInCooldown = false;

        userMonoBehaviour.StartCoroutine(base.SkillCooldownCoroutine());

        return ()=> { BuffAllSquadsInCell(currentCell); };
    }
    private void BuffAllSquadsInCell(GameObject currentCell)
    {
        TanksSkill newBuff = new TanksSkill(tanksCount);

        foreach (var squad in ServiceRegistry.WorkWithController<GameController>().GetAllSquadsOnCell(currentCell))
        {
            squad.AddBuffWithTimer(newBuff);
            buffsControllerScript.AddBuffToList(newBuff);
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