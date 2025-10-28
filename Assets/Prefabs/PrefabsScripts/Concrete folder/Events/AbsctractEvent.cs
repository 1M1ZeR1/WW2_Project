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
    private EventBuffsType buffType;
    private EventInfluenceType influenceType; 

    private string eventHeading;
    private string eventDiscription;

    private string[] eventActionsDicriptions;

    private string[] eventActionsString;
    private Action[] eventActions;

    private int time;

    public virtual void SetActions(Action[] actions) { eventActions = actions; }

    public EventBuffsType GetEventBuffsType() { return buffType; }
    public EventInfluenceType GetInfluenceType() { return influenceType; }

    public void SetEventHeading(string heading) { eventHeading = heading; }
    public string GetEventHeading() {  return eventHeading; }
    public void SetEventDiscription(string discription) {  eventDiscription = discription; }
    public string GetEventDiscription() {  return eventDiscription; }
    public void SetEventActionsDiscriptions(string[] discriptions) { eventActionsDicriptions = discriptions; }
    public string[] GetEventActionsDiscriptions() { return eventActionsDicriptions; }
    public void SetEventActionsString(string[] actionsString) {  eventActionsString = actionsString; }
    public string[] GetEventActionsString() {  return eventActionsString; }
    public void SetEventActions(Action[] actions) { eventActions = actions; }
    public Action[] GetEventActions() {  return eventActions; }

    public void SetTime(int time) { this.time = time; }
    public int GetTime() { return time; }
}
public class RandomEvent : AbsctractEvent
{
    public RandomEvent(string eventHeading, string eventDiscription, string[] eventActionsDicriptions, string[] eventActionsFromTable)
    {
        SetEventHeading(eventHeading);
        SetEventDiscription(eventDiscription);
        SetEventActionsDiscriptions(eventActionsDicriptions);
        SetEventActionsString(eventActionsFromTable);
    }
}
public class EventParser
{


    public AbsctractEvent CreateEventFromTable(string[] keys)
    {
#if UNITY_EDITOR
        TextAsset csvText = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Prefabs/Events/EventsTable/RandomEventTable.csv");
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

        currentActionsStrings = newEvent.GetEventActionsString().ToList();

        return newEvent;
    }
    public void CreateActionsForEvent(AbsctractEvent currentEvent, AbstractSquad[] allSquads,GameObject[] cells)
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
                    resultActions.Add(ActionCreater(stringToType[action[i]], action[i + 1], action[i +2],allSquads, cells, action[i+3]));

                    i += 4;
                }
            }
        }

        currentEvent.SetEventActions(resultActions.ToArray());
    }
    private Action ActionCreater(EventInfluenceType eventInfluenceType,string plusOrMinus, string count, AbstractSquad[] allSquads,  GameObject[] cells, string timeForBuff)
    {
        int cellsCount = 0;
        int squadsCount = 0;
        Action newAction = null;

        switch (eventInfluenceType)
        {
            case EventInfluenceType.CSpeed:
                cellsCount = UnityEngine.Random.Range(2, 10);
                for (int i = 0; i < cellsCount; i++)
                {
                    newAction += CellBuffCreater(GetCellWithoutEvent(cells), eventInfluenceType, plusOrMinus, count, timeForBuff);
                }
                newAction += ()=>PauseScript.SetGameState(GameState.Play);
                newAction += ()=> CameraMovementScript.UnBlockMovement();
                return newAction;
            case EventInfluenceType.SSpeed: break;
            case EventInfluenceType.GSpeed: break;
            case EventInfluenceType.CAttack:
                cellsCount = UnityEngine.Random.Range(2, 10);
                for (int i = 0; i < cellsCount; i++)
                {
                    newAction += CellBuffCreater(GetCellWithoutEvent(cells), eventInfluenceType, plusOrMinus, count, timeForBuff);
                }
                newAction += () => PauseScript.SetGameState(GameState.Play);
                newAction += () => CameraMovementScript.UnBlockMovement();
                return newAction;
            case EventInfluenceType.SAttack: break;
            case EventInfluenceType.GAttack: break;
            case EventInfluenceType.CProtection:
                cellsCount = UnityEngine.Random.Range(2, 10);
                for (int i = 0; i < cellsCount; i++)
                {
                    newAction += CellBuffCreater(GetCellWithoutEvent(cells), eventInfluenceType, plusOrMinus, count, timeForBuff);
                }
                newAction += () => PauseScript.SetGameState(GameState.Play);
                newAction += () => CameraMovementScript.UnBlockMovement();
                return newAction;
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
                newAction += CellBuffCreater(GetCellWithoutEvent(cells), eventInfluenceType, plusOrMinus, count, timeForBuff);
                newAction += () => PauseScript.SetGameState(GameState.Play);
                newAction += () => CameraMovementScript.UnBlockMovement();
                return newAction;
        }
        return null;
    }
    private Action CellBuffCreater(GameObject cell, EventInfluenceType eventInfluenceType, string plusOrMinus, string count, string timeForBuff)
    {
        CellTypeScript cellTypeScript = cell.GetComponent<CellTypeScript>();

        BuffsController buffsController = GameObject.Find("Buffs Master").GetComponent<BuffsController>();

        int buffScale = 0;
        if(plusOrMinus == "-") { buffScale = -(int.Parse(count)); }
        else { buffScale = int.Parse(count); }

        cellTypeScript.SetWithEventBuff(true);

        if(eventInfluenceType == EventInfluenceType.CSpeed)
        {
            return (() =>
            {
                RandomEventBuff newRandomEventBuff = new RandomEventBuff(new float[] { buffScale, 0, 0 }, TimeControllerScript.GetCurrentDateTime(), int.Parse(timeForBuff));

                cellTypeScript.AddBuffWithTimer(newRandomEventBuff);

                buffsController.AddBuffToList(newRandomEventBuff);
            });
        }
        if(eventInfluenceType == EventInfluenceType.CAttack)
        {
            return (() =>
            {
                RandomEventBuff newRandomEventBuff = new RandomEventBuff(new float[] { 0, buffScale, 0 }, TimeControllerScript.GetCurrentDateTime(), int.Parse(timeForBuff));

                cellTypeScript.AddBuffWithTimer(newRandomEventBuff);

                buffsController.AddBuffToList(newRandomEventBuff);
            });
        }
        if(eventInfluenceType == EventInfluenceType.CProtection)
        {
            return (() =>
            {
                RandomEventBuff newRandomEventBuff = new RandomEventBuff(new float[] { 0, 0, buffScale }, TimeControllerScript.GetCurrentDateTime(), int.Parse(timeForBuff));

                cellTypeScript.AddBuffWithTimer(newRandomEventBuff);

                buffsController.AddBuffToList(newRandomEventBuff);
            });
        }
        if (eventInfluenceType == EventInfluenceType.CHeight)
        {
            return (() =>
            {
                UnReachableBuff newRandomEventBuff = new UnReachableBuff(TimeControllerScript.GetCurrentDateTime(), int.Parse(timeForBuff), UnReachableBuffType.Fire,cell);

                cellTypeScript.AddBuffWithTimer(newRandomEventBuff);

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
            if (!ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(workingCells[UnityEngine.Random.Range(0, workingCells.Length)]))
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
    private GameObject GetCellWithoutEvent(GameObject[] cells)
    {
        foreach (GameObject cell in cells)
        {
            if (!cell.GetComponent<CellTypeScript>().IsWithEventBuff())return cell;
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
