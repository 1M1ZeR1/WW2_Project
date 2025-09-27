using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    [Header("Главный контроллер")]
    [SerializeField] private GameObject gameControllerObject;
    protected GameController _gameControllerScript;

    [Header("Контроллер битв")]
    [SerializeField] private GameObject battleControllerObject;
    protected BattleController _battleControllerScript;

    [Header("Контроллер ресурсов")]
    [SerializeField] private GameObject resourcesControllerObject;
    protected ResourcesController _resourcesControllerScript;

    [SerializeField] private int timeToGetNewSecondaryQuest;

    [Header("Держатель времени")]
    [SerializeField] private GameObject timeTaker;
    private TimeControllerScript timeControllerScript;

    [Header("Панель с квестами")]
    [SerializeField] private GameObject questUI;
    private QuestUIScript questUIScript;

    protected bool _HaveCaptureQuest = false;

    protected float _timer;
    void Start()
    {
        gameControllerObject.TryGetComponent(out _gameControllerScript);
        battleControllerObject.TryGetComponent(out _battleControllerScript);
        resourcesControllerObject.TryGetComponent(out _resourcesControllerScript);

        timeTaker.TryGetComponent(out timeControllerScript);
        questUI.TryGetComponent(out questUIScript);

        //_gameControllerScript.oneSecondPassed += OtherTimerTaker;
        timeControllerScript.OnHourHasPassed += OneHourWasPassed;

        _globalQuests = new List<AbstractQuest>()
    {
        new CaptureCellQuest(4320,GameObject.Find("VoronoiCell(378)"),45,20, TimeControllerScript.GetCurrentDateTime().AddHours(5),new string[]
        {
            "Освобождение Гродно",
            "Освободить город Гродно от захватчиков",
@"Город находится в осаде. Вражеские войска удерживают ключевой стратегический район, 
и твоя задача — переломить ход битвы. Без лишних приказов и сложных манёвров: 
ты должен взять контроль над указанной зоной и закрепить позицию.
Цель: прорваться через линию обороны, дойти до города и взять его под контроль.
"
        })
    };

        _HaveCaptureQuest = true;
    }
    private void OneHourWasPassed(DateTime currentTime)
    {
        GetQuestFromQuestList();
    }

    private void GetQuestFromQuestList()
    {
        if(_globalQuests.Count == 0) { return; }
        if (_globalQuests[0].CheckToInvokeQuest())
        {
            if (_globalQuests[0].GetType() == typeof(CaptureCellQuest)) 
            {
                _battleControllerScript.SideOnCellWasChanged += ChechCapturedCellInQuest;
                _gameControllerScript.SideOnCellWasChanged += ChechCapturedCellInQuest;
            }
            InvokeQuest(_globalQuests[0]);
        }
    }
    
    private void ChechCapturedCellInQuest(GameObject cell)
    {
        foreach(var quest in _startedQuests)
        {
            if(quest is CaptureCellQuest captureQuest)
            {
                if(captureQuest.IsCellCaptured(cell)){

                    foreach(var itemReward in captureQuest.GetReward())
                    {
                        _resourcesControllerScript.AddSomeResourcesByType(itemReward.Key, itemReward.Value);
                    }

                    _startedQuests.Remove(quest);
                    questUIScript.QuestIsComplited(quest);Debug.Log("ПОБЕДА!!!!");
                    break;
                }
            }
        }
    }

    private void InvokeQuest(AbstractQuest quest)
    {
        _globalQuests.Remove(quest);
        _startedQuests.Add(quest);

        questUIScript.CreateQuestPanel(quest);
    }
    protected List<AbstractQuest> _globalQuests;
    protected List<AbstractQuest> _startedQuests = new List<AbstractQuest>();   
}
