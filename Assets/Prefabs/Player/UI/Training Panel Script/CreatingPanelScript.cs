using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreatingPanelScript : MonoBehaviour
{
    [Header("InPut пехота")]
    [SerializeField] private GameObject infantryInputObject;
    protected TMP_InputField _infantryInput;

    [Header("InPut инженеры")]
    [SerializeField] private GameObject engeenerInputObject;
    protected TMP_InputField _engeenerInput;

    [Header("InPut разведчики")]
    [SerializeField] private GameObject scoutsInputObject;
    protected TMP_InputField _scoutsInput;

    [Header("Панель артиллерии")]
    [SerializeField] private GameObject artillerySquadPanel;

    [Header("Панель танкистов")]
    [SerializeField] private GameObject tankSquadPanel;

    [SerializeField] private GameObject trainingPanel;
    protected TrainingPanelScript _trainingPanelScriptScript;

    private AlliesSpawner alliesSpawnerScript;

    [Header("Список созданных")]
    [SerializeField] private SquadsPresetPanelScript squadsPresetPanelScript;

    [Header("Сообщение для игрока")]
    [SerializeField] private GameObject messageTaker;
    private MessageScript messageScript;

    protected int _infantrySquadWeaponType = 0;
    protected int _infantrySquadTransportType = 0;

    protected int _engeenerSquadWeaponType = 0;
    protected int _engeenerSquadTransportType = 0;

    protected int _scoutsSquadWeaponType = 0;
    protected int _scoutsSquadTransportType = 0;

    public void AddSquadToTraining_FromButtons(int index)
    {
        AddSquadToTraining((SquadEnum)index + 1);
    }
    public void AddSquadToTraining(SquadEnum squadType)
    {
        if(_infantryInput == null)
        {
            messageTaker.TryGetComponent(out messageScript);

            infantryInputObject.TryGetComponent(out _infantryInput);
            engeenerInputObject.TryGetComponent(out _engeenerInput);
            scoutsInputObject.TryGetComponent(out _scoutsInput);
        }
        if(squadType == SquadEnum.Infantry)
        {
            var count = int.Parse(_infantryInput.text);
            _infantryInput.text = "";

            if (!ChechCount(count)) { return; }

            SquadPresetObject squadPreset = new SquadPresetObject();

            SquadTransport selectedTransport = squadPreset.ConvertSquadTransport(SquadEnum.Infantry,_infantrySquadTransportType);
            SquadWeapon selectedWeapon = squadPreset.ConvertSquadWeapon(SquadEnum.Infantry, _infantrySquadWeaponType);

            squadPreset.SetModeOfTransport(selectedTransport);squadPreset.SetAttackWeapon(selectedWeapon);

            int costTransport = TypesConverter.TransportCost[selectedTransport] * count, costWeapon = TypesConverter.WeaponCost[selectedWeapon] * count;

            if (!CheckResourceCount(costTransport, ResourcesEnum.Transport)) { return; }

            if (!CheckResourceCount(costWeapon, ResourcesEnum.Weapon)) { return; }

            squadPreset.SetCountOfPeople(count);

            alliesSpawnerScript.RemovePeople(count);
            alliesSpawnerScript.RemoveResource(costTransport, ResourcesEnum.Transport);
            alliesSpawnerScript.RemoveResource(costWeapon, ResourcesEnum.Weapon);

            squadPreset.SetCost(count, costTransport, costWeapon);

            AbstractSquad squad = new InfantrySquad(3, count, selectedTransport, selectedWeapon);
            squad.Side = SideEnum.Allies;
            _trainingPanelScriptScript.CreateSquadTrainingPanel(squad);
            AddToLatestPresets(squad, squadPreset);

            gameObject.SetActive(false);
        }
        if(squadType == SquadEnum.Engeener)
        {
            var count = int.Parse(_engeenerInput.text);
            _engeenerInput.text = "";

            if (!ChechCount(count * 2)) { return; }

            SquadPresetObject squadPreset = new SquadPresetObject();

            SquadTransport selectedTransport = squadPreset.ConvertSquadTransport(SquadEnum.Engeener, _engeenerSquadTransportType);
            SquadWeapon selectedWeapon = squadPreset.ConvertSquadWeapon(SquadEnum.Engeener, _engeenerSquadWeaponType);

            squadPreset.SetModeOfTransport(selectedTransport); squadPreset.SetAttackWeapon(selectedWeapon);

            int costTransport = TypesConverter.TransportCost[selectedTransport] * count, costWeapon = TypesConverter.WeaponCost[selectedWeapon] * count;

            if (!CheckResourceCount(costTransport, ResourcesEnum.Transport)) { return; }

            if (!CheckResourceCount(costWeapon, ResourcesEnum.Weapon)) { return; }

            squadPreset.SetCountOfPeople(count);

            alliesSpawnerScript.RemovePeople(count * 2);
            alliesSpawnerScript.RemoveResource(costTransport, ResourcesEnum.Transport);
            alliesSpawnerScript.RemoveResource(costWeapon, ResourcesEnum.Weapon);

            squadPreset.SetCost(count, costTransport, costWeapon);

            AbstractSquad squad = new EngineerSquad(3, count, selectedTransport, selectedWeapon);
            squad.Side = SideEnum.Allies;
            _trainingPanelScriptScript.CreateSquadTrainingPanel(squad);
            AddToLatestPresets(squad, squadPreset);

            gameObject.SetActive(false);
        }
        if (squadType == SquadEnum.Scouts)
        {
            var count = int.Parse(_scoutsInput.text);
            _scoutsInput.text = "";

            if(count > 5) { messageTaker.SetActive(true); messageScript.SendMessage(16);return; }

            if (!ChechCount(count * 2)) { return; }

            SquadPresetObject squadPreset = new SquadPresetObject();

            SquadTransport selectedTransport = squadPreset.ConvertSquadTransport(SquadEnum.Scouts, _engeenerSquadTransportType);
            SquadWeapon selectedWeapon = squadPreset.ConvertSquadWeapon(SquadEnum.Scouts, _engeenerSquadWeaponType);

            squadPreset.SetModeOfTransport(selectedTransport); squadPreset.SetAttackWeapon(selectedWeapon);

            int costTransport = TypesConverter.TransportCost[selectedTransport] * count, costWeapon = TypesConverter.WeaponCost[selectedWeapon] * count;

            if (!CheckResourceCount(costTransport, ResourcesEnum.Transport)) { return; }

            if (!CheckResourceCount(costWeapon, ResourcesEnum.Weapon)) { return; }

            squadPreset.SetCountOfPeople(count);

            alliesSpawnerScript.RemovePeople(count * 2);
            alliesSpawnerScript.RemoveResource(costTransport, ResourcesEnum.Transport);
            alliesSpawnerScript.RemoveResource(costWeapon, ResourcesEnum.Weapon);

            squadPreset.SetCost(count, costTransport, costWeapon);

            AbstractSquad squad = new ScoutSquad(3, count, selectedTransport, selectedWeapon);
            squad.Side = SideEnum.Allies;
            _trainingPanelScriptScript.CreateSquadTrainingPanel(squad);

            gameObject.SetActive(false);
        }
        if(squadType == SquadEnum.Artillery)
        {
            if (!ChechCount(3*5)) { return; }

            SquadPresetObject squadPreset = new SquadPresetObject();

            SquadTransport selectedTransport = SquadTransport.Artillery;
            SquadWeapon selectedWeapon = SquadWeapon.Artillery;

            squadPreset.SetModeOfTransport(selectedTransport); squadPreset.SetAttackWeapon(selectedWeapon);

            int costTransport = TypesConverter.TransportCost[selectedTransport] * 3, costWeapon = TypesConverter.WeaponCost[selectedWeapon] * 3;

            if (!CheckResourceCount(costTransport, ResourcesEnum.Transport)) { return; }

            if (!CheckResourceCount(costWeapon, ResourcesEnum.Weapon)) { return; }

            squadPreset.SetCountOfPeople(3*5);

            alliesSpawnerScript.RemovePeople(3*5);
            alliesSpawnerScript.RemoveResource(TypesConverter.TransportCost[selectedTransport] * 3, ResourcesEnum.Transport);
            alliesSpawnerScript.RemoveResource(TypesConverter.WeaponCost[selectedWeapon] * 3, ResourcesEnum.Weapon);

            squadPreset.SetCost(3*5, costTransport, costWeapon);

            AbstractSquad squad = new ArtillerySquad(2, 15, SquadTransport.Artillery, SquadWeapon.Artillery);
            squad.Side = SideEnum.Allies;
            _trainingPanelScriptScript.CreateSquadTrainingPanel(squad);
            AddToLatestPresets(squad, squadPreset);

            gameObject.SetActive(false);
        }
        if(squadType == SquadEnum.Tanks)
        {
            if (!ChechCount(5 * 4)) { return; }

            SquadPresetObject squadPreset = new SquadPresetObject();

            SquadTransport selectedTransport = SquadTransport.Tank;
            SquadWeapon selectedWeapon = SquadWeapon.Tank;

            squadPreset.SetModeOfTransport(selectedTransport); squadPreset.SetAttackWeapon(selectedWeapon);

            int costTransport = TypesConverter.TransportCost[selectedTransport] * 5, costWeapon = TypesConverter.WeaponCost[selectedWeapon] * 5;

            if (!CheckResourceCount(costTransport, ResourcesEnum.Transport)) { return; }

            if (!CheckResourceCount(costWeapon, ResourcesEnum.Weapon)) { return; }

            squadPreset.SetCountOfPeople(5*4);

            alliesSpawnerScript.RemovePeople(5 * 4);
            alliesSpawnerScript.RemoveResource(TypesConverter.TransportCost[selectedTransport] * 5, ResourcesEnum.Transport);
            alliesSpawnerScript.RemoveResource(TypesConverter.WeaponCost[selectedWeapon] * 5, ResourcesEnum.Weapon);

            squadPreset.SetCost(5*4, costTransport, costWeapon);

            TankSquad squad = new TankSquad(4, 20, SquadTransport.Tank, SquadWeapon.Tank);
            squad.Side = SideEnum.Allies;
            _trainingPanelScriptScript.CreateSquadTrainingPanel(squad);
            AddToLatestPresets(squad,squadPreset);

            gameObject.SetActive(false);
        }
    }
    public void AddSquadToTrainingWithPreset(SquadPresetObject squadPreset, SquadEnum squadType)
    {
        if (!ChechCount(squadPreset.GetResourceCost_ByType(ResourcesEnum.People))) return;

        if (!CheckResourceCount(squadPreset.GetResourceCost_ByType(ResourcesEnum.Transport), ResourcesEnum.Transport)) { return; }

        if (!CheckResourceCount(squadPreset.GetResourceCost_ByType(ResourcesEnum.Weapon), ResourcesEnum.Weapon)) { return; }

        alliesSpawnerScript.RemovePeople(squadPreset.GetResourceCost_ByType(ResourcesEnum.People));
        alliesSpawnerScript.RemoveResource(squadPreset.GetResourceCost_ByType(ResourcesEnum.Transport), ResourcesEnum.Transport);
        alliesSpawnerScript.RemoveResource(squadPreset.GetResourceCost_ByType(ResourcesEnum.Weapon), ResourcesEnum.Weapon);

        AbstractSquad squad;
        switch (squadType)
        {
            case SquadEnum.Infantry:
                squad = new InfantrySquad(3,squadPreset.GetCountOfPeople(),squadPreset.GetModeOfTransport(),squadPreset.GetAttackWeapon());break;
            case SquadEnum.Engeener:
                squad = new EngineerSquad(3, squadPreset.GetCountOfPeople(), squadPreset.GetModeOfTransport(), squadPreset.GetAttackWeapon()); break;
            case SquadEnum.Scouts:
                squad = new ScoutSquad(3, squadPreset.GetCountOfPeople(), squadPreset.GetModeOfTransport(), squadPreset.GetAttackWeapon()); break;
            case SquadEnum.Artillery:
                squad = new EngineerSquad(2, squadPreset.GetCountOfPeople(), squadPreset.GetModeOfTransport(), squadPreset.GetAttackWeapon()); break;
            case SquadEnum.Tanks:
                squad = new EngineerSquad(4, squadPreset.GetCountOfPeople(), squadPreset.GetModeOfTransport(), squadPreset.GetAttackWeapon()); break;
            default:
                throw new ArgumentOutOfRangeException(nameof(squadPreset));
        }

        squad.Side = SideEnum.Allies;
        _trainingPanelScriptScript.CreateSquadTrainingPanel(squad);
    }
    private void OnEnable()
    {
        if(_trainingPanelScriptScript == null) { trainingPanel.TryGetComponent(out _trainingPanelScriptScript); }
        if (_trainingPanelScriptScript.CheckHasBuild(BuildsEnum.MilitaryAcademy))
        {
            artillerySquadPanel.SetActive(true);
            tankSquadPanel.SetActive(true);
        }
    }
    private void OnDisable()
    {
        artillerySquadPanel.SetActive(false);
        tankSquadPanel.SetActive(false);
    }
    private bool ChechCount(int count)
    {
        if(alliesSpawnerScript.GetCountOfPeopleResource() >= count)
        {
            return true;
        }
        else if (!alliesSpawnerScript.CanAddNewSquadInTraining())
        {
            messageTaker.SetActive(true);
            messageScript.SendMessage(6);
            return false;
        }
        else {
            messageTaker.SetActive(true);
            messageScript.SendMessage(2);
            return false; 
        }
    }
    private bool CheckResourceCount(int count, ResourcesEnum resourceType)
    {
        if(alliesSpawnerScript.GetCountOfResource(resourceType)>= count)
        {
            return true;
        }
        else { messageTaker.SetActive(true);
               if(resourceType == ResourcesEnum.Weapon) { messageScript.SendMessage(7); }
               else { messageScript.SendMessage(8); }

               return false;
        }
    }
    public void SetAllGameObjects(GameObject cell)
    {
        cell.TryGetComponent(out alliesSpawnerScript);
    }

    private void AddToLatestPresets(AbstractSquad squad, SquadPresetObject squadPresetObject)
    {
        squadsPresetPanelScript.AddToPresets_Latest(squad, squadPresetObject);
    }

    /////////////////////////////
    public void InfantryTransportChangedDropDown(int value) { _infantrySquadTransportType = value;  }
    public void InfantryWeaponChangedDropDown(int value) { _infantrySquadWeaponType = value;  }

    public void EngeenerTransportChangedDropDown(int value) { _engeenerSquadTransportType = value;  }
    public void EngeenerWeaponChangedDropDown(int value) { _engeenerSquadWeaponType = value;  }

    public void ScoutsTransportChangedDropDown(int value) { _scoutsSquadTransportType = value; }
    public void ScoutsWeaponChangedDropDown(int value) { _scoutsSquadWeaponType = value; }
}

