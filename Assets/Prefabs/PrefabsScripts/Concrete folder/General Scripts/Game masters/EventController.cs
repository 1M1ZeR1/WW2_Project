using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventController : MonoBehaviour
{

    [Header("Панель ивента")]
    [SerializeField] private GameObject eventPanelObject;
    private EventPanelController eventPanelController;

    [Header("Время создания ивента")]
    [SerializeField] private int timerToEvent;

    [Header("Время поставок")]
    [SerializeField] private int timerToSupplies;

    //MVP project
    [SerializeField] private GameObject[] workingCells;

    protected float _timerForSupplies;
    protected float _timer = 0;
    protected int _countOfBadEvents = 0;

    [Header("Шанс хорошо ивента")]
    [SerializeField] private int chanceOfGoodEvent_Proccesentes;

    private EventCreater eventCreater = new EventCreater();

    void Start()
    {
        ServiceRegistry.WorkWithController<GameController>().oneSecondPassed += OtherTimerController;

        eventPanelObject.TryGetComponent(out eventPanelController);

        _timerForSupplies = timerToSupplies - 5;
    }

    private void OtherTimerController()
    {
        if (PauseScript.CurrentGameState != GameState.Play) { return; }
        _timer++;   _timerForSupplies++;
        if(_timer >= timerToEvent)
        {
            _timer = 0;
            StartEvent();
        }
        if(_timerForSupplies >= timerToSupplies)
        {
            _timerForSupplies = 0;
            GetSuppliesEvent();
        }
    }
    private void StartEvent()
    {
        RandomEvent currentEvent = (RandomEvent)eventCreater.CreateEvent(CreateRandomTagsForEvent());
        eventCreater.CreateActionsForEvent(currentEvent,ServiceRegistry.WorkWithController<GameController>().GetAllSquadsInGame().ToArray(), workingCells);

        PauseScript.SetGameState(GameState.Pause);

        eventPanelController.ShowEvent(currentEvent);
        eventPanelObject.SetActive(true);
        CameraMovementScript.BlockMovement();
    }
    private string[] CreateRandomTagsForEvent()
    {
        List<string> resultTags = new List<string>();

        switch (UnityEngine.Random.Range(0, 4))
        {
            case 0: resultTags.Add("Клетка");break;
            case 1: resultTags.Add("Клетка"); break;
            case 2: resultTags.Add("Клетка"); break;
            case 3: resultTags.Add("Клетка"); break;
        }

        resultTags.Add("Передвижение");
        resultTags.Add("Природа");

        return resultTags.ToArray();
    }
    private AbstractSquad GetRandomSquad()
    {
        List<AbstractSquad>? squads = ServiceRegistry.WorkWithController<GameController>().GetAllSquadsInGame();

        if (squads == null)
        {
            return null;
        }
        return squads[Random.Range(0, squads.Count)];
    }
    private void GetSuppliesEvent()
    {
        //Добавить множетели
        float ration = ServiceRegistry.WorkWithController<ResourcesController>().GetInfluenceRation();

        ServiceRegistry.WorkWithController<ResourcesController>().AddSomeResourcesByType(ResourcesEnum.Building,200+(int)(200*ration));
        ServiceRegistry.WorkWithController<ResourcesController>().AddSomeResourcesByType(ResourcesEnum.Weapon, 50 + (int)(100 * ration));
        ServiceRegistry.WorkWithController<ResourcesController>().AddSomeResourcesByType(ResourcesEnum.People, 30 + (int)(100 * ration));
        ServiceRegistry.WorkWithController<ResourcesController>().AddSomeResourcesByType(ResourcesEnum.Transport, 50 + (int)(100 * ration));
    }

    public void SetWorkingArray(GameObject[] cells)
    {
        workingCells = cells;
    }
}
