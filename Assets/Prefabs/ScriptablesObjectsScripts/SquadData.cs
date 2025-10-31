using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;


[CreateAssetMenu(menuName = "Data objects/Squad data")]
[Preserve]
public class SquadData : ScriptableObject
{
    public Sprite squadSprite;

    public string id;
    public string squadName;

    [Header("Weapons and transports for this squad")]
    public List<SquadWeapon> weapons;
    public List<SquadTransport> transports;

    [Header("Squad cost of people(By 1 human)")]
    public int squadCostPeople;

    [Header("Squad need academy to train")]
    public bool needAcademy = false;

    [Header("Squad parameters")]

    [Header("Squad speed")] public int squadSpeedOfMovement;
}