public class SquadPresetObject:IConverter
{
    private SquadWeapon squadWeapon;
    private SquadTransport squadTransport;

    private int countOfPeople;

    private Dictionary<ResourcesEnum, int> resourceCount = new Dictionary<ResourcesEnum, int>()
    {
        {ResourcesEnum.People, 0 },
        {ResourcesEnum.Weapon, 0 },
        {ResourcesEnum.Transport, 0 }
    };

    public void SetCost(int value_people, int value_transport,int value_weapon)
    {
        resourceCount[ResourcesEnum.People] = value_people;
        resourceCount[ResourcesEnum.Weapon] = value_weapon;
        resourceCount[ResourcesEnum.Transport] = value_transport;
    }
    public void SetCountOfPeople(int countOfPeople) { this.countOfPeople = countOfPeople; }
    public int GetCountOfPeople() { return countOfPeople; }

    public void SetAttackWeapon(SquadWeapon squadWeapon) { this.squadWeapon = squadWeapon; }
    public SquadWeapon GetAttackWeapon() { return squadWeapon; }

    public void SetModeOfTransport(SquadTransport squadTransport) { this.squadTransport = squadTransport; }
    public SquadTransport GetModeOfTransport() { return squadTransport; }


    public SquadWeapon ConvertSquadWeapon(SquadEnum squadType, int index)
    {
        return _squadAndThierWeapon[squadType][index];
    }
    public SquadTransport ConvertSquadTransport(SquadEnum squadType, int index)
    {
        return _squadAndThierTransport[squadType][index];
    }

