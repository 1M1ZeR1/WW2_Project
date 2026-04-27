using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlliesSpawner
{
    private GameObject trainingPanel;

    public GameObject currentWorkingCell { private get; set;}

    public void Initialize(GameObject trainingPanel)
    {
        this.trainingPanel = trainingPanel;

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject, bool>((sender, cell, UI_resolution) =>
        {
            currentWorkingCell = cell;
        });
    }

    public IEnumerator TrainingSquads(int timeToTrain, AbstractSquad trainingSquad, Image progressBar)
    {
        timeToTrain = (int)(timeToTrain*0.1f);//Debug-убрать после баланса
        int otherTime = timeToTrain;

        while (timeToTrain > 0)
        {
            yield return new WaitUntil(()=>PauseScript.CurrentGameState == GameState.Play);

            yield return new WaitForSeconds(1f);

            if(progressBar != null)
            {
                progressBar.fillAmount = 1 - (float)timeToTrain / (float)otherTime;
            }

            timeToTrain--;
        }

        ServiceRegistry.WorkWithController<GameController>().TrainingIsOver(trainingSquad);
        GameObject.Destroy(progressBar.transform.parent.transform.parent.gameObject);

    }
    public void CreateCoroutine(int timeToTrain, AbstractSquad trainingSquad, Image progressBar)
    {
        ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(TrainingSquads(timeToTrain, trainingSquad, progressBar));
    }
    public int GetCountOfPeopleResource()
    {
       return ServiceRegistry.WorkWithController<ResourcesController>().GetCountOfResourceByType(ResourcesEnum.People);
    }
    public bool CanAddNewSquadInTraining(int countOfPeople)
    {
        CampBuild camp = (CampBuild)ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(currentWorkingCell).GetParameter<CellBuildings>().builds["CampBuild"];

        return camp.CanAddToTraining(countOfPeople);
    }
    public void RemovePeople(int count) { ServiceRegistry.WorkWithController<ResourcesController>().RemoveSomeResourcesByType(ResourcesEnum.People, count); }
    public void RemoveResource(int count, ResourcesEnum resourceType) { ServiceRegistry.WorkWithController<ResourcesController>().RemoveSomeResourcesByType(resourceType, count); }
    public int GetCountOfResource(ResourcesEnum resourceType) { return ServiceRegistry.WorkWithController<ResourcesController>().GetCountOfResourceByType(resourceType); }
}
