using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static BattleController;

public class BattleController : MonoBehaviour
{
    public enum BattleSide
    {
        None,
        Attack,
        Defence
    }

    protected Dictionary<GameObject, Battle> _currentBattles = new Dictionary<GameObject, Battle>();
    protected Dictionary<GameObject, GameObject> _cellToBattlemodule = new Dictionary<GameObject, GameObject>();

    public delegate void CellCaptured(GameObject cell);
    public event CellCaptured SideOnCellWasChanged;

    public delegate void BattleIsOver(GameObject cell);
    public event BattleIsOver BattleOnCellIsOver;

    public delegate void OnlyEnemy_SquadInfo(List<AbstractSquad> squads);
    public event OnlyEnemy_SquadInfo OnlyEnemySquadInfo;

    [Header("Главный контроллер")]
    [SerializeField] private GameObject gameControllerObject;
    private GameController gameController;

    [Header("Контроллер ресурсов")]
    [SerializeField] private GameObject resourcesControllerObject;
    private ResourcesController resourcesController;

    [Header("Canvas битвы")]
    [SerializeField]private GameObject battleCanvas;
    private WorldOnCanvasScript battleCanvasScript;

    private void Start()
    {
        gameControllerObject.TryGetComponent(out gameController);

        resourcesControllerObject.TryGetComponent(out resourcesController);

        battleCanvas.TryGetComponent(out battleCanvasScript);
    }
    public void TryStartBattle(GameObject disputedTerritory,GameObject startBattleCell, List<AbstractSquad> squadsStartAttack)
    {
        if (CheckExistingBattle(disputedTerritory, startBattleCell, squadsStartAttack)) return;

        GameObject newBattleModule = battleCanvasScript.CreateBattleModule();

        Battle newBattle = new Battle(disputedTerritory, startBattleCell,gameController, newBattleModule.GetComponent<BattleModule>(), squadsStartAttack);

        _cellToBattlemodule.Add(disputedTerritory, newBattleModule);
        _currentBattles.Add(disputedTerritory,newBattle);

        newBattle.battleIsover += (GameObject cell, bool changedSide) =>
        {
            _cellToBattlemodule.Remove(cell);
            _currentBattles.Remove(cell);

            if (changedSide) SideOnCellWasChanged.Invoke(cell);
        };
    }
    public void TryStartBattle_Simple(GameObject disputedTerritory, List<AbstractSquad> squads)
    {
        foreach(AbstractSquad squad in squads)
        {
            TryStartBattle(disputedTerritory,gameController.GetCellWithThisSquad(squad),new List<AbstractSquad> { squad });
        }
    }

    private bool CheckExistingBattle(GameObject disputedTerritory, GameObject attackCell, List<AbstractSquad> squadsStartAttack)
    {
        if (_currentBattles.ContainsKey(disputedTerritory)){
            _currentBattles[disputedTerritory].AddOtherAttackDirection(attackCell,squadsStartAttack);

            return true;
        }
        foreach (var battle in _currentBattles.Values)
        {
            if(battle.CheckHasAttackCell(attackCell))
            {
                TryStartBattle(disputedTerritory, attackCell, squadsStartAttack);

                return true;
            }
        }

        return false;
    }

    public void CheckDrawnIntoBattle(AbstractSquad squad, GameObject cell)
    {
        if (_currentBattles.ContainsKey(cell))
        {
            squad.Action = SquadActions.Battle;
            _currentBattles[cell].DragInBattle(squad);
        }
    }
}

public sealed class Battle
{
    private BattleModule battleModuleScript;

    private GameObject currentBattleCell;

    private List<GameObject> listOfAttackCells;

    private SquadsManager squadsManager;

    public delegate void BattleIsOver(GameObject cell, bool changedSide);
    public event BattleIsOver battleIsover;

    public Battle
        (GameObject currentBattleCell, GameObject attackCell, 
        GameController gameControllerScript,BattleModule battleModuleScript, 
        List<AbstractSquad> squadsStartAttack)
    {
        this.currentBattleCell = currentBattleCell;

        listOfAttackCells = new List<GameObject>() { attackCell};

        squadsManager = new SquadsManager(gameControllerScript);

        foreach(var squad in gameControllerScript.GetAllSquadsOnCell(currentBattleCell))
        {
            squadsManager.AddToSquadsInDefence(squad);
        }
        foreach (var squad in squadsStartAttack)
        { 
            squadsManager.AddToSQuadsInAttack(attackCell, squad);
        }


        this.battleModuleScript = battleModuleScript;

        ConfigurateBattleModule();
    }

