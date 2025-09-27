using Mono.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Android.Gradle;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using static BuildingsController;

public class BuildingsController : MonoBehaviour
{
    [Header("Контроллер взаимодействия")]
    [SerializeField] private GameObject interactableControllerObject;
    private InteractableScript interactableScript;

    private GameObject currentChosenCell;

    [SerializeField] private Transform panelsTaker;

    void Start()
    {
        GetComponents();

        foreach(Transform panel in panelsTaker)
        {
            Button buildButton = panel.Find("BuildButton").GetComponent<Button>();
            buildButton.onClick.AddListener(() =>
            {
                buildButton.interactable = false;

                var coroutine = ServiceRegistry.WorkWithController<BuilderController>().StartBuildProccess(currentChosenCell,panel.GetComponent<BuildItemPanel>().GetBuidType(),BuildsLevel.First);

                if(coroutine != null)
                {
                    GameController.AddActionToQueue(() => StartCoroutine(coroutine));
                }
            });

            Button upgradeButton = panel.Find("UpgradeButton").GetComponent<Button>();
            upgradeButton.onClick.AddListener(() =>
            {
                upgradeButton.interactable = false;

                var coroutine = ServiceRegistry.WorkWithController<BuilderController>().StartBuildProccess(currentChosenCell, panel.GetComponent<BuildItemPanel>().GetBuidType(), BuildsLevel.Second);

                if (coroutine != null)
                {
                    GameController.AddActionToQueue(() => StartCoroutine(coroutine));
                }
            });
        }
    }

    private void GetComponents()
    {
        interactableControllerObject.TryGetComponent(out interactableScript);
        interactableScript.playerIsInteract += UpdateChoosenCell;
    }

    private void UpdateChoosenCell(GameObject cell) { currentChosenCell = cell; }
}
public class BuilderController
{
    public Dictionary<GameObject, (AbstractBuildings, BuildingModule)> cellsToBuildingObjects { get; private set; } = new();

    public Action<GameObject> RequestUIUpdate;

    public IEnumerator StartBuildProccess(GameObject cell, BuildsEnum buildType, BuildsLevel buildLevel)
    {
        var parameters = TryStartBuilding(cell, buildType, buildLevel);

        if (parameters != null)
        {
            var newBuild = WhatBuild(buildType, cell);

            cellsToBuildingObjects.Add(cell, (newBuild, CreateBuildingModule(buildType, cell)));

            if (buildLevel == BuildsLevel.First) { parameters.Add(newBuild.TimeBuild); }
            else { parameters.Add(newBuild.TimeUpgrade); }

            return BuildingConstruction(parameters[0], parameters[1], newBuild, cell);
        }

        return null;
    }

    private AbstractBuildings WhatBuild(BuildsEnum buildType, GameObject cell)
    {
        switch (buildType)
        {
            case BuildsEnum.HeadQuarters: return new HeadquartersBuild(5, 2, cell.transform);//(30, 2, cell.transform)
            case BuildsEnum.Camp: return new CampBuild(5, 2);//(60, 2)
            case BuildsEnum.Fort: return new FortBuild(5, 2);//(100, 2)
            case BuildsEnum.MilitaryAcademy: return new MilitaryAcademy(5, 2);//(160, 2)
        }

        return null;
    }
    private List<float> TryStartBuilding(GameObject cell, BuildsEnum buildType, BuildsLevel buildLevel)
    {
        if (!ServiceRegistry.WorkWithController<ResourcesController>().CheckResourcesToBuild(buildType, buildLevel))
        {
            ServiceRegistry.WorkWithController<MessageScript>().EnableMessage();
            ServiceRegistry.WorkWithController<MessageScript>().SendMessage(5);
            return null;
        }

        List<float> parameters = new List<float> { 0 };

        foreach (AbstractSquad squad in ServiceRegistry.WorkWithController<GameController>().GetAllSquadsOnCell(cell))
        {
            if (squad.Action != SquadActions.None) { continue; }
            parameters[0] += squad.BuildingSkill;
            squad.Action = SquadActions.Building;
        }
        if (parameters[0] == 0)
        {
            ServiceRegistry.WorkWithController<MessageScript>().EnableMessage();
            ServiceRegistry.WorkWithController<MessageScript>().SendMessage(9); return null;
        }

        return parameters;
    }
    private BuildingModule CreateBuildingModule(BuildsEnum buildType, GameObject cell)
    {
        var buildingModuleScript = ServiceRegistry.WorkWithController<WorldOnCanvasScript>().CreateBuildingModule(cell);
        buildingModuleScript.SetBuildingType(buildType);

        return buildingModuleScript;
    }
    private IEnumerator BuildingConstruction(float speedOfBuilding, float timeToBuild, AbstractBuildings build, GameObject cell)
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

                BuldingWasOver(build, cell);
            }
        }


        ServiceRegistry.WorkWithController<GameController>().oneSecondPassed += TimerUpdate;

        while (timeToBuild > 0)
        {
            yield return null;
        }
    }
    public void BuldingWasOver(AbstractBuildings build, GameObject cell)
    {
        cellsToBuildingObjects[cell].Item2.InvokeDestroy();

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuildings>().AddToBuildsList(build, build.Type);

        foreach (var squad in ServiceRegistry.WorkWithController<GameController>().GetAllSquadsOnCell(cell))
        {
            squad.Action = SquadActions.None;
        }
        

        RequestUIUpdate.Invoke(cell);

        cellsToBuildingObjects.Remove(cell);

        build.ActivateBuild();
    }
}