using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public abstract class AbsctractEvent
{
    private EventType eventType;
    public EventBuffsType BuffType { get;private set; }
    public EventInfluenceType InfluenceType { get; private set; }

    public string EventHeading { get; set; }
    public string EventDiscription { get; set; }

    public string[] EventActionsDicriptions { get; set; }

    public string[] EventActionsString { get; set; }
    public Action[] EventActions { get; set; }

    public int time { get; set; }

    public List<GlobalLogElement> GlobalLogElement { get; set; } = new();
}
public class RandomEvent : AbsctractEvent
{
    public RandomEvent(string eventHeading, string eventDiscription, string[] eventActionsDicriptions, string[] eventActionsFromTable)
    {
        EventHeading = eventHeading;
        EventDiscription = eventDiscription;
        EventActionsDicriptions = eventActionsDicriptions;

        foreach(var text in EventActionsDicriptions) { Debug.LogError(text); }
        EventActionsString = eventActionsFromTable;
    }
}
public class EventParser
{


    public AbsctractEvent CreateEventFromTable(string[] keys)
    {
#if UNITY_EDITOR
        TextAsset csvText = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/EventTable/RandomEventTable.csv");
#else
        TextAsset csvText = Resources.Load<TextAsset>("EventTable/RandomEventTable");
#endif

        List<string[]> csvData = new List<string[]>();

        string[] lines = csvText.text.Split('\n');
        foreach (string line in lines)
        {
            string[] row = line.Split(",,");
            csvData.Add(row);
        }

        List<string[]> eventToChoise = new List<string[]>();

        foreach(string[] line in csvData)
        {
            List<string> tagList = line[0].Split('#',StringSplitOptions.RemoveEmptyEntries).ToList();
            if (keys.All(tag => tagList.Contains(tag)))
            {
                eventToChoise.Add(line);
            }

        }

        int randomIndexEvent = UnityEngine.Random.Range(0,eventToChoise.Count);

        return new RandomEvent(eventToChoise[randomIndexEvent][1], eventToChoise[randomIndexEvent][2], new string[] { eventToChoise[randomIndexEvent][3], eventToChoise[randomIndexEvent][4] }, new string[] { eventToChoise[randomIndexEvent][5], eventToChoise[randomIndexEvent][6] });
    }
    private static string EventTypeConverter(EventType eventType)
    {
        if (eventType == EventType.Battle) { return "Боевое"; }
        if (eventType == EventType.Nature) { return "Природное"; }
        if (eventType == EventType.Influence) { return "Влияние"; }
        if (eventType == EventType.Resource) { return "Ресурсные"; }
        if (eventType == EventType.Global) { return "Глобальные"; }

        return "Ничего";
    }
    private static string EventBuffTypeConverter(EventBuffsType eventBuffsType)
    {
        if(eventBuffsType == EventBuffsType.Bad) { return "Bad"; }
        if (eventBuffsType == EventBuffsType.Good) { return "Good"; }

        return "None";
    }
    private static string EventInfluenceTypeConverter(EventInfluenceType eventInfluenceType)
    {
        if(eventInfluenceType == EventInfluenceType.Squad) { return "Squad"; }
        if (eventInfluenceType == EventInfluenceType.Cell) { return "Cell"; }
        if (eventInfluenceType == EventInfluenceType.Resource) { return "Resource"; }
        return "None";
    }
}
public class EventCreater
{
    private EventParser eventParser = new EventParser();

    protected List<string> currentActionsStrings = new List<string>();

