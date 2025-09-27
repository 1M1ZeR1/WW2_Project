using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CellTypeScript : MonoBehaviour
{
    [Header("Главный контроллер")]
    [SerializeField] private GameObject gameControllerObject;
    private GameController gameControllerScript;

    private AbstractCell cellType;

    private string cellName;

    [SerializeField]private float height;

    protected List<AbstractBuffs> _buffs = new List<AbstractBuffs>();

    private float cost;

    protected bool _hasEventBuff = false;

    private void Start()
    {
        gameControllerObject.TryGetComponent(out gameControllerScript);
    }

    public void SetTypeCell(CellTypes_enum cellType)
    {
        if (cellType == CellTypes_enum.Plain)
        {
            this.cellType = new PlainCell(20);
            cost = 0.5f;
            _buffs.Add(this.cellType.GetBuff());
        }
        if(cellType == CellTypes_enum.Forest)
        {
            this.cellType = new ForestCell(30);
            cost = 1f;
            _buffs.Add(this.cellType.GetBuff());
        }
        if(cellType == CellTypes_enum.City)
        {
            this.cellType = new CityCell(10);
            cost = 10f;
            _buffs.Add(this.cellType.GetBuff());
        }
        if(cellType == CellTypes_enum.River)
        {
            this.cellType = new RiverCell();
            cost = 5f;
        }
        if (cellType == CellTypes_enum.Beach)
        {
            this.cellType = new BeachCell();
            cost = 10f;
        }

    }
    public void AddBuffWithTimer(AbstractBuffs buff)
    {
        _buffs.Add(buff);
        UpdateBuffsForSquad();

        if(buff.GetType() == typeof(UnReachableBuff)) 
        {
            SetHeight(GetHeight() + 1000);
        }

        buff.RequestToClear += BuffRemoverListener;
    }
    private void BuffRemoverListener(AbstractBuffs buff)
    {
        _hasEventBuff = false;

        _buffs.Remove(buff);

        if (buff.GetType() == typeof(UnReachableBuff))
        {
            SetHeight(GetHeight() - 1000);
        }
    }
    public void AddBuffToCell(AbstractBuffs buff)
    {
        if(buff == null) { return;}
        _buffs.Add(buff);
        UpdateBuffsForSquad();
    }
    public void RemoveBuffFromCell(AbstractBuffs buff)
    {
        RemoveUpdateBuffsForSquad(buff);
        _buffs.Remove(buff);
    }
    public string GetBuffCell()
    {
        return cellType.GetBuffDiscription();
    }
    public string GetTypeCell()
    {
        return cellType.GetTypeCell();
    }

    public float GetHeight()
    {
        return height;
    }
    public void SetHeight(float height)
    {
        this.height = height;
    }
    public bool IsRiver()
    {
        if(cellType.GetType() == typeof(RiverCell))
        {
            return true;
        }
        else { return false; }
    }
    public bool IsBeach()
    {
        if (cellType.GetType() == typeof(BeachCell))
        {
            return true;
        }
        else { return false; }
    }
    public bool IsLake()
    {
        if (cellType.GetType() == typeof(LakeCell))
        {
            return true;
        }
        else { return false; }
    }
    public float GetCost() { return cost; }

    public void SetToSquadBuffs(AbstractSquad squad)
    {
        foreach(var buff in _buffs)
        {
            squad.AddBuff(buff);
        }
    }
    public void RemoveFromSquadBuffs(AbstractSquad squad)
    {
        foreach (var buff in _buffs)
        {
            squad.RemoveBuff(buff);
        }
    }
    private void UpdateBuffsForSquad()
    {
        foreach(var squad in gameControllerScript.GetAllSquadsOnCell(gameObject))
        {
            var allBuffs = squad.GetAllSquadBuffs();

            foreach(var cellBuff in _buffs)
            {
                if (!allBuffs.Contains(cellBuff) && cellBuff!=null)
                {
                    squad.AddBuff(cellBuff);
                }
            }
        }
    }
    private void RemoveUpdateBuffsForSquad(AbstractBuffs buff)
    {
        foreach (var squad in gameControllerScript.GetAllSquadsOnCell(gameObject))
        {
            if (squad.GetAllSquadBuffs().Contains(buff)) { squad.RemoveBuff(buff); }
        }
    }
    public List<AbstractBuffs> GetAllCellBuffs() { return  _buffs; }

    public void SetCellName(string name) { cellName = name; }
    public string GetCellName() {  return cellName; }

    public void SetBasicCellName() { cellName = cellType.GetTypeCell(); }

    public bool IsWithEventBuff() { return _hasEventBuff; }
    public void SetWithEventBuff(bool state) { _hasEventBuff = state; }
    public void AssetHelper(SideEnum sideEnum)
    {
        ServiceRegistry.WorkWithController<UnitsSpawner>().SpawnSquadOnCell(sideEnum, gameObject);
    }
}
