using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackDirection
{
    private int attackMinLimit = 0;

    private HeadquartersBuild responsibleHeadquarters;

    private HeadquartersBuild purposeHeadquarters;

    private GameObject cellWithRescHead, cellWithPurpHead;

    private int attackPower;

    private List<GameObject> cellIncluded = new();

    private Dictionary<GameObject, List<GameObject>> cellToCellFronts = new();

    public AttackDirection(HeadquartersBuild responsibleHeadquarters, HeadquartersBuild purposeHeadquarters, int attackMinLimit = 200)
    {
        this.attackMinLimit = attackMinLimit;

        this.responsibleHeadquarters = responsibleHeadquarters;
        this.purposeHeadquarters = purposeHeadquarters;

        cellWithRescHead = responsibleHeadquarters.CellWithThisBuild.gameObject;
        cellWithPurpHead = purposeHeadquarters.CellWithThisBuild.gameObject;

        GetCellsAttackDir();

        RecalculateAttackPower();
    }

    public void Step()
    {
        if(attackPower >= attackMinLimit)
        {
            Debug.LogError("We can start attack action");

            AttackAction();
        }
    }

    private void AttackAction()
    {
        var selectedCellToAttack = cellToCellFronts.Keys.ToList()[UnityEngine.Random.Range(0, cellToCellFronts.Keys.Count)];
        Debug.LogError($"Selected cell to attack {selectedCellToAttack}");


        var parameters = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(selectedCellToAttack);

        var squadsOnCell = parameters.GetParameter<CellSquadsOnArea>().squadsOnCell;
        int attackSummaryOnCell = 0;

        foreach (var squad in squadsOnCell) { attackSummaryOnCell += squad.GetAllAttack(); }

        if(attackSummaryOnCell >= 0)//Action of attack
        {
            Debug.LogError($"Want to attack {attackSummaryOnCell}");

            ServiceRegistry.WorkWithController<BattleController>().TryStartBattle(cellToCellFronts[selectedCellToAttack][UnityEngine.Random.Range(0, cellToCellFronts[selectedCellToAttack].Count)],
                selectedCellToAttack, parameters.GetParameter<CellSquadsOnArea>().squadsOnCell);
        }
        else //Action of move
        {
            Debug.LogError($"Need more squads {attackSummaryOnCell}");
        }
    }
    private void GetFrontCells()
    {
        List<CellParametersHandler> frontPlayer = new(), frontBot = new();

        foreach (var cell in cellIncluded) 
        {
            var parameters = ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell);

            if (parameters.GetParameter<CellArea>().IsFrontCell)
            {
                if(parameters.GetParameter<CellArea>().Side == SideEnum.Enemys) { frontBot.Add(parameters); }
                if(parameters.GetParameter<CellArea>().Side == SideEnum.Allies) { frontPlayer.Add(parameters); }
            }
        }

        foreach(var parameters in frontBot)
        {
            cellToCellFronts[parameters.GetCellWorkWith()] = new();

            foreach(var playerParam in frontPlayer)
            {
                if (parameters.GetParameter<CellArea>().IsCellNeighbor(playerParam.GetCellWorkWith()))
                {
                    cellToCellFronts[parameters.GetCellWorkWith()].Add(playerParam.GetCellWorkWith());

                    Debug.LogError($"Im {parameters.GetCellWorkWith()} and i can attack {playerParam.GetCellWorkWith()}");
                }
            }
        }
    }
    private void GetCellsAttackDir(int angle = 60)//expensive 
    {
        Vector3 direction = (cellWithPurpHead.transform.position - cellWithRescHead.transform.position).normalized;

        Collider[] findedCells =  
        Physics.OverlapSphere(cellWithRescHead.transform.position, Vector3.Distance(cellWithPurpHead.transform.position,
            cellWithRescHead.transform.position));

        List<GameObject> result = new List<GameObject>();

        float halfAngle = angle / 2;

        foreach (Collider findedCell in findedCells) 
        {
            Vector3 targetDir = (findedCell.transform.position - cellWithRescHead.transform.position).normalized;

            float targetAngle = Vector3.Angle(direction, targetDir);
            
            if(targetAngle <= halfAngle)
            {
                cellIncluded.Add(findedCell.gameObject);

                findedCell.transform.position += new Vector3(0f, 100f, 0f);
            }
        }


        GetFrontCells();
    }
    public void RecalculateAttackPower(GameObject cellInitiator = null)
    {
        if(cellInitiator == null)
        {
            foreach(var cell in cellIncluded)
            {
                foreach(AbstractSquad squad in ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(cell)
                    .GetParameter<CellSquadsOnArea>().squadsOnCell)
                {
                    if (squad.Side != SideEnum.Enemys) break;

                    attackPower += squad.Attack;
                }
            }

            Debug.LogError(attackPower);
        }
    }
}
