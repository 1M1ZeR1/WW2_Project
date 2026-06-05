using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EnemysController
{
    public DecisionTree DecisionTree { get; private set; } = new();

    public Dictionary<HeadquartersBuild, int> HeadquartersDangerPoints { get; private set; } = new();

    public Dictionary<AbstractSquad, GameObject> SquadOnCell { get; private set; } = new();

    public Dictionary<GameObject, HeadquartersBuild> CellWithHeadquarters_Bot { get; private set; } = new();
    public Dictionary<GameObject, HeadquartersBuild> CellWithHeadquarters_Player { get; private set; } = new();



    public List<HeadquartersBuild> Headquarters { get; private set; } = new();

    public Dictionary<HeadquartersBuild,HeadquartersAreaCasing> HeadquartersCasing { get; private set; } = new();


    public AlliesCellsAnalyzer AlliesCellsAnalyzer { get; private set; } = new();



    public GameObject mainEnemysCell { get; private set; }
    public GameObject mainAlliesCell{ get; private set; }

    protected int timer = 5;


    public void Start() 
    {
        foreach (var headquarter in HeadquartersCasing.Keys.ToList()) { HeadquartersCasing[headquarter] = new HeadquartersAreaCasing(headquarter, 350); }
        ServiceRegistry.WorkWithController<GameController>().oneSecondPassed += AddToTimer;

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<HeadquartersAreaCasing, HeadquartersBuild, List<GameObject>>((sender, headquartersBuild, needHelp) =>
        {
            CustomLog.BlueText($"I realy want to get help {headquartersBuild.CellWithThisBuild}");

            HeadquartersAreaCasing foundedHeadquarterToHelp = GetHeadquartersToSupport(headquartersBuild,3);

            foundedHeadquarterToHelp.SupportAction(needHelp);
        });


        var result = FindHeadquartersToAttack();
        AttackDirection attackDirection = new AttackDirection(CellWithHeadquarters_Bot[result.Item1], CellWithHeadquarters_Player[result.Item2]);
    }
    private void AddToTimer() { timer++;if(timer == 10) { DecisionTree.OneStep(); } }

    private (GameObject,GameObject) FindHeadquartersToAttack()
    {
        List<GameObject> playerCells = CellWithHeadquarters_Player.Keys.ToList();
        List<GameObject> botCells = CellWithHeadquarters_Bot.Keys.ToList();

        GameObject nearestBot = null, nearestPlayer = null;

        float minSqrDist = float.MaxValue;

        if(playerCells.Count > botCells.Count)
        {
            foreach(var botCell in botCells)
            {
                Vector3 cellPosBot = botCell.transform.position;

                foreach(var playerCell in playerCells)
                {
                    Vector3 cellPosPlayer = playerCell.transform.position;

                    float dx = cellPosPlayer.x - cellPosBot.x;
                    float dy = cellPosPlayer.y - cellPosBot.y;
                    float dz = cellPosPlayer.z - cellPosBot.z;

                    float sqrDist = dx * dx + dy * dy + dz * dz;

                    if (sqrDist < minSqrDist) 
                    {
                        minSqrDist = sqrDist;
                        nearestBot = botCell; nearestPlayer = playerCell;

                        if (sqrDist <= 100) return (nearestBot,nearestPlayer);
                    }
                }
            }
        }
        else
        {
            foreach (var playerCell in playerCells)
            {
                Vector3 cellPosPlayer = playerCell.transform.position;

                foreach(var botCell in botCells) 
                {
                    Vector3 cellPosBot = botCell.transform.position;

                    float dx = cellPosBot.x - cellPosPlayer.x;
                    float dy = cellPosBot.y - cellPosPlayer.y;
                    float dz = cellPosBot.z - cellPosPlayer.z;

                    float sqrDist = dx * dx + dy * dy + dz * dz;

                    if (sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        nearestBot = botCell; nearestPlayer = playerCell;

                        if (sqrDist <= 100) return (nearestBot, nearestPlayer);
                    }
                }
            }
        }

        return (nearestBot, nearestPlayer);
    }


    public HeadquartersBuild FindNearestHeadquarters(GameObject cellNeedFor)
    {
        float minDistance = float.MaxValue;
        HeadquartersBuild choosedHeadquarters = null;

        foreach (var headquarters in ServiceRegistry.WorkWithController<EnemysController>().CellWithHeadquarters_Bot)
        {
            var distance = Vector3.Distance(headquarters.Key.transform.position, cellNeedFor.transform.position);
            if (distance < minDistance) { choosedHeadquarters = headquarters.Value; minDistance = distance; }
        }

        return choosedHeadquarters;
    }

    private HeadquartersAreaCasing GetHeadquartersToSupport(HeadquartersBuild needHelpHeadquarters, int distanceLimitCount = 1)
    {
        var bufferedList = HeadquartersDangerPoints.Keys.ToList();
        bufferedList.Remove(needHelpHeadquarters);

        Vector3 needHelpHeadquartersPosition = needHelpHeadquarters.CellWithThisBuild.transform.position;

        Dictionary<HeadquartersBuild, float> headquartersDistanceToHelp = new();

        float MinDistance = float.MaxValue;

        foreach(var headquarter in bufferedList)
        {
            var distanceBetween = Vector3.Distance(headquarter.CellWithThisBuild.transform.position, needHelpHeadquartersPosition);

            if (distanceBetween < MinDistance)
            {
                if(headquartersDistanceToHelp.Keys.Count == distanceLimitCount)
                {
                    HeadquartersBuild headquartersToDelete = null;

                    foreach(var choosedHeadquarters in headquartersDistanceToHelp.Keys)
                    {
                        if (headquartersDistanceToHelp[choosedHeadquarters] > distanceBetween) { headquartersToDelete = choosedHeadquarters;break; }
                    }

                    headquartersDistanceToHelp.Remove(headquartersToDelete);
                }

                headquartersDistanceToHelp.Add(headquarter,distanceBetween);

                MinDistance = headquartersDistanceToHelp.Aggregate((x,y)=> x.Value > y.Value ? x : y).Value;
            }
        }

        if (headquartersDistanceToHelp.Count == 0) return null;

        Dictionary<HeadquartersBuild, float> helpfulnessOfHeadquarter = new();

        foreach(var headquarterItem in headquartersDistanceToHelp)
        {
            helpfulnessOfHeadquarter[headquarterItem.Key] = (float)(headquarterItem.Value * 0.4 + HeadquartersDangerPoints[headquarterItem.Key] * 0.6f);
        }

        return HeadquartersCasing[helpfulnessOfHeadquarter.Aggregate((x, y) => x.Value > y.Value ? x : y).Key];
    }
}

