using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingPanelScript : MonoBehaviour
{
    [Header("Панель отряда в тренировке")]
    [SerializeField] private GameObject trainingSquadPanel;

    [Header("Панель создания")]
    [SerializeField] private GameObject creatingPanel;
    private CreatingPanelScript creatingPanelScript;

    protected GameObject _interactableCell;

    protected Dictionary<AbstractSquad, GameObject> _squadToCell = new Dictionary<AbstractSquad, GameObject>();

    private void Start()
    {
        creatingPanelScript = creatingPanel.GetComponent<CreatingPanelScript>();
    }

    private void OnEnable()
    {
        PauseScript.SetGameState(GameState.Pause);
    }
    private void OnDisable()
    {
        PauseScript.SetGameState(GameState.Play);
    }
    public void GetAllTrainingSquads(GameObject cell)
    {
        _interactableCell = cell;

        ClearList(trainingSquadPanel.transform.parent);

        List<AbstractSquad> trainingSquads = ServiceRegistry.WorkWithController<GameController>().GetAllSquadsInTraining(_interactableCell);

        if(trainingSquads != null)
        {
            foreach(var squad in trainingSquads)
            {
                if (_squadToCell.ContainsKey(squad))
                {
                    _squadToCell[squad].gameObject.SetActive(true);
                    continue;
                }
            }
        }
    }
    private void ClearList(Transform transformToClean)
    {
        foreach (Transform child in transformToClean)
        {
            if (child.name.Contains("(Clone)"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }
    public void CreateSquadTrainingPanel(AbstractSquad squad)
    {
        ServiceRegistry.WorkWithController<GameController>().AddSquadInTraining(_interactableCell, squad);

        CreateTrainingSquad(squad);

        GetAllTrainingSquads(_interactableCell);
    }

    public void OpenTrainingPanel(GameObject cell)
    {
        gameObject.SetActive(true);
        _interactableCell = cell;
    }
    public void CloseTrainingPanel()
    {
        gameObject.SetActive(false);
    }
    public void ShowCreatePanel()
    {
        creatingPanelScript.SetCurrentWorkingCell(_interactableCell);
        creatingPanel.SetActive(true);
    }
    private void CreateTrainingSquad(AbstractSquad squad)
    {
        int timeToTrainAllSquad = squad.TrainingTime * squad.PeopleCount;
        ServiceRegistry.WorkWithController<AlliesSpawner>().CreateCoroutine(timeToTrainAllSquad,squad,GetProgressBar(squad));
    }
    private Image GetProgressBar(AbstractSquad squad)
    {
        CreatePanel(squad);

        GameObject squadTrainingPanel = _squadToCell[squad];

        return squadTrainingPanel.transform.Find("BackGround/ProccessOfTraining").GetComponent<Image>();
    }
    private void CreatePanel(AbstractSquad squad)
    {
        GameObject newTrainingPanel = Instantiate(trainingSquadPanel, trainingSquadPanel.transform.parent);

        newTrainingPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = squad.Name;
        newTrainingPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = squad.PeopleCount.ToString();

        newTrainingPanel.SetActive(true);
        _squadToCell.Add(squad, newTrainingPanel);
    }
}
