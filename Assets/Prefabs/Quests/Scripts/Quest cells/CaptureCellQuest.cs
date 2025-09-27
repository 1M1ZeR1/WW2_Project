using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureCellQuest : AbstractQuest
{
    private int difficultyToRewardRation;

    protected GameObject _cellToNeedCapture;

    private DateTime dateToInvoke;

    public CaptureCellQuest(int hours, GameObject cellToNeedCapture, int difficulty, int difficultyToRewardRation, DateTime dateToInvoke, string[] allText)
    {
        SetDeadLine(TimeControllerScript.GetCurrentDateTime().AddHours(hours));

        _cellToNeedCapture = cellToNeedCapture;

        SetDifficulty(difficulty);

        this.difficultyToRewardRation = difficultyToRewardRation;

        RewardConstructor(new List<ResourcesEnum>()
        {
            ResourcesEnum.None,
            ResourcesEnum.Weapon,
            ResourcesEnum.Building,
            ResourcesEnum.Transport,
            ResourcesEnum.People
        });

        SetTitle(allText[0]);
        SetHeader(allText[1]);
        SetDiscription(allText[2]);

        this.dateToInvoke = dateToInvoke;
    }
    public override bool CheckToInvokeQuest()
    {
        if (TimeControllerScript.GetCurrentDateTime() == dateToInvoke) { return true; }
        else return false;
    }
    public bool IsCellCaptured(GameObject cell)
    {
        if(cell == _cellToNeedCapture) { return true; }
        else { return false; }
    }
    private void RewardConstructor(List<ResourcesEnum> blockedRewardResources)
    {
        int difficulty = GetDifficulty();

        int countOfResource = difficultyToRewardRation * difficulty;

        int[] resourcesCount = new int[5];

        for (int i = 0; i < resourcesCount.Length; i++)
        {
            if (blockedRewardResources[i] != ResourcesEnum.None) { resourcesCount[i] = 0; continue; }

            int count = (int)UnityEngine.Random.Range(0, countOfResource / 0.7f);

            resourcesCount[i] = count;
            countOfResource -= count;
        }

        if (countOfResource != 0)
        {
            int countOfNoneBlockedTypes = 0;
            foreach (ResourcesEnum type in blockedRewardResources)
            {
                if (type == ResourcesEnum.None) { countOfNoneBlockedTypes++; }
            }
            int partRemaining = countOfResource / countOfNoneBlockedTypes;

            for (int i = 0; i < resourcesCount.Length; i++)
            {
                if (blockedRewardResources[i] != ResourcesEnum.None) {continue; }

                resourcesCount[i] += partRemaining;
            }
        }

        SetReward(resourcesCount[0], resourcesCount[1], resourcesCount[2], resourcesCount[3], resourcesCount[4]);
    }
}