/// <summary>
/// //////////////////////////////////////////////////////////////
/// </summary>
public class DecisionTree
{
    private NodeFactory nodeFactory;

    private ActionConstructor _actionConstructor = new();

    private uint economyPointsModify = 50;
    public uint DangerPoints { get; set; } = 0;
    protected float _economyPoints = 500;

    public DecisionTree() { nodeFactory = new(this); }
    public float EconomyPoints
    {
        get {  return _economyPoints; }
        set { _economyPoints += value * (1 + DangerPoints * 10f); }
    }

    public void OneStep()
    {
        EconomyPoints += economyPointsModify;

        var commands = nodeFactory.CreateCommands(_actionConstructor.Construct_Economy(DangerPoints, EconomyPoints), _actionConstructor.Construct_Attack(DangerPoints, EconomyPoints));

        foreach (var command in commands) { ServiceRegistry.WorkWithService<CommandBus>().Enqueue(command, CommandPriority.High); }
    }

    private class ActionConstructor
    {
        protected Dictionary<int, List<ActionType_Economy>> _necessarilyActions = new()
        {
            {7, new List<ActionType_Economy>(){ ActionType_Economy.Economy_Squad_Train} },
            {8, new List<ActionType_Economy>(){ ActionType_Economy.Economy_Build_Economy,ActionType_Economy.Economy_Squad_Train} },
            {9, new List<ActionType_Economy>(){ ActionType_Economy.Economy_Squad_Train} },
            {10,new List<ActionType_Economy>(){ ActionType_Economy.Economy_Build_Economy,ActionType_Economy.Economy_Squad_Train} }

        };