    public int GetResourceCost_ByType(ResourcesEnum resourcesEnum) { return resourceCount[resourcesEnum]; }

    protected Dictionary<SquadEnum, List<SquadWeapon>> _squadAndThierWeapon = new Dictionary<SquadEnum, List<SquadWeapon>>() 
    {
        {SquadEnum.Infantry,new List<SquadWeapon>(){ SquadWeapon.MachineHun, SquadWeapon.Rifle, SquadWeapon.SniperRifle} },
        {SquadEnum.Engeener,new List<SquadWeapon>(){ SquadWeapon.MachineHun, SquadWeapon.Rifle, SquadWeapon.SniperRifle} },
        {SquadEnum.Scouts,new List<SquadWeapon>(){ SquadWeapon.AssautRifle, SquadWeapon.MachineHun} }
    };
    protected Dictionary<SquadEnum, List<SquadTransport>> _squadAndThierTransport = new Dictionary<SquadEnum, List<SquadTransport>>()
    {
        {SquadEnum.Infantry,new List<SquadTransport>(){ SquadTransport.ByFoot, SquadTransport.Horses, SquadTransport.Cars} },
        {SquadEnum.Engeener,new List<SquadTransport>(){ SquadTransport.ByFoot, SquadTransport.Horses, SquadTransport.Cars } },
        {SquadEnum.Scouts,new List<SquadTransport>(){ SquadTransport.ByFoot, SquadTransport.Horses} }
    };
}
public interface IConverter:IConverter_Weapon,IConverter_Transport
{

}
public interface IConverter_Weapon
{
    public SquadWeapon ConvertSquadWeapon(SquadEnum squadType, int index);
}
public interface IConverter_Transport
{
    public SquadTransport ConvertSquadTransport(SquadEnum squadType, int index);
}