    public AbsctractEvent CreateEvent(string[] keys)
    {
        AbsctractEvent newEvent = eventParser.CreateEventFromTable(keys);

        currentActionsStrings = newEvent.EventActionsString.ToList();

        return newEvent;
    }
    public void CreateActionsForEvent(AbsctractEvent currentEvent, AbstractSquad[] allSquads)
    {
        List<Action> resultActions = new List<Action>();

        foreach (string actionString in currentActionsStrings)
        {
            List<string> action = actionString.Split(new char[] { ' ', ',' }).ToList();

            if (action.Count == 0) { resultActions.Add(null); continue; }

            for (int i = 0; i < action.Count; i++)
            {
                if (stringToType.ContainsKey(action[i]))
                {
                    var newAction = ActionCreater(stringToType[action[i]], action[i + 1], action[i + 2], allSquads, action[i + 3],currentEvent);
                    resultActions.Add(newAction);

                    i += 4;
                }
            }
        }

        if (currentEvent.GlobalLogElement.Count != 0) { foreach (var logElement in currentEvent.GlobalLogElement) 
            {
                logElement.TextTitle = $"{currentEvent.EventHeading}";
                logElement.TextContent = $"{currentEvent.EventDiscription}";
            } 
        }

        resultActions[0] += () => { currentEvent.GlobalLogElement.ForEach(log => { LogsController.AddLogElement(log,SideEnum.None); }); };
        if(resultActions.Count > 1) resultActions[1] += () => { currentEvent.GlobalLogElement.ForEach(log => { LogsController.AddLogElement(log, SideEnum.None); }); };

        currentEvent.EventActions = resultActions.ToArray();
    }
    private Action ActionCreater(EventInfluenceType eventInfluenceType,string plusOrMinus, string count, AbstractSquad[] allSquads, string timeForBuff,
        AbsctractEvent currentEvent)
    {
        int cellsCount = 0;
        int squadsCount = 0;

        switch (eventInfluenceType)
        {
            case EventInfluenceType.CSpeed:
                return ApplyEfectOnCell(UnityEngine.Random.Range(2, 4), eventInfluenceType, plusOrMinus, count, timeForBuff, currentEvent);
            case EventInfluenceType.SSpeed: break;
            case EventInfluenceType.GSpeed: break;
            case EventInfluenceType.CAttack:
                return ApplyEfectOnCell(UnityEngine.Random.Range(2, 4), eventInfluenceType, plusOrMinus, count, timeForBuff, currentEvent);
            case EventInfluenceType.SAttack: break;
            case EventInfluenceType.GAttack: break;
            case EventInfluenceType.CProtection:
                return ApplyEfectOnCell(UnityEngine.Random.Range(2, 4), eventInfluenceType, plusOrMinus, count, timeForBuff, currentEvent);
            case EventInfluenceType.SProtection: break;
            case EventInfluenceType.GProtection: break;
            case EventInfluenceType.CStealth: break;
            case EventInfluenceType.SStealth: break;
            case EventInfluenceType.GStealth: break;
            case EventInfluenceType.CSummon:
                
                break;
            case EventInfluenceType.SSummon: break;
            case EventInfluenceType.GSummon: break;
            case EventInfluenceType.CResource: break;
            case EventInfluenceType.SResource: break;
            case EventInfluenceType.GResource: break;
            case EventInfluenceType.CHeight:
                return ApplyEfectOnCell(UnityEngine.Random.Range(2, 4), eventInfluenceType, plusOrMinus, count, timeForBuff, currentEvent);
        }
        return null;
    }

    private Action ApplyEfectOnCell(int count,
        EventInfluenceType eventInfluenceType, string plusOrMinus, string strength, string timeForBuff,
        AbsctractEvent currentEvent)
    {
        Action eventAction = null;

        for (int i = 0; i < count; i++)
        {
            var selectedCell = GetRandomCellWithoutEvent(ServiceRegistry.WorkWithService<CellsInCameraAreaScript>().visiableCells);
            if (selectedCell != null) { 
                eventAction += CellBuffCreater(selectedCell, eventInfluenceType, plusOrMinus, strength, timeForBuff);

                currentEvent.GlobalLogElement.Add(new GlobalLogElement(selectedCell));
            }
        }
        eventAction += () => PauseScript.SetGameState(GameState.Play);
        eventAction += () => CameraMovementScript.UnBlockMovement();

        return eventAction;
    }

    private Action CellBuffCreater(GameObject cell, EventInfluenceType eventInfluenceType, string plusOrMinus, string count, string timeForBuff)
    {
        BuffsController buffsController = ServiceRegistry.WorkWithController<BuffsController>();

        int buffScale = 0;
        if(plusOrMinus == "-") { buffScale = -(int.Parse(count)); }
        else { buffScale = int.Parse(count); }

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
            GetParameter<CellType>().SetWithEventBuff(true);

        if(eventInfluenceType == EventInfluenceType.CSpeed)
        {
            return (() =>
            {
                RandomEventBuff newRandomEventBuff = new RandomEventBuff(new float[] { buffScale, 0, 0 }, TimeControllerScript.GetCurrentDateTime(), int.Parse(timeForBuff));

                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellBuffs>().AddBuffWithTimer(newRandomEventBuff);

                buffsController.AddBuffToList(newRandomEventBuff);
            });
        }
        if(eventInfluenceType == EventInfluenceType.CAttack)
        {
            return (() =>
            {
                RandomEventBuff newRandomEventBuff = new RandomEventBuff(new float[] { 0, buffScale, 0 }, TimeControllerScript.GetCurrentDateTime(), int.Parse(timeForBuff));

                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellBuffs>().AddBuffWithTimer(newRandomEventBuff);

                buffsController.AddBuffToList(newRandomEventBuff);
            });
        }
        if(eventInfluenceType == EventInfluenceType.CProtection)
        {
            return (() =>
            {
                RandomEventBuff newRandomEventBuff = new RandomEventBuff(new float[] { 0, 0, buffScale }, TimeControllerScript.GetCurrentDateTime(), int.Parse(timeForBuff));

                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellBuffs>().AddBuffWithTimer(newRandomEventBuff);

                buffsController.AddBuffToList(newRandomEventBuff);
            });
        }
        if (eventInfluenceType == EventInfluenceType.CHeight)
        {
            return (() =>
            {
                UnReachableBuff newRandomEventBuff = new UnReachableBuff(TimeControllerScript.GetCurrentDateTime(), int.Parse(timeForBuff), UnReachableBuffType.Fire,cell);

                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).
                GetParameter<CellBuffs>().AddBuffWithTimer(newRandomEventBuff);

                buffsController.AddBuffToList(newRandomEventBuff);
            });
        }

