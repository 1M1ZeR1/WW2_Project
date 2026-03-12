using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HeadquartersChooser : ICommand
{
    public enum FindFor
    {

    }


    protected HeadquartersBuild _headquartersBuild;
    private HeadquartersBuild nearestHeadquarters = null;

    private GameObject startCell;
    private bool economyStep;

    public Action<HeadquartersBuild> HeadquartersChoosed;


    public Guid Id { get; set; }

    public CommandState State { get; private set; } = CommandState.Created;

    public HeadquartersChooser(GameObject startCell = null, bool economyStep = false) { this.startCell = startCell; this.economyStep = economyStep; }

    public void Cancel()
    {

    }

    public bool CanExecute()
    {
        return true;
    }

    public void Execute()
    {
        HeadquartersChoosed?.Invoke(nearestHeadquarters);

        State = CommandState.Completed;
    }



    private HeadquartersBuild FindNearestHeadquarters(List<HeadquartersBuild> headquartersBuilds)
    {
        float distance = float.MaxValue;
        HeadquartersBuild nearestHeadquarters = null;

        foreach (var headquarters in headquartersBuilds)
        {
            float? resultCost = ServiceRegistry.WorkWithController<AAlgorithm>().CalculateWayCost(startCell, headquarters.CellWithThisBuild.gameObject, SideEnum.Enemys);

            if (resultCost == null) continue;
            if (resultCost < distance) { distance = (float)resultCost; nearestHeadquarters = headquarters; }
        }

        return nearestHeadquarters;
    }


    public void Prepare()
    {
        if (economyStep)
        {
            _headquartersBuild = (HeadquartersBuild)ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(startCell)
                .GetParameter<CellBuildings>().builds["HeadquartersBuild"];

            int currentDangerPoint = ServiceRegistry.WorkWithController<EnemysController>().HeadquartersDangerPoints[_headquartersBuild];

            List<HeadquartersBuild> headquartersListCommon = new();

            foreach (var headquartersDanger in ServiceRegistry.WorkWithController<EnemysController>().HeadquartersDangerPoints)
            {
                if (headquartersDanger.Key == _headquartersBuild) { continue; }

                if (headquartersDanger.Value < currentDangerPoint) { headquartersListCommon.Add(headquartersDanger.Key); }
            }

            if (headquartersListCommon.Count() != 0) { nearestHeadquarters = FindNearestHeadquarters(headquartersListCommon); }
            else
            {
                headquartersListCommon = ServiceRegistry.WorkWithController<EnemysController>().HeadquartersDangerPoints.Keys.ToList();
                headquartersListCommon.Remove(_headquartersBuild);
                nearestHeadquarters = FindNearestHeadquarters(headquartersListCommon);
            }
        }
        else
        {
            var headquartersListCommon = ServiceRegistry.WorkWithController<EnemysController>().HeadquartersDangerPoints.Keys.ToList();
            headquartersListCommon.Remove(_headquartersBuild);
            nearestHeadquarters = FindNearestHeadquarters(headquartersListCommon);
        }

        State = CommandState.Prepared;
    }
}
