using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class)]
public class BuildingAttribute : Attribute
{
    public string Id { get;private set; }
    public BuildingAttribute(string id)=>Id = id;
}

public abstract class AbstractBuildings:IName,IClone
{
    public string Name { get; set; } = "null";
    public AbstractBuffs Buff { get; set; }
    public int LevelBuild { get; set; }
    public int MaxLevelBuild { get; set; }
    public int TimeBuild { get; set; }
    public int TimeUpgrade { get; set; }

    public abstract void InstanceBuild(int timeToBuild, int timeToUpgrade, int maxLevelOfConstruction);
    public abstract object Clone();
    public abstract void UpgradeBuild();

    public abstract void ActivateBuild();

    public bool IsUpgraded() { if(LevelBuild > 1) { return true; } return false; }
}

[Building("HeadquartersBuild")]
[Serializable]
public class HeadquartersBuild : AbstractBuildings
{
    public List<GameObject> hardCells { get; private set; } = new();
    public int MaxCountOfHardCells { get; private set; } = 2;

    private List<GameObject> cellsInArea;
    public List<GameObject> CellsInArea 
    {
        get { 
            if(cellsInArea == null) { cellsInArea = new(); return cellsInArea; }
            if(cellsInArea.Count == 0)
            {
                Debug.LogError($"Data is incorrect. Count of cells is 0.");
            }
            return cellsInArea; }
        set { cellsInArea = value; }
    }

    protected int _searchingRadius = 200;
    public Transform CellWithThisBuild { get; set; }

    private int headquartersBonus = 3;

    public override void InstanceBuild(int timeToBuild, int timeToUpgrade, int maxLevelOfConstruction)
    {
        TimeBuild = timeToBuild;
        TimeUpgrade = timeToUpgrade;

        MaxLevelBuild = maxLevelOfConstruction;

        Buff = null;

        Name = "Штаб";
    }
    public override object Clone()
    {
        var newClone = new HeadquartersBuild();
        newClone.InstanceBuild(TimeBuild, TimeUpgrade, MaxLevelBuild);
        return newClone;
    }
    public override void ActivateBuild()
    {
        SearchingCells();
    }
    public override void UpgradeBuild()
    {
        _searchingRadius += 300;

        headquartersBonus += 2;

        MaxCountOfHardCells += 1;

        SearchingCells();
    }

    public int GetCountOfHardCells() { return hardCells.Count; }
    public bool TryAddToHardCells(GameObject cell)
    {
        if (hardCells.Contains(cell)) return false;

        if (!ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(cell)) return false;

        if(hardCells.Count == MaxCountOfHardCells)return false;
        hardCells.Add(cell);

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellSquadsOnArea>().bonusHarden = 2;
        return true;
    }
    public bool TryRemoveFromHardCells(GameObject cell)
    {
        if (!hardCells.Contains(cell)) return false;

        hardCells.Remove(cell);
        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellSquadsOnArea>().bonusHarden = 0;

        return true;
    }

    private void SearchingCells()
    {
        var findedCells = Physics.OverlapSphere(CellWithThisBuild.position, _searchingRadius).Select(colider => colider.gameObject).Where(cell => cell.CompareTag("Interactable Cell")).ToList();

        if(CellsInArea.Count == 0) { CellsInArea = findedCells.ToList();
            foreach (var cell in CellsInArea) { AddBonusToCell(cell); }
            return; 
        }

        foreach(var cell in findedCells)
        {
            if (CellsInArea.Contains(cell)) continue;

            CellsInArea.Add(cell);

            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().cellChangedSide += CellInListChangedSide;

            AddBonusToCell(cell);
        }
    }
    private void CellInListChangedSide(GameObject cell)
    {
        if (!ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(cell))
        {
            hardCells.Remove(cell);
        }
    }

    //Transfer code
    public void TransferCell(HeadquartersBuild otherHeadquartersBuild, GameObject cell)
    {
        CellsInArea.Remove(cell);
        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().cellChangedSide -= CellInListChangedSide;

        otherHeadquartersBuild.GetTransfer(cell);
    }
    public void GetTransfer(GameObject cell)
    {
        CellsInArea.Add(cell);

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().cellChangedSide += CellInListChangedSide;
    }