        return null;
        //дописать
    }
    private Action SquadSummoner(GameObject[] workingCells, string plusOrMinus, string count, string timeForBuff)
    {
        GameObject eventMaster = GameObject.Find("Random Event Master");

        int buffScale = 0;
        if (plusOrMinus == "-") { buffScale = -(int.Parse(count)); }
        else { buffScale = int.Parse(count); }

        return () =>
        {
            
        };
    }
    private IEnumerator RandomSquadSummoner(GameObject[] workingCells,AbstractSquad[] squads, SideEnum side)
    {
        while (true)
        {
            if (!ServiceRegistry.WorkWithService<CellAccessibilityValidator>().InteractWithAlliesCell(workingCells[UnityEngine.Random.Range(0, workingCells.Length)]))
            {
                
            }
        }
    }
    private Action SquadBuffCreater(AbstractSquad squad, EventInfluenceType eventInfluenceType, string plusOrMinus, string count, string timeForBuff)
    {
        BuffsController buffsController = GameObject.Find("Buffs Master").GetComponent<BuffsController>();

        int buffScale = 0;
        if (plusOrMinus == "-") { buffScale = -(int.Parse(count)); }
        else { buffScale = int.Parse(count); }

        if (eventInfluenceType == EventInfluenceType.SSpeed)
        {
            return () =>
            {
                RandomEventBuff newRandomEventBuff = new RandomEventBuff(new float[] { buffScale, 0, 0 }, TimeControllerScript.GetCurrentDateTime(), int.Parse(timeForBuff));

                buffsController.AddBuffToList(newRandomEventBuff);

                squad.AddBuff(newRandomEventBuff);
            };
        }
        if (eventInfluenceType == EventInfluenceType.SAttack)
        {
            return () =>
            {
                RandomEventBuff newRandomEventBuff = new RandomEventBuff(new float[] { 0, buffScale, 0 }, TimeControllerScript.GetCurrentDateTime(), int.Parse(timeForBuff));

                buffsController.AddBuffToList(newRandomEventBuff);

                squad.AddBuff(newRandomEventBuff);
            };
        }
        if (eventInfluenceType == EventInfluenceType.SProtection)
        {
            return () =>
            {
                RandomEventBuff newRandomEventBuff = new RandomEventBuff(new float[] { 0, 0, buffScale }, TimeControllerScript.GetCurrentDateTime(), int.Parse(timeForBuff));

                buffsController.AddBuffToList(newRandomEventBuff);

                squad.AddBuff(newRandomEventBuff);
            };
        }

        return null;
    }
    private GameObject GetRandomCellWithoutEvent(List<GameObject> cells)
    {
        int attempts = 5;

        while (attempts > 0)
        {
            var randomCell = cells[UnityEngine.Random.Range(0,cells.Count - 1)];

            if (!ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(randomCell).GetParameter<CellType>().IsWithEventBuff())
            {
                return randomCell;
            }
            attempts--;
        }
        return null;
    }
    private enum EventInfluenceType
    {
        None,

        CSpeed,
        SSpeed,
        GSpeed,

        CAttack,
        SAttack,
        GAttack,

        CProtection,
        SProtection,
        GProtection,

        CStealth,
        SStealth,
        GStealth,

        CSummon,
        SSummon,
        GSummon,

        CResource,
        SResource,
        GResource,

        CHeight
    }
    private Dictionary<string, EventInfluenceType> stringToType = new Dictionary<string, EventInfluenceType>() 
    {
        {"CSpe",EventInfluenceType.CSpeed },
        {"SSpe",EventInfluenceType.SSpeed },
        {"GSpe",EventInfluenceType.GSpeed },

        {"CAtt",EventInfluenceType.CAttack},
        {"SAtt",EventInfluenceType.SAttack},
        {"GAtt",EventInfluenceType.GAttack},

        {"CPro",EventInfluenceType.CProtection},
        {"SPro",EventInfluenceType.SProtection},
        {"GPro",EventInfluenceType.GProtection},

        {"CSte",EventInfluenceType.CStealth },
        {"SSte",EventInfluenceType.SStealth },
        {"GSte",EventInfluenceType.GStealth },

        {"CSum",EventInfluenceType.CSummon },
        {"SSum",EventInfluenceType.SSummon },
        {"GSum",EventInfluenceType.GSummon },

        {"CRes",EventInfluenceType.CResource },
        {"SRes",EventInfluenceType.SResource },
        {"GRes",EventInfluenceType.GResource },

        {"CHeight", EventInfluenceType.CHeight}
    };
}
