using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class CreatingPanelScript : MonoBehaviour
{
    protected ResourcesController _resourcesController;

    [SerializeField] private GameObject blockPanel;

    [SerializeField] private SquadsPresetPanelScript presetsScript;
    [SerializeField] private TrainingPanelScript trainingScript;


    [SerializeField] private TMP_Dropdown dropDown_SquadType;
    [SerializeField] private TMP_Dropdown dropDown_WeaponType;
    [SerializeField] private TMP_Dropdown dropDown_TransportType;
    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private TextMeshProUGUI peopleCost;
    [SerializeField] private TextMeshProUGUI transportCost;
    [SerializeField] private TextMeshProUGUI weaponCost;


    private Dictionary<SquadWeapon, string> weponDictionary_temporarily = new()
    {
        {SquadWeapon.Tank,"Танк" },
        {SquadWeapon.MachineHun,"Пистолет-пулимёт" },
        {SquadWeapon.Artillery,"Артиллерия" },
        {SquadWeapon.SniperRifle,"Снайперская винтовка" },
        {SquadWeapon.AssautRifle,"Автомат" },
        {SquadWeapon.Rifle,"Винтовка" }
    };
    private Dictionary<SquadTransport, string> transportDictionary_temporarily = new()
    {
        {SquadTransport.Cars,"Машина" },
        {SquadTransport.Artillery,"Артиллерия" },
        {SquadTransport.ByFoot,"Без транспорта" },
        {SquadTransport.Horses,"Лошади" },
        {SquadTransport.Tank,"Танк" }
    };

    private SquadData currentSquadData;

    private SquadTransport[] currentTransportList;
    private SquadWeapon[] currentWeaponList;

    private SquadWeapon currentWeapon;
    private SquadTransport currentTransport;
    private int currentPeopleCount = 0;

    protected GameObject _currentWorkingCell;

    private void Start()
    {
        _resourcesController = ServiceRegistry.WorkWithController<ResourcesController>();

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<TrainingPanelScript, CreatingPanelScript, bool>((sender, key, state) =>
        {
            gameObject.SetActive(state);
        });

        gameObject.SetActive(false);
    }

    public void SetCurrentWorkingCell(GameObject currentWorkingCell) { _currentWorkingCell = currentWorkingCell; }

    public void OnValueChanged(int index)
    {
        dropDown_WeaponType.interactable = false;
        dropDown_TransportType.interactable = false;
        inputField.interactable = false;

        dropDown_WeaponType.ClearOptions();
        dropDown_TransportType.ClearOptions();

        dropDown_WeaponType.value = -1;
        dropDown_TransportType.value = -1;

        inputField.text = "";
        peopleCost.text = "0";
        weaponCost.text = "0";
        transportCost.text = "0";


        currentWeapon = SquadWeapon.None;
        currentTransport = SquadTransport.None;
        currentPeopleCount = 0;

        ShowCreatingMode(ServiceRegistry.WorkWithService<PanelFactory_SquadPanel>().OptionToId[dropDown_SquadType.options[index]]);
    }

    private void ShowCreatingMode(string id)
    {
        dropDown_WeaponType.interactable = true;
        dropDown_TransportType.interactable = true;
        inputField.interactable = true;

        blockPanel.SetActive(false);

        currentSquadData = (SquadData)ServiceRegistry.WorkWithService<DataHolder>().GetDataFromHolder(id);

        if (currentSquadData.needAcademy)
        {
            if (!ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(_currentWorkingCell).GetParameter<CellBuildings>().CheckBuildIsBuilt("AcademyBuild"))
            {
                blockPanel.SetActive(true);
            }
        }

        currentWeaponList = currentSquadData.weapons.ToArray();
        currentTransportList = currentSquadData.transports.ToArray();

        foreach(SquadTransport transport in currentTransportList)
        {
            dropDown_TransportType.options.Add(new TMP_Dropdown.OptionData(transportDictionary_temporarily[transport]));
        }
        foreach(SquadWeapon weapon in currentWeaponList)
        {
            dropDown_WeaponType.options.Add(new TMP_Dropdown.OptionData(weponDictionary_temporarily[weapon]));
        }
    }

    public void OnValueChanged_WeaponDropDown(int index){ currentWeapon = currentWeaponList[index]; CalculateTotalCost(); }
    public void OnValueChanged_TransportDropDown(int index) { currentTransport = currentTransportList[index]; CalculateTotalCost(); }
    public void OnValueChanged_PeopleCount(string input)
    { 
        if(int.TryParse(input, out currentPeopleCount)) { }
        else { currentPeopleCount = 0; }
            CalculateTotalCost(); }

    private void CalculateTotalCost()
    {
        peopleCost.text = $"{currentSquadData.squadCostPeople * currentPeopleCount}";
        PlayerHits_ColorSwipe(currentSquadData.squadCostPeople * currentPeopleCount, ResourcesEnum.People, peopleCost);

        if (currentWeapon == SquadWeapon.None) { weaponCost.text = "0";
            PlayerHits_ColorSwipe(0, ResourcesEnum.Transport, weaponCost);
        }
        else { weaponCost.text = $"{TypesConverter.WeaponCost[currentWeapon] * currentPeopleCount}";
            PlayerHits_ColorSwipe(TypesConverter.WeaponCost[currentWeapon] * currentPeopleCount, ResourcesEnum.Transport, weaponCost);
        }

        if (currentTransport == SquadTransport.None) { transportCost.text = "0";
            PlayerHits_ColorSwipe(0, ResourcesEnum.Transport, transportCost);
        }
        else { transportCost.text = $"{TypesConverter.TransportCost[currentTransport] * currentPeopleCount}";
            PlayerHits_ColorSwipe(TypesConverter.TransportCost[currentTransport] * currentPeopleCount, ResourcesEnum.Transport, transportCost);
        }

    }

    private void PlayerHits_ColorSwipe(int count,ResourcesEnum resourcesType, TextMeshProUGUI textMesh)
    {
        if (CheckCountOfResource(count,resourcesType)) textMesh.color = Color.red;
        else textMesh.color = Color.green;
    }
    private bool CheckCountOfResource(int count, ResourcesEnum resourcesType)
    {
        return _resourcesController.GetCountOfResourceByType(resourcesType) < count;
    }
    public void AddToTrainSquad()
    {
        if(currentWeapon == SquadWeapon.None || currentTransport == SquadTransport.None || currentPeopleCount == 0)return;

        AbstractSquad newSquad = (AbstractSquad)ServiceRegistry.WorkWithService<ObjectFactory_Squads>().CreateObject(currentSquadData.id);

        newSquad.Initialize(currentSquadData.squadSpeedOfMovement,currentPeopleCount,currentTransport,currentWeapon);

        SquadPresetObject squadPresetObject = new SquadPresetObject(
            currentWeapon,
            currentTransport,
            currentPeopleCount,
            new Dictionary<ResourcesEnum, int>() 
            {
                {ResourcesEnum.People, currentSquadData.squadCostPeople * currentPeopleCount },
                {ResourcesEnum.Weapon, TypesConverter.WeaponCost[currentWeapon] * currentPeopleCount },
                {ResourcesEnum.Transport, TypesConverter.TransportCost[currentTransport] * currentPeopleCount }
            });

        newSquad.Side = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(_currentWorkingCell).GetParameter<CellArea>().Side;

        presetsScript.AddToPresets_Latest(newSquad,squadPresetObject);

        trainingScript.CreateSquadTrainingPanel(newSquad);

        gameObject.SetActive(false);
    }
}

public class SquadPresetObject
{
    public SquadWeapon squadWeapon { get; private set; }
    public SquadTransport squadTransport { get; private set; }

    public int countOfPeople { get; private set; }

    public SquadPresetObject(SquadWeapon squadWeapon, SquadTransport squadTransport, int countOfPeople, Dictionary<ResourcesEnum, int> resourceCount)
    {
        this.squadWeapon = squadWeapon;
        this.squadTransport = squadTransport;
        this.countOfPeople = countOfPeople;
        this.resourceCount = resourceCount;
    }

    private Dictionary<ResourcesEnum, int> resourceCount = new Dictionary<ResourcesEnum, int>()
    {
        {ResourcesEnum.People, 0 },
        {ResourcesEnum.Weapon, 0 },
        {ResourcesEnum.Transport, 0 }
    };
    public int GetResourceCost_ByType(ResourcesEnum resourcesEnum) { return resourceCount[resourcesEnum]; }

}
