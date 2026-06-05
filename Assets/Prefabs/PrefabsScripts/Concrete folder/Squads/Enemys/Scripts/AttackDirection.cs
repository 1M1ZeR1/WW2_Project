using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDirection
{
    private HeadquartersBuild responsibleHeadquarters;

    private HeadquartersBuild purposeHeadquarters;

    private GameObject cellWithRescHead, cellWithPurpHead;

    private int attackPower;

    private List<GameObject> cellIncluded = new();

    public AttackDirection(HeadquartersBuild responsibleHeadquarters, HeadquartersBuild purposeHeadquarters)
    {
        this.responsibleHeadquarters = responsibleHeadquarters;
        this.purposeHeadquarters = purposeHeadquarters;

        cellWithRescHead = responsibleHeadquarters.CellWithThisBuild.gameObject;
        cellWithPurpHead = purposeHeadquarters.CellWithThisBuild.gameObject;

        GetCellsAttackDir();

        RecalculateAttackPower();
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
    }
    public void RecalculateAttackPower(GameObject cellInitiator = null)
    {
        if(cellInitiator == null)
        {
            foreach(var cell in cellIncluded)
            {
                Debug.LogError(cell.name);

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
