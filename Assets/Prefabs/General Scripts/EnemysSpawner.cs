using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemysSpawner : MonoBehaviour
{
    private GameObject baseCell;

    [SerializeField] private int countOfDummy;
    protected int countOfEnemysSquads;

    [SerializeField] private int time;
    protected float _timer;

    [SerializeField] private GameObject gameControllerObject;
    private GameController gameController;

    [Header("Контроллер противника")]
    [SerializeField] private GameObject enemysControllerObject;
    private EnemysController enemysControllerScript;

    private void Start()
    {
        gameControllerObject.TryGetComponent<GameController>(out gameController);
        enemysControllerObject.TryGetComponent(out enemysControllerScript);

        gameController.oneSecondPassed += OtherTimerController;
    }
    private void OtherTimerController()
    {
        if (PauseScript.CurrentGameState != GameState.Play) { return; }

        _timer++;
        if(_timer >= time)
        {
            _timer = 0;
            SpawnUnit();
        }
    }
    private void SpawnUnit()
    {
        if (countOfEnemysSquads >= countOfDummy) { return; }

        countOfEnemysSquads++;

        if (Random.Range(0, 2) == 0)
        {
            InfantrySquad squad = new InfantrySquad(3, Random.Range(10, 25), RandomTransport(), RandomWeapon());
            squad.Side = SideEnum.Enemys;

            enemysControllerScript.AddBot(squad,gameObject);
            gameController.AddSquadInDictionary(squad, gameObject);
            return;
        }
        else
        {
            EngineerSquad squad = new EngineerSquad(3, Random.Range(10, 25),RandomTransport(),RandomWeapon());
            squad.Side = SideEnum.Enemys;

            enemysControllerScript.AddBot(squad, gameObject);
            gameController.AddSquadInDictionary(squad, gameObject);
            return;
        }
    }
    private SquadTransport RandomTransport() 
    {
        switch (Random.Range(0, 3))
        {
            case 0:return SquadTransport.ByFoot;
            case 1:return SquadTransport.Horses;
            case 2:return SquadTransport.Cars;
        }
        return SquadTransport.None;
    }
    private SquadWeapon RandomWeapon() 
    {
        switch (Random.Range(0, 3))
        {
            case 0: return SquadWeapon.MachineHun;
            case 1: return SquadWeapon.Rifle;
            case 2: return SquadWeapon.SniperRifle;
        }
        return SquadWeapon.None;
    }
    public void SetBaseCell(GameObject cell) { baseCell = cell; }
}
