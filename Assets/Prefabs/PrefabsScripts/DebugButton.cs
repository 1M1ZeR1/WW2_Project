using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DebugButton : MonoBehaviour
{
    protected GameObject selectedCell;

    [SerializeField] private SquadEnum choisedSquad = SquadEnum.None;

    private Dictionary<SquadEnum, string> dictionary = new()
    {
        {SquadEnum.Tanks,"TanksSquad" },
        {SquadEnum.Scouts,"ScoutsSquad" },
        {SquadEnum.Artillery,"ArtillerySquad" },
        {SquadEnum.Engineer,"EnginnersSquad" },
        {SquadEnum.Infantry,"InfantrySquad" }
    };


    public void SpawnUnit()
    {
        if(selectedCell != null)
        {
            ServiceRegistry.WorkWithController<UnitsSpawner>().SpawnSquadByType(dictionary[choisedSquad], SideEnum.Allies);
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

    public AbstractSquad SpawnSquadByType(string id, SideEnum side)
    {
        AbstractSquad newSquad;

        newSquad = (AbstractSquad)ServiceRegistry.WorkWithService<ObjectFactory_Squads>().CreateObject(id);

        newSquad.Side = side;

        SquadData squadData = (SquadData)ServiceRegistry.WorkWithService<DataHolder>().GetDataFromHolder(id);

        newSquad.Initialize(squadData.squadSpeedOfMovement, UnityEngine.Random.Range(15, 21), squadData.transports[UnityEngine.Random.Range(0, squadData.transports.Count)], squadData.weapons[UnityEngine.Random.Range(0, squadData.weapons.Count)]);

        return newSquad;
    }

    public AbstractSquad SpawnSquadOnCell(string id,SideEnum side,GameObject cell)
    {
        var squad = SpawnSquadByType(id,side);

        ServiceRegistry.WorkWithController<GameController>().AddSquadInDictionary(squad, cell);
        ServiceRegistry.WorkWithController<GameController>().OnlyCaptureCell(side, cell);

        ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell).GetParameter<CellSquadsOnArea>().squadsOnCell.Add(squad);

        return squad;
    }

    public Action GetSummonAction(SideEnum side, GameObject cell)
    {
        return null;
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