        protected Dictionary<int, List<ActionType_Economy>> _dangerPointToAction_Economy = new()
        {
            {0,new List<ActionType_Economy>(){ActionType_Economy.Economy_Build_Economy} },
            {1,new List<ActionType_Economy>(){ActionType_Economy.Economy_Build_Economy,ActionType_Economy.Economy_Squad_Train} },
            {2,new List<ActionType_Economy>(){ActionType_Economy.Economy_Build_Economy,ActionType_Economy.Economy_Squad_Train} },
            {3,new List<ActionType_Economy>(){ActionType_Economy.Economy_Build_Economy,ActionType_Economy.Economy_Squad_Train} },
            {4,new List<ActionType_Economy>(){ActionType_Economy.Economy_Build_Economy,ActionType_Economy.Economy_Build_Protection} },
            {5,new List<ActionType_Economy>(){ActionType_Economy.Economy_Squad_Train,ActionType_Economy.Economy_Build_Protection} },
            {6,new List<ActionType_Economy>(){ActionType_Economy.Economy_Build_Economy,ActionType_Economy.Economy_Build_Protection} },
            {7,new List<ActionType_Economy>() },
            {8,new List<ActionType_Economy>(){ActionType_Economy.Economy_Build_Protection} },
            {9,new List<ActionType_Economy>() },
            {10,new List<ActionType_Economy>()}
        };
        protected Dictionary<int, List<ActionType_Attack>> _dangerPointToAction_Attack = new()
        {
            {0,new List<ActionType_Attack>() },
            {1,new List<ActionType_Attack>() },
            {2,new List<ActionType_Attack>(){ActionType_Attack.Attack_Exploration} },
            {3,new List<ActionType_Attack>(){ActionType_Attack.Attack_Exploration, ActionType_Attack.Attack_LowPower} },
            {4,new List<ActionType_Attack>()},
            {5,new List<ActionType_Attack>(){ ActionType_Attack.Attack_Exploration, ActionType_Attack.Attack_LowPower,ActionType_Attack.Attack_MediumPower} },
            {6,new List<ActionType_Attack>(){ActionType_Attack.Attack_Exploration} },
            {7,new List<ActionType_Attack>(){ ActionType_Attack.Attack_Exploration, ActionType_Attack.Attack_MediumPower, ActionType_Attack.Attack_HighPower} },
            {8,new List<ActionType_Attack>(){ ActionType_Attack.Attack_Exploration,ActionType_Attack.Attack_MediumPower, ActionType_Attack.Attack_HighPower} },
            {9,new List<ActionType_Attack>(){ ActionType_Attack.Attack_Exploration} },
            {10,new List<ActionType_Attack>(){ ActionType_Attack.Attack_Exploration, ActionType_Attack.Attack_MediumPower, ActionType_Attack.Attack_HighPower} }
        };

