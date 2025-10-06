using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlliesSpawner : MonoBehaviour
{

    [Header("Максимальное кол-во тренирующихся отрядов в лагере")]
    [SerializeField] private int maxCountOfSquadsInCamp;

    protected int _currentSquadsInTraining;

    [Header("Тренировочное окно")]
    [SerializeField] private GameObject trainingPanel;

    public delegate void SquadTrainingResult(AbstractSquad squad);
    public SquadTrainingResult trainingSquadResult;
    private void Start()
    {
        trainingSquadResult += ServiceRegistry.WorkWithController<GameController>().TrainingIsOver;
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

        if(trainingSquadResult != null)
        {
            trainingSquadResult.Invoke(trainingSquad);
            Destroy(progressBar.transform.parent.transform.parent.gameObject);
        }
    }
    public void CreateCoroutine(int timeToTrain, AbstractSquad trainingSquad, Image progressBar)
    {
        StartCoroutine(TrainingSquads(timeToTrain, trainingSquad, progressBar));
    }
    public int GetCountOfPeopleResource()
    {
       return ServiceRegistry.WorkWithController<ResourcesController>().GetCountOfResourceByType(ResourcesEnum.People);
    }
    public bool CanAddNewSquadInTraining()
    {
        if (_currentSquadsInTraining < GetComponent<CellBuildings>().builds[BuildsEnum.Camp].MaxLevelBuild) { return true; }
        else return false;
    }
    public void RemovePeople(int count) { ServiceRegistry.WorkWithController<ResourcesController>().RemoveSomeResourcesByType(ResourcesEnum.People, count); }
    public void RemoveResource(int count, ResourcesEnum resourceType) { ServiceRegistry.WorkWithController<ResourcesController>().RemoveSomeResourcesByType(resourceType, count); }
    public int GetCountOfResource(ResourcesEnum resourceType) { return ServiceRegistry.WorkWithController<ResourcesController>().GetCountOfResourceByType(resourceType); }
}