    private void AddBonusToCell(GameObject cell)
    {
        var cellSquadOnAreaScript = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellSquadsOnArea>();
        if (cellSquadOnAreaScript != null)
        {
            if (CheckSideProperties(cell))
            {
                cellSquadOnAreaScript.BonusHeadquarters = headquartersBonus;
            }
        }
        else { Debug.Log($"{cell.name} не настроенна"); }
    }
    private bool CheckSideProperties(GameObject cell)
    {
        return ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellArea>().Side ==
                    ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(CellWithThisBuild.gameObject).GetParameter<CellArea>().Side;
    }
}

[Building("CampBuild")]
[Serializable]
public class CampBuild : AbstractBuildings
{

    private int maxCountOfTrainingPeople = 15;

    public bool CanAddToTraining(int count) { return maxCountOfTrainingPeople>=count; }
    public override void InstanceBuild(int timeToBuild,int timeToUpgrade, int maxLevelOfConstruction)
    {
        TimeBuild = timeToBuild;
        TimeUpgrade = timeToUpgrade;

        MaxLevelBuild = maxLevelOfConstruction;

        Buff = null;

        Name = "Лагерь";
    }
    public override object Clone()
    {
        var newClone = new CampBuild();
        newClone.InstanceBuild(TimeBuild, TimeUpgrade, MaxLevelBuild);
        return newClone;
    }

    public override void ActivateBuild()
    {

    }
    public override void UpgradeBuild()
    {
        throw new System.NotImplementedException();
    }

}

[Building("FortBuild")]
[Serializable]
public class FortBuild : AbstractBuildings
{
    public override void InstanceBuild(int timeToBuild, int timeToUpgrade, int maxLevelOfConstruction)
    {
        TimeBuild = timeToBuild;
        TimeUpgrade = timeToUpgrade;

        MaxLevelBuild = maxLevelOfConstruction;

        Buff = new FortBuff();

        Name = "Форт";
    }
    public override object Clone()
    {
        var newClone = new FortBuild();
        newClone.InstanceBuild(TimeBuild, TimeUpgrade, MaxLevelBuild);
        return newClone;
    }

    public override void ActivateBuild()
    {

    }
    public override void UpgradeBuild()
    {
        Buff.SetBuffScale(40);
    }
}

[Building("AcademyBuild")]
[Serializable]
public class MilitaryAcademy : AbstractBuildings
{
    public override void InstanceBuild(int timeToBuild, int timeToUpgrade, int maxLevelOfConstruction)
    {
        TimeBuild = timeToBuild;
        TimeUpgrade = timeToUpgrade;

        MaxLevelBuild = maxLevelOfConstruction;

        Buff = new MilitaryAcademyBuff();

        Name = "Академия";
    }
    public override object Clone()
    {
        var newClone = new MilitaryAcademy();
        newClone.InstanceBuild(TimeBuild, TimeUpgrade, MaxLevelBuild);
        return newClone;
    }

    public override void ActivateBuild()
    {

    }
    public override void UpgradeBuild()
    {
        Debug.Log("Академия улучшена");
    }
}

//Скилы отрядов
[Building("FoxholeBuild")]
[Serializable]
public class FoxholeBuild : AbstractBuildings
{
    public override void InstanceBuild(int timeToBuild, int timeToUpgrade, int maxLevelOfConstruction)
    {
        TimeBuild = timeToBuild;
        TimeUpgrade = timeToUpgrade;

        MaxLevelBuild = maxLevelOfConstruction;

        Buff = new MilitaryAcademyBuff();

        Name = "Академия";
    }
    public override object Clone()
    {
        var newClone = new FoxholeBuild();
        newClone.InstanceBuild(TimeBuild, TimeUpgrade, MaxLevelBuild);
        return newClone;
    }
    public override void ActivateBuild()
    {

    }
    public override void UpgradeBuild() { }
}

//

public interface IBuild_BuildMaster:IBuild_LevelBuild,IBuild_MaxLevelBuils,IBuild_TimeBuild,IBuild_TimeUpgrade
{
    public abstract void ChangeLevel(int level);
    public abstract void UpgradeBuild();
}

public interface IBuild_TimeUpgrade
{
    int TimeUpgrade {  get; set; }
}
public interface IBuild_TimeBuild
{
    int TimeBuild { get; set; }
} 
public interface IBuild_LevelBuild
{
    int LevelBuild { get; set; }
}
public interface IBuild_MaxLevelBuils
{
    int MaxLevelBuild { get; set; }
}

