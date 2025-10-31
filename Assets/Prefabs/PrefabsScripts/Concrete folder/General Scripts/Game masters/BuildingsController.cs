using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderController
{
    public Dictionary<GameObject, (AbstractBuildings, BuildingModule)> cellsToBuildingObjects { get; private set; } = new();

    public Action<GameObject> RequestUIUpdate;

    public IEnumerator StartBuildProccess(GameObject cell, string id, int buildLevel)
    {
        var parameters = TryStartBuilding(cell, id, buildLevel);

        if (parameters != null)
        {
            AbstractBuildings newBuild = (AbstractBuildings)ServiceRegistry.WorkWithService<ObjectFactory_Builds>().CreateObject(id);

            cellsToBuildingObjects.Add(cell, (newBuild, CreateBuildingModule(id, cell)));

            if (buildLevel == 1) { parameters.Add(newBuild.TimeBuild); }
            else { parameters.Add(newBuild.TimeUpgrade); }

            return BuildingConstruction(parameters[0], parameters[1], newBuild,id, cell);
        }

        return null;
    }
    private List<float> TryStartBuilding(GameObject cell, string id, int buildLevel)
    {
        if (!ServiceRegistry.WorkWithController<ResourcesController>().CheckResourcesToBuild(id, buildLevel))
        {
            ServiceRegistry.WorkWithController<MessageScript>().EnableMessage();
            ServiceRegistry.WorkWithController<MessageScript>().SendMessage(5);
            return null;
        }

        List<float> parameters = new (){ 0 };

        Debug.LogError($"Count of squads on this cell:{ServiceRegistry.WorkWithController<GameController>().GetAllSquadsOnCell(cell).Count}");

        foreach (AbstractSquad squad in ServiceRegistry.WorkWithController<GameController>().GetAllSquadsOnCell(cell))
        {
            Debug.LogError($"Squad name:{squad.Name}, Squad speed:{squad.Speed}, Squad build skill:{squad.BuildingSkill}");
            if (squad.SquadAction != SquadActions.None) { continue; }
            parameters[0] += squad.BuildingSkill;
            squad.SquadAction = SquadActions.Building;
        }
        if (parameters[0] == 0)
        {
            ServiceRegistry.WorkWithController<MessageScript>().EnableMessage();
            ServiceRegistry.WorkWithController<MessageScript>().SendMessage_Custom($"Ќа клетке {cell.name}, общий навык строительства равен 0"); return null;
        }

        return parameters;
    }
    private BuildingModule CreateBuildingModule(string id, GameObject cell)
    {
        var buildingModuleScript = ServiceRegistry.WorkWithController<WorldOnCanvasScript>().CreateBuildingModule(cell,id);

        return buildingModuleScript;
    }
    private IEnumerator BuildingConstruction(float speedOfBuilding, float timeToBuild, AbstractBuildings build,string id, GameObject cell)
    {
        float currentTimeToBuild = timeToBuild;

        void TimerUpdate()
        {
            if (PauseScript.CurrentGameState != GameState.Play) return;

            cellsToBuildingObjects[cell].Item2.ChangeFillAmount(1 - timeToBuild / currentTimeToBuild);
            timeToBuild -= speedOfBuilding;

            if (timeToBuild <= 0)
            {
                ServiceRegistry.WorkWithController<GameController>().oneSecondPassed -= TimerUpdate;

                BuldingWasOver(build, cell, id);
            }
        }


        ServiceRegistry.WorkWithController<GameController>().oneSecondPassed += TimerUpdate;

        while (timeToBuild > 0)
        {
            yield return null;
        }
    }
    public void BuldingWasOver(AbstractBuildings build, GameObject cell, string id)
    {
        cellsToBuildingObjects[cell].Item2.InvokeDestroy();

        build.ActivateBuild();

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuildings>().AddToBuildsList(build, id);

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuildings>().buildInBuilding = null;

        foreach (var squad in ServiceRegistry.WorkWithController<GameController>().GetAllSquadsOnCell(cell))
        {
            squad.SquadAction = SquadActions.None;
        }
        

        RequestUIUpdate.Invoke(cell);

        cellsToBuildingObjects.Remove(cell);
    }
}