    private void ConfigurateBattleModule()
    {
        battleModuleScript.SetSquads(squadsManager.GetListOfSquadsDefence(), squadsManager.GetListOfSquadsAttack_ByCell(listOfAttackCells[0]));
        battleModuleScript.SetBattleCell(currentBattleCell);

        battleModuleScript.battleIsOver += BattleisOver;
    }
    private void BattleisOver(bool success) 
    {
        if (success) 
        {
            ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(currentBattleCell).GetParameter<CellArea>().RequestToControlCell(squadsManager.GetListOfSquadsAttack_ByCell(listOfAttackCells[0])[0].Side);
            squadsManager.DeleteAllDefenceSquads();

            foreach (var attackCell in listOfAttackCells)
            {
                squadsManager.ResultBattleForAttackers(attackCell,currentBattleCell);
            }
        }
        else
        {
            squadsManager.ResultBattleForDefenders();

            foreach(var attackCell in listOfAttackCells)
            {
                squadsManager.DeleteAllAttackSquads(attackCell);
            }
        }

        battleModuleScript = null;
        battleIsover.Invoke(currentBattleCell, success);
    }
    public bool CheckHasAttackCell(GameObject attackingCell)
    {
        if (listOfAttackCells.Contains(attackingCell))
        {
            battleModuleScript.RemoveSquadsFromBuffer_Later(squadsManager.GetListOfSquadsAttack_ByCell(attackingCell));
            squadsManager.RemoveAllAttackSquads(attackingCell);

            return true;
        }

        //Здесь должна быть проверка на наличие атакующей клетки клетки в списке и после вернуть эту клетку и отряды на ней

        return false;
    }


    public void DragInBattle(AbstractSquad squad) { squadsManager.AddToSquadsInDefence(squad); }

    public void AddOtherAttackDirection(GameObject cell, List<AbstractSquad> squadsStartAttack)
    {
        if (!listOfAttackCells.Contains(cell)) { listOfAttackCells.Add(cell); }

        foreach(var squad in squadsStartAttack)
        {
            squadsManager.AddToSQuadsInAttack(cell, squad);
        }
        battleModuleScript.AddSquadsToBuffer(squadsStartAttack, BattleSide.Attack);
    }
}

public class SquadsManager
{
    private List<AbstractSquad> squadsInDefence = new List<AbstractSquad>();

    private Dictionary<GameObject, List<AbstractSquad>> attackCellsToSquadsInAttack = new Dictionary<GameObject, List<AbstractSquad>>();

    private GameController gameControllerScript;

    
    public SquadsManager(GameController gameControllerScript) { this.gameControllerScript = gameControllerScript; }

    public void AddToSquadsInDefence(AbstractSquad squad) 
    { 
        squadsInDefence.Add(squad);

        SetSquadAction(squad);
    }
    public void AddToSQuadsInAttack(GameObject cell, AbstractSquad squad) 
    {
        if (!attackCellsToSquadsInAttack.ContainsKey(cell)) { attackCellsToSquadsInAttack.Add(cell, new List<AbstractSquad>()); }
        attackCellsToSquadsInAttack[cell].Add(squad);
    
        SetSquadAction(squad);
    }

    public List<AbstractSquad> GetListOfSquadsDefence() { return squadsInDefence; }
    public List<AbstractSquad> GetListOfSquadsAttack_ByCell(GameObject cell) { return attackCellsToSquadsInAttack[cell]; }

    private void SetSquadAction(AbstractSquad squad)
    {
        if (squad.Action != SquadActions.Battle) { squad.Action = SquadActions.Battle; }
        else
        {
            squad.Action = SquadActions.None;
        }

        gameControllerScript.UpdateSquadInformation_SwipeState(squad);
    }

    public void DeleteAllDefenceSquads() { foreach(var squad in squadsInDefence)gameControllerScript.SingleThrasher_Squad(squad); }
    public void DeleteAllAttackSquads(GameObject cell) { foreach (var squad in attackCellsToSquadsInAttack[cell]) gameControllerScript.SingleThrasher_Squad(squad); }
    public void RemoveAllAttackSquads(GameObject cell) { attackCellsToSquadsInAttack.Remove(cell); }

    public void ResultBattleForDefenders()
    {
        foreach (var squad in squadsInDefence)
        {
            if (squad.IsDead)
            {
                gameControllerScript.SingleThrasher_Squad(squad); continue;
            }

            SetSquadAction(squad);
            gameControllerScript.AllowBotToAct(squad);
        }
    }
    public void ResultBattleForAttackers(GameObject cell, GameObject cellTo) 
    {
        foreach(var squad in attackCellsToSquadsInAttack[cell])
        {
            if (squad.IsDead)
            {
                gameControllerScript.SingleThrasher_Squad(squad); continue;
            }

            squad.Action = SquadActions.None;

            gameControllerScript.UpdateSquadInformation_ChangeCell(squad, cell, cellTo);
            gameControllerScript.AllowBotToAct(squad);
        }   
    } 

    public void HelpSquadSwipeCell(AbstractSquad squad, GameObject cellFrom, GameObject cellTo)
    {
        gameControllerScript.UpdateSquadInformation_ChangeCell(squad, cellFrom, cellTo);
    }
}