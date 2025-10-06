using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesController
{
    [SerializeField] private int influenceReward = 50;

    private ResourcesUIScript resourcesUIScript;

    protected InfluenceController influenceController = new InfluenceController();
    protected BuildingResource buildingResource = new BuildingResource(200);
    protected WeaponResource weaponResource = new WeaponResource(0);
    protected TransportResource transportResource = new TransportResource(0);
    protected PeopleResource peopleResource = new PeopleResource(0);

    public Action<int, ResourcesEnum> ChangedResourceCount;

    protected Dictionary<(BuildsEnum,BuildsLevel), int> _buildCost = new Dictionary<(BuildsEnum, BuildsLevel), int> 
    {
        {(BuildsEnum.HeadQuarters, BuildsLevel.First),150},
        {(BuildsEnum.HeadQuarters, BuildsLevel.Second),250},
        {(BuildsEnum.Camp, BuildsLevel.First), 150},//Лагерь(1 уровень)
        {(BuildsEnum.Camp, BuildsLevel.Second), 250},//Лагерь(2 уровень)
        {(BuildsEnum.Fort, BuildsLevel.First), 400},//Форт(1 уровень)
        {(BuildsEnum.Fort, BuildsLevel.Second),600},//Форт(2 уровень)
        {(BuildsEnum.MilitaryAcademy,BuildsLevel.First),500 },//Военная акаденимя(1 уровень)
        {(BuildsEnum.MilitaryAcademy,BuildsLevel.Second),750 },//Военная акаденимя(2 уровень)
    };

    public enum SkillType_ForCost
    {
        Artillary
    }
    protected Dictionary<SkillType_ForCost, int> _skillCost = new Dictionary<SkillType_ForCost, int> 
    {
        {SkillType_ForCost.Artillary, 50 }
    };

    public ResourcesController()
    {
        resourcesUIScript = GameObject.FindFirstObjectByType<ResourcesUIScript>().GetComponent<ResourcesUIScript>();
    }
    public void Start()
    {
        ChangedResourceCount.Invoke(influenceController.GetInfluenceCount(), ResourcesEnum.Influence);
        ChangedResourceCount.Invoke(weaponResource.GetCount(), ResourcesEnum.Weapon);
        ChangedResourceCount.Invoke(buildingResource.GetCount(), ResourcesEnum.Building);
        ChangedResourceCount.Invoke(peopleResource.GetCount(), ResourcesEnum.People);
        ChangedResourceCount.Invoke(transportResource.GetCount(), ResourcesEnum.Transport);
    }

    private void NewControlArea()
    {
        influenceController.AddInfluenceResource(influenceReward);
        ChangedResourceCount.Invoke(influenceController.GetInfluenceCount(), ResourcesEnum.Influence);
    }
    private void EnemyCapturedAlliesCell()
    {
        influenceController.RemoveInfluenceResource(influenceReward);
        ChangedResourceCount.Invoke(influenceController.GetInfluenceCount(), ResourcesEnum.Influence);
    }
    public void WhichSideCapturedCell(SideEnum side)
    {
        if (side == SideEnum.Allies) { NewControlArea(); }
        else { EnemyCapturedAlliesCell(); }
    }
    public void RemoveSomeResourcesByType(ResourcesEnum type, int count)
    {
        if (type == ResourcesEnum.Weapon) { weaponResource.ChangeCount(-count); resourcesUIScript.ChangeResource(ResourcesEnum.Weapon,-count,()=>ChangedResourceCount.Invoke(weaponResource.GetCount(),type)); }
        if (type == ResourcesEnum.Building) {  buildingResource.ChangeCount(-count); resourcesUIScript.ChangeResource(ResourcesEnum.Building, -count, () => ChangedResourceCount.Invoke(buildingResource.GetCount(), type)); }
        if (type == ResourcesEnum.People) { peopleResource.ChangeCount(-count); resourcesUIScript.ChangeResource(ResourcesEnum.People, -count, () => ChangedResourceCount.Invoke(peopleResource.GetCount(), type)); }
        if (type == ResourcesEnum.Transport) { transportResource.ChangeCount(-count);resourcesUIScript.ChangeResource(ResourcesEnum.Transport, -count, () => ChangedResourceCount.Invoke(transportResource.GetCount(), type)); }
    }
    public void AddSomeResourcesByType(ResourcesEnum type, int count)
    {
        if (type == ResourcesEnum.Weapon) { weaponResource.ChangeCount(count); resourcesUIScript.ChangeResource(ResourcesEnum.Weapon, count, () => ChangedResourceCount.Invoke(weaponResource.GetCount(), type)); }
        if (type == ResourcesEnum.Building) { buildingResource.ChangeCount(count); resourcesUIScript.ChangeResource(ResourcesEnum.Building, count, () => ChangedResourceCount.Invoke(buildingResource.GetCount(), type)); }
        if (type == ResourcesEnum.People) { peopleResource.ChangeCount(count); resourcesUIScript.ChangeResource(ResourcesEnum.People, count, () => ChangedResourceCount.Invoke(peopleResource.GetCount(), type)); }
        if (type == ResourcesEnum.Transport) { transportResource.ChangeCount(count); resourcesUIScript.ChangeResource(ResourcesEnum.Transport, count, () => ChangedResourceCount.Invoke(transportResource.GetCount(), type)); }
        if (type == ResourcesEnum.Influence) { influenceController.AddInfluenceResource(count); resourcesUIScript.ChangeResource(ResourcesEnum.Influence, count, () => ChangedResourceCount.Invoke(influenceController.GetInfluenceCount(), type)); }
    }
    public bool CheckResourcesToBuild(BuildsEnum buildsType, BuildsLevel buildsLevel)
    {
        if(buildingResource.GetCount() >= _buildCost[(buildsType, buildsLevel)])
        {
            RemoveSomeResourcesByType(ResourcesEnum.Building, _buildCost[(buildsType, buildsLevel)]);
            return true;
        }
        else { return false; }
    }
    public bool CheckReourcesToSkill(SkillType_ForCost skillType)
    {
        if(weaponResource.GetCount() >= _skillCost[skillType])
        {
            RemoveSomeResourcesByType(ResourcesEnum.Weapon, _skillCost[skillType]);
            return true;
        }
        else { return false; }
    }
    public int GetCountOfResourceByType(ResourcesEnum type)
    {
        if(type == ResourcesEnum.Weapon) { return  weaponResource.GetCount(); }
        if(type == ResourcesEnum.Building) { return buildingResource.GetCount(); }
        if(type == ResourcesEnum.People) { return peopleResource.GetCount(); }
        if(type == ResourcesEnum.Transport) { return transportResource.GetCount(); }

        return 0;
    }

    public float GetInfluenceRation() { return influenceController.GetInfluenceCount() / 1000f; }

    public int GetBuildingCost(BuildsEnum buildType, BuildsLevel buildsLevel)
    {
        return _buildCost[(buildType, buildsLevel)];
    }
}
