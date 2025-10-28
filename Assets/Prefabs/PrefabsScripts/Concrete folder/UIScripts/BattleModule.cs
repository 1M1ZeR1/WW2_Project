using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleModule : MonoBehaviour
{
    protected enum BattleAction
    {
        None,
        Defence,
        Attack
    }

    [Header("Таймер одного хода боя")]
    [SerializeField] private int timerForOneTurn;

    [Header("Предельное кол-во людей отряде")]
    [SerializeField] private int minCountOfPeopleInSquad;

    protected float _timer;

    protected GameObject _battleCell;

    protected List<AbstractSquad> _squadDefence, _squadAttack;
    private Dictionary<AbstractSquad, BattleController.BattleSide> squadsNeedAddLater = new Dictionary<AbstractSquad, BattleController.BattleSide> ();
    private List<AbstractSquad> squadsNeedToDelete = new List<AbstractSquad>();

    protected float _ratioDefenceSide, _ratioAttackSide, _allParameters;

    [Header("Панель атаки")]
    [SerializeField] private GameObject attackPanel;
    protected Image _attackImage;

    [Header("Панель защиты")]
    [SerializeField] private GameObject defencePanel;
    protected Image _defenceImage;

    [Header("Окно битв")]
    [SerializeField] private GameObject battlePanelObject;


    public delegate void BattleIsOver(bool result);
    public BattleIsOver battleIsOver;

    protected Dictionary<AbstractSquad, float> _squadSpeedBattle = new Dictionary<AbstractSquad, float>();
    protected float _currentSpeedBattle;

    protected BattleModuleUIScript _battleModuleUIScript;



    private void Awake()
    {
        defencePanel.TryGetComponent(out _defenceImage);
        attackPanel.TryGetComponent(out _attackImage);

        ServiceRegistry.WorkWithController<GameController>().oneSecondPassed += OtherTimerController;

        battlePanelObject.TryGetComponent(out _battleModuleUIScript);
    }
    private void Start()
    {
        _battleModuleUIScript.StartBattle(_battleCell);
        FirstRecalculation();
    }
    private void OnDestroy()
    {
        ServiceRegistry.WorkWithController<GameController>().oneSecondPassed -= OtherTimerController;
    }
    private void OtherTimerController()
    {
        if (PauseScript.CurrentGameState != GameState.Play) { return; }
        _timer++;
        if (_timer >= timerForOneTurn)
        {
            _timer = 0;
            OneStep();
        }
    }
    public void SetSquads(List<AbstractSquad> squadDefence, List<AbstractSquad> squadAttack)
    {
        _squadDefence = squadDefence.ToList();
        _squadAttack = squadAttack.ToList();
    }
    private void OneStep()
    {
        RecalculateParameters();

        KillSomePeople();

        CheckBattleOver();

        AddBattleSpeedToEveryOne();

        AddNewSquadFromBuffer();
        RemoveSquadsFromBuffer();
    }
    private void FirstRecalculation()
    {
        _ratioAttackSide = 0; _ratioDefenceSide = 0;

        if (_squadDefence.Count != 0)
        {
            foreach (var squad in _squadDefence)
            {
                _battleModuleUIScript.TryAddSquad(_battleCell,squad,false); 
                _ratioDefenceSide += squad.GetAllProtection();
            }
        }
        else { _ratioDefenceSide = 0.0001f; }

        foreach (var squad in _squadAttack)
        {
            _battleModuleUIScript.TryAddSquad(_battleCell, squad, true);
            _ratioAttackSide += squad.GetAllAttack();
        }

        _allParameters = _ratioAttackSide + _ratioDefenceSide;

        UpdateBar_UI();

        GetSpeedBattle(_ratioAttackSide/_allParameters);

        if(_squadDefence.Count != 0)
        {
            foreach(var squad in _squadDefence) { _squadSpeedBattle.Add(squad, _currentSpeedBattle); }
        }
        foreach (var squad in _squadAttack) { _squadSpeedBattle.Add(squad, _currentSpeedBattle); }

        _battleModuleUIScript.UpdateAllParameters_UI(_battleCell,(int)_ratioAttackSide, (int)_ratioDefenceSide);
    }
    private void RecalculateParameters()
    {
        _ratioAttackSide = 0; _ratioDefenceSide = 0;

        if (_squadDefence.Count != 0)
        {
            foreach (var squad in _squadDefence)
            {
                if (squad.IsDead) continue;

                _battleModuleUIScript.TryAddSquad(_battleCell, squad,false);
                _ratioDefenceSide += squad.GetAllProtection();
            }
        }
        else { _ratioDefenceSide = 0.0001f; }

        foreach(var squad in _squadAttack)
        {
            if (squad.IsDead) continue;

            _battleModuleUIScript.TryAddSquad(_battleCell, squad, true);
            _ratioAttackSide += squad.GetAllAttack();
        }

        _allParameters = _ratioAttackSide + _ratioDefenceSide;

        UpdateBar_UI();

        _battleModuleUIScript.UpdateAllParameters_UI(_battleCell, (int)_ratioAttackSide, (int)_ratioDefenceSide);
    }
    public void SetBattleCell(GameObject battleCell)
    {
        _battleCell = battleCell;

        transform.position = new Vector3(battleCell.transform.position.x,transform.position.y, battleCell.transform.position.z);
    }
    private void AddNewSquadFromBuffer()
    {
        foreach (var squad in squadsNeedAddLater.Keys)
        {
            squad.Action = SquadActions.Battle;

            if (squadsNeedAddLater[squad] == BattleController.BattleSide.Attack) { _squadAttack.Add(squad); _battleModuleUIScript.TryAddSquad(_battleCell, squad, true); }
            else { _squadDefence.Add(squad); _battleModuleUIScript.TryAddSquad(_battleCell, squad, false); }

            _squadSpeedBattle.Add(squad, _currentSpeedBattle);

            Debug.Log($"Добавил {squad}");
        }

        squadsNeedAddLater.Clear();
    }
    private void RemoveSquadsFromBuffer()
    {
        foreach(var squad in squadsNeedToDelete) { _squadAttack.Remove(squad); _squadSpeedBattle.Remove(squad); _battleModuleUIScript.TryRemoveSquad(_battleCell, squad); }

        squadsNeedToDelete.Clear();
    }
    private void KillSomePeople()
    {
        float rationToKillDefence = _ratioAttackSide / _allParameters;
        float rationToKillAttack = 1 - _ratioAttackSide / _allParameters;

        CalculateDamageForOneSide(_squadAttack, rationToKillDefence, rationToKillAttack, BattleController.BattleSide.Attack);
        CalculateDamageForOneSide(_squadDefence, rationToKillDefence, rationToKillAttack, BattleController.BattleSide.Defence);
    }

    private void CalculateDamageForOneSide(List<AbstractSquad> squads, float rationToKillDefence, float rationToKillAttack, BattleController.BattleSide battleSide)
    {
        foreach (var squad in squads)
        {
            if (squad.IsDead) continue;

            int count = squad.PeopleCount;

            if(battleSide == BattleController.BattleSide.Attack) count = (int)(count * _squadSpeedBattle[squad] * rationToKillAttack);
            else count = (int)(count * _squadSpeedBattle[squad] * rationToKillDefence);

            count = Mathf.Max(1, count);

            squad.KillSomePerson(count);

            if (squad.PeopleCount < minCountOfPeopleInSquad)
            {
                _battleModuleUIScript.TryRemoveSquad(_battleCell, squad);

                squad.IsDead = true;
                continue;
            }

            squad.CalculateParam();
        }
    }
    private void CheckBattleOver() 
    {
        if (_squadAttack.Count == GetCountOfDeadSquads(_squadAttack)) { battleIsOver.Invoke(false); _battleModuleUIScript.BattleIsOver(_battleCell); Destroy(gameObject); }
        if(_squadDefence.Count == GetCountOfDeadSquads(_squadDefence)) { battleIsOver.Invoke(true); _battleModuleUIScript.BattleIsOver(_battleCell); Destroy(gameObject); }
    }
    private int GetCountOfDeadSquads(List<AbstractSquad> squads)
    {
        int count = 0;
        foreach(var squad in squads)
        {
            if (squad.IsDead) { count++; }
        }

        return count;
    }

    private void GetSpeedBattle(float rationAttackDefence)
    {
        if(rationAttackDefence > 0 && rationAttackDefence <= 0.15f) { _currentSpeedBattle = 0.1f * Mathf.Pow(2,4);}
        if(rationAttackDefence > 0.15f && rationAttackDefence <= 0.25f) { _currentSpeedBattle = 0.1f * Mathf.Pow(2,3);}
        if (rationAttackDefence > 0.25f && rationAttackDefence <= 0.35f) { _currentSpeedBattle = 0.1f * Mathf.Pow(2, 2); }
        if (rationAttackDefence > 0.35f && rationAttackDefence <= 0.45f) { _currentSpeedBattle = 0.1f * Mathf.Pow(2, 1); }

        if (rationAttackDefence > 0.45f && rationAttackDefence <= 0.55f) { _currentSpeedBattle = 0.1f; }

        if (rationAttackDefence > 0.55f && rationAttackDefence <= 0.65f) { _currentSpeedBattle = 0.1f * Mathf.Pow(2, 1); }
        if (rationAttackDefence > 0.65f && rationAttackDefence <= 0.75f) { _currentSpeedBattle = 0.1f * Mathf.Pow(2, 2); }
        if (rationAttackDefence > 0.75f && rationAttackDefence <= 0.85f) { _currentSpeedBattle = 0.1f * Mathf.Pow(2, 3); }
        if (rationAttackDefence > 0.85f && rationAttackDefence <= 0) { _currentSpeedBattle = 0.1f * Mathf.Pow(2, 4); }

        _currentSpeedBattle = Mathf.Clamp(_currentSpeedBattle, 0f, 1f);
    }
    private void AddBattleSpeedToEveryOne()
    {
        foreach (var squad in _squadAttack)
        {
            float newSpeed = _squadSpeedBattle[squad] += 0.1f;

            newSpeed = Mathf.Clamp(newSpeed, 0f, 1f);

            _squadSpeedBattle[squad] = newSpeed;
        }
        foreach (var squad in _squadDefence)
        {
            float newSpeed = _squadSpeedBattle[squad] += 0.1f;

            newSpeed = Mathf.Clamp(newSpeed, 0f, 1f);

            _squadSpeedBattle[squad] = newSpeed;
        }
    }
    public void AddSquadsToBuffer(List<AbstractSquad> squads, BattleController.BattleSide battleSide)
    {
        foreach(var squad in squads)
        {
            squadsNeedAddLater.Add(squad, battleSide);
        }
    }
    public void RemoveSquadsFromBuffer_Later(List<AbstractSquad> squads)
    {
        foreach(var squad in squads)
        {
            squadsNeedToDelete.Add(squad);
        }
    }

    private void UpdateBar_UI()
    {
        _attackImage.fillAmount = _ratioAttackSide / _allParameters;
        _defenceImage.fillAmount = _ratioDefenceSide / _allParameters;
    }
}
