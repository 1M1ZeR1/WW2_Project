using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AbstractBuildings:IName,IType<BuildsEnum>
{
    private int timeToBuild;
    private int timeToUpgrade;

    private int levelOfConstruction;
    private int maxLevelOfConstruction;

    public BuildsEnum Type { get; set; }
    public string Name { get; set; } = "null";
    public AbstractBuffs Buff { get; set; }
    public int LevelBuild { get; set; }
    public int MaxLevelBuild { get; set; }
    public int TimeBuild { get; set; }
    public int TimeUpgrade { get; set; }
    public abstract void UpgradeBuild();

    public abstract void ActivateBuild();

    public bool IsUpgraded() { if(levelOfConstruction > 1) { return true; } return false; }
}



public class HeadquartersBuild : AbstractBuildings
{
    private List<GameObject> cellsInArea = new();

    protected int _searchingRadius = 200;
    protected Transform _cellWithThisBuild;

    private int headquartersBonus = 3;

    public HeadquartersBuild(int timeToBuild, int maxLevelOfConstruction, Transform cellWherePlaced)
    {
        TimeBuild = timeToBuild;
        TimeUpgrade = 20;

        MaxLevelBuild = maxLevelOfConstruction;

        Type = BuildsEnum.HeadQuarters;
        Buff = null;

        Name = "Штаб";

        _cellWithThisBuild = cellWherePlaced;
    }
    public override void ActivateBuild()
    {
        SearchingCells();
    }
    public override void UpgradeBuild()
    {
        _searchingRadius += 300;

        headquartersBonus += 2;

        SearchingCells();
    }

    private void SearchingCells()
    {
        var findedCells = Physics.OverlapSphere(_cellWithThisBuild.position, _searchingRadius).Select(colider => colider.gameObject).ToList();

        if(cellsInArea.Count == 0) { cellsInArea = findedCells.ToList();AddBonusToCells(); return; }

        foreach(var cell in findedCells)
        {
            if (cellsInArea.Contains(cell)) continue;

            cellsInArea.Add(cell);
        }

        AddBonusToCells();
    }
    private void AddBonusToCells()
    {
        foreach(var cell in cellsInArea)
        {
            var cellSquadOnAreaScript = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellSquadsOnArea>();
            if(cellSquadOnAreaScript != null)
            {
                cellSquadOnAreaScript.BonusHeadquarters = headquartersBonus;
            }
            else { Debug.Log($"{cell.name} не настроенна"); }
        }
    }
}
public class CampBuild : AbstractBuildings
{
    private int maxCountOfTrainingSquads = 3;

    public bool CanAddToTraining(int count) { return maxCountOfTrainingSquads>=count; }
    public CampBuild() 
    {
        TimeBuild = 30;
        TimeUpgrade = 20;

        MaxLevelBuild = 2;

        Type = BuildsEnum.Camp;
        Buff = null;

        Name = "Лагерь";
    }
    public CampBuild(int timeToBuild, int maxLevelOfConstruction)
    {
        TimeBuild = timeToBuild;
        TimeUpgrade = 20;

        MaxLevelBuild = maxLevelOfConstruction;

        Type = BuildsEnum.Camp;
        Buff = null;

        Name = "Лагерь";
    }

    public override void ActivateBuild()
    {

    }
    public override void UpgradeBuild()
    {
        throw new System.NotImplementedException();
    }

}
public class FortBuild : AbstractBuildings
{
    public FortBuild()
    {
        TimeBuild = 60;
        TimeUpgrade = 40;

        MaxLevelBuild = 2;

        Type = BuildsEnum.Fort;
        Buff = new FortBuff();

        Name = "Форт";
    }
    public FortBuild(int timeToBuild, int maxLevelOfConstruction)
    {
        TimeBuild = timeToBuild;
        TimeUpgrade = 40;

        MaxLevelBuild = maxLevelOfConstruction;

        Type = BuildsEnum.Fort;
        Buff = new FortBuff();

        Name = "Форт";
    }

    public override void ActivateBuild()
    {

    }
    public override void UpgradeBuild()
    {
        Buff.SetBuffScale(40);
    }
}
public class MilitaryAcademy : AbstractBuildings
{
    public MilitaryAcademy()
    {
        TimeBuild = 100;
        TimeUpgrade = 40;

        MaxLevelBuild = 2;

        Type = BuildsEnum.MilitaryAcademy;
        Buff = new MilitaryAcademyBuff();

        Name = "Академия";
    }
    public MilitaryAcademy(int timeToBuild, int maxLevelOfConstruction)
    {
        TimeBuild = timeToBuild;
        TimeUpgrade = 40;

        MaxLevelBuild = maxLevelOfConstruction;

        Type = BuildsEnum.MilitaryAcademy;
        Buff = new MilitaryAcademyBuff();

        Name = "Академия";
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
public class FoxholeBuild : AbstractBuildings
{
    public FoxholeBuild()
    {
        Buff = new FoxholeBuff();
        Type = BuildsEnum.Foxhole;

        Name = "Окоп";
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