        public List<ActionType_Economy> Construct_Economy(uint dangerPoints,float economyPoints)
        {
            List<ActionType_Economy> listOfActionsType = new();

            listOfActionsType.Add(_dangerPointToAction_Economy[(int)dangerPoints][UnityEngine.Random.Range(0, _dangerPointToAction_Economy[(int)dangerPoints].Count)]);

            if (_necessarilyActions.ContainsKey((int)dangerPoints)) { listOfActionsType.AddRange(_necessarilyActions[(int)dangerPoints]);}

            return listOfActionsType;
        }
        public List<ActionType_Attack> Construct_Attack(uint dangerPoints, float economyPoints)
        {
            List<ActionType_Attack> listOfActionsType = new();

            listOfActionsType.Add(_dangerPointToAction_Attack[(int)dangerPoints][UnityEngine.Random.Range(0, _dangerPointToAction_Attack[(int)dangerPoints].Count-1)]);

            if (_necessarilyActions.ContainsKey((int)dangerPoints)) { listOfActionsType.AddRange(_necessarilyActions[(int)dangerPoints]); }

            return listOfActionsType;
        }
    }
}
public enum ActionType_Economy
{
    Economy_Build_Economy,
    Economy_Build_Protection,
    Economy_Squad_Train,
}
public enum ActionType_Attack
{
    Attack_Exploration,
    Attack_LowPower,
    Attack_MediumPower,
    Attack_HighPower
}

/// <summary>
/// //////////////////////////////////////////////////////////////
/// </summary>
public class NodeFactory
{
    private EconomyNode economyNode;
    private AttackNode attackNode;

    public NodeFactory(DecisionTree actionTree) { economyNode = new(actionTree); attackNode = new(actionTree); }
    public List<ICommand> CreateCommands(List<ActionType_Economy> actionsType_Economy, List<ActionType_Attack> actionsType_Attack)
    {
        Debug.LogError($"{actionsType_Economy.Count}");
        List<ICommand> createdCommands = new();

        if(actionsType_Economy.Count != 0)
        {
            foreach (var action in actionsType_Economy)
            {
                EconomyNode newCreatedCommand_Economy = (EconomyNode)economyNode.Clone();

                newCreatedCommand_Economy.ActionType_Economy = action;

                createdCommands.Add(newCreatedCommand_Economy);
            }
        }

        if(actionsType_Attack.Count != 0)
        {
            foreach (var action in actionsType_Attack)
            {
                AttackNode newCreatedCommand_Attack = (AttackNode)attackNode.Clone();

                newCreatedCommand_Attack.ActionType_Attack = action;

                createdCommands.Add(newCreatedCommand_Attack);
            }
        }

        return createdCommands;
    }
}

public class EconomyNode : INode,IClone,ICommand
{
    public ActionType_Economy ActionType_Economy { private get; set; }

    private DecisionTree actionTree;

    public Guid Id { get; set; }

    public CommandState State { get; private set; } = CommandState.Created;

    public EconomyNode(DecisionTree actionTree){this.actionTree = actionTree;}

    public object Clone(){ return new EconomyNode(actionTree); }


