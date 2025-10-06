using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DebugButton : MonoBehaviour
{
    protected GameObject selectedCell;

    [SerializeField] private SquadEnum choisedSquad = SquadEnum.None;


    public void SpawnUnit()
    {
        if(selectedCell != null)
        {
            ServiceRegistry.WorkWithController<UnitsSpawner>().SpawnSquadByType(choisedSquad, SideEnum.Allies);
        }
    }
    public void SetSelectedCell(GameObject cell)
    {
        selectedCell = cell;
    }
}

public class UnitsSpawner
{

    public UnitsSpawner()
    {

    }

    public AbstractSquad SpawnSquadByType(SquadEnum squadType, SideEnum side)
    {
        AbstractSquad newSquad;

        switch (squadType)
        {
            case SquadEnum.Infantry: newSquad = new InfantrySquad(3, UnityEngine.Random.Range(10, 25), RandomTransport(), RandomWeapon());break;
            case SquadEnum.Engeener: newSquad = new EngineerSquad(3, UnityEngine.Random.Range(10, 25), RandomTransport(), RandomWeapon());break;
            case SquadEnum.Scouts: newSquad = new ScoutSquad(3, UnityEngine.Random.Range(2, 6), RandomTransport(), RandomWeapon()); break;
            case SquadEnum.Tanks: newSquad = new TankSquad(4, 20, SquadTransport.Tank, SquadWeapon.Tank); break;
            case SquadEnum.Artillery: newSquad = new ArtillerySquad(2, 15, SquadTransport.Artillery, SquadWeapon.Artillery); break;

            default:throw new ArgumentOutOfRangeException();
        }

        newSquad.Side = side;

        return newSquad;
    }

    public AbstractSquad SpawnSquadOnCell(SideEnum side,GameObject cell)
    {
        var squad = SpawnSquadByType((SquadEnum)UnityEngine.Random.Range(1,3),side);

        ServiceRegistry.WorkWithController<GameController>().AddSquadInDictionary(squad, cell);
        //ServiceRegistry.WorkWithController<GameController>().OnlyCaptureCell(side, cell);

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellSquadsOnArea>().squadsOnCell.Add(squad);

        return squad;
    }

    public Action GetSummonAction(SideEnum side, GameObject cell)
    {
        return () => SpawnSquadOnCell(side, cell);
    }

    private SquadTransport RandomTransport()
    {
        switch (UnityEngine.Random.Range(0, 3))
        {
            case 0: return SquadTransport.ByFoot;
            case 1: return SquadTransport.Horses;
            case 2: return SquadTransport.Cars;
        }
        return SquadTransport.None;
    }
    private SquadWeapon RandomWeapon()
    {
        switch (UnityEngine.Random.Range(0, 3))
        {
            case 0: return SquadWeapon.MachineHun;
            case 1: return SquadWeapon.Rifle;
            case 2: return SquadWeapon.SniperRifle;
        }
        return SquadWeapon.None;
    }
}
