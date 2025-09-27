using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlliesSpawner : MonoBehaviour
{
    [Header("Главный контроллер")]
    [SerializeField] private GameObject gameControllerObject;
    private GameController gameControllerScript;

    [Header("Контроллер ресурсов")]
    [SerializeField] private GameObject resourceControllerObject;
    private ResourcesController resourcesControllerScript;

    [Header("Максимальное кол-во тренирующихся отрядов в лагере")]
    [SerializeField] private int maxCountOfSquadsInCamp;

    protected int _currentSquadsInTraining;

    [Header("Тренировочное окно")]
    [SerializeField] private GameObject trainingPanel;

    public delegate void SquadTrainingResult(AbstractSquad squad);
    public SquadTrainingResult trainingSquadResult;
    private void Start()
    {
        gameControllerObject.TryGetComponent(out gameControllerScript);
        resourceControllerObject.TryGetComponent(out resourcesControllerScript);

        trainingSquadResult += gameControllerScript.TrainingIsOver;
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
       return resourcesControllerScript.GetCountOfResourceByType(ResourcesEnum.People);
    }
    public bool CanAddNewSquadInTraining()
    {
        if (_currentSquadsInTraining < GetComponent<CellBuildings>().GetBuildByType(BuildsEnum.Camp).MaxLevelBuild) { return true; }
        else return false;
    }
    public void RemovePeople(int count) { resourcesControllerScript.RemoveSomeResourcesByType(ResourcesEnum.People, count); }
    public void RemoveResource(int count, ResourcesEnum resourceType) { resourcesControllerScript.RemoveSomeResourcesByType(resourceType, count); }
    public int GetCountOfResource(ResourcesEnum resourceType) { return resourcesControllerScript.GetCountOfResourceByType(resourceType); }
}