    public void Execute()
    {
        switch (ActionType_Economy)
        {
            case ActionType_Economy.Economy_Build_Economy:Economy_Build(Build_Type.Camp);break;
            case ActionType_Economy.Economy_Build_Protection:Economy_Build(Build_Type.Fort);break;
            case ActionType_Economy.Economy_Squad_Train:Economy_Squad();break;
        }
    }
    private void Economy_Build(Build_Type build_Type)
    {
        string build_id = "";
        switch (build_Type) 
        {
            case Build_Type.Camp:build_id = "CampBuild";break;
            case Build_Type.Academy:build_id = "AcademyBuild";break;
            case Build_Type.Fort:build_id = "FortBuild";break;
        }

        List<GameObject> headquartersCells = ServiceRegistry.WorkWithController<EnemysController>().CellWithHeadquarters_Bot.Keys.ToList();

        foreach (var cell in headquartersCells)
        {
            if (!ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuildings>().CheckBuildIsBuilt(build_id) &&
                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellBuildings>().buildInBuilding != build_id)
            {
                IEnumerator coroutine = ServiceRegistry.WorkWithController<BuilderController>().StartBuildProccess_Bot(cell, build_id, 1);
                if (coroutine == null)
                {
                    CustomLog.RedText($"Bot try build {build_id}");

                    HeadquartersChooser headquartersChooser = new(cell, true);
                    headquartersChooser.HeadquartersChoosed += (headquarters) =>
                    {
                        if (headquarters == null) { ServiceRegistry.WorkWithService<CommandBus>().Cancel(Id); }

                        else
                        {
                            PullUpSquads pullUpSquads = new(headquarters.CellWithThisBuild.gameObject, cell, 2, UnityEngine.Random.Range(3, 6));

                            pullUpSquads.CommandResult += (result) =>
                            {
                                Debug.LogError(result);
                                if (result)
                                {
                                    actionTree.EconomyPoints -= 500;

                                    ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(ServiceRegistry.WorkWithController<BuilderController>().StartBuildProccess_Bot(cell, "CampBuild", 1));
                                    ServiceRegistry.WorkWithService<CommandBus>().DeleteCommand(Id);
                                }
                                else { ServiceRegistry.WorkWithService<CommandBus>().DeleteCommand(Id); }
                            };

                            ServiceRegistry.WorkWithService<CommandBus>().Enqueue(pullUpSquads);
                        }
                    };

                    ServiceRegistry.WorkWithService<CommandBus>().Enqueue(headquartersChooser);

                    break;
                }
                else
                {
                    //Debug.LogError("Без проблем строю");

                    actionTree.EconomyPoints -= 500;

                    ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(coroutine);
                    ServiceRegistry.WorkWithService<CommandBus>().DeleteCommand(Id);

                    break;
                }
            }
        }
    }
    private void Economy_Squad()
    {
        List<GameObject> headquartersCells = ServiceRegistry.WorkWithController<EnemysController>().CellWithHeadquarters_Bot.Keys.ToList();

        foreach (var cell in headquartersCells) 
        {

            HeadquartersChooser headquartersChooser = new(cell, true);
            headquartersChooser.HeadquartersChoosed += (headquarters) =>
            {
                if (headquarters == null) { ServiceRegistry.WorkWithService<CommandBus>().Cancel(Id); }

                else
                {
                   
                }
            };

            ServiceRegistry.WorkWithService<CommandBus>().Enqueue(headquartersChooser);

            break;
        }
    }

    public bool CanExecute()
    {
        return true;
    }

    public void Prepare()
    {
        State = CommandState.Prepared;
    }

    public void Cancel()
    {
        throw new NotImplementedException();
    }

    private enum Build_Type
    {
        Camp,
        Academy,
        Fort,
    }
}

public class AttackNode: INode, IClone, ICommand
{
    public ActionType_Attack ActionType_Attack { private get; set; }

    private DecisionTree actionTree;

    public Guid Id { get; set; }

    public CommandState State { get; private set; } = CommandState.Created;

    public AttackNode(DecisionTree actionTree) { this.actionTree = actionTree; }

    public object Clone() { return new AttackNode(actionTree); }


    public void Execute()
    {
        switch (ActionType_Attack)
        {
            case ActionType_Attack.Attack_Exploration:break;
            case ActionType_Attack.Attack_LowPower:break;
            case ActionType_Attack.Attack_MediumPower:break;
            case ActionType_Attack.Attack_HighPower:break;
        }
    }
    private void Attack_Exploration()
    {

    }
    private void Attack_LowPower()
    {

    }

    private (HeadquartersBuild,HeadquartersBuild) GetHeadquartersToAttack()
    {
        return (null, null);
    }

    public bool CanExecute()
    {
        return true;
    }

    public void Prepare()
    {
        State = CommandState.Prepared;
    }

    public void Cancel()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// //////////////////////////////////////////////////////////////
/// </summary>


public interface INode
{
    public void Execute();
}

enum DangerPoints_Detection
{
    DetectionSquadsNear,
    NewHeadquartersBuilt
}
enum EconomyPoints_Actions
{
    HeadquartersBuilt,
    CampBuilt,
    AcademyBuild
}
