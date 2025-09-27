using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SecondaryQuestsTypes
{
    None,
    SaveSquad,
    CaptureCell
}
public abstract class AbstractQuest
{
    private DateTime deadLine;

    private int difficulty;

    private string title;

    private string header;
    private string discription;

    private Dictionary<ResourcesEnum, int> reward;


    public void SetTitle(string title) { this.title = title; }
    public string GetTitle() { return title; }
    public void SetDeadLine(DateTime deadLine) { this.deadLine = deadLine; }
    public DateTime GetDeadLine() { return deadLine; }
    public void SetDifficulty(int difficulty) {  this.difficulty = difficulty; }
    public int GetDifficulty() { return difficulty;}

    public void SetHeader(string header) { this.header = header;}
    public string GetHeader() { return header;}
    public void SetDiscription(string discription) {  this.discription = discription; }
    public string GetDiscription() {  return discription; }

    public abstract bool CheckToInvokeQuest();

    public virtual void SetReward(int influence, int buildingResource, int weaponResource, int transportResource, int peopleResource) 
    {
        reward = new Dictionary<ResourcesEnum, int>
        {
            { ResourcesEnum.Influence, influence },
            { ResourcesEnum.Building, buildingResource },
            { ResourcesEnum.Weapon, weaponResource },
            { ResourcesEnum.Transport, transportResource },
            { ResourcesEnum.People, peopleResource }
        };
    }
    public virtual Dictionary<ResourcesEnum, int> GetReward() { return reward;}
    public virtual string GetDiscriptionDeadLine()
    {
        return $" {deadLine.Hour}:00 {deadLine.Day}.{deadLine.Month}.{deadLine.Year} г.";
    }
    public virtual List<string> GetDiscriptionReward()
    {
        var reward = GetReward();

        List<string> rewardList = new List<string>();

        if (reward[ResourcesEnum.Influence] != 0) { rewardList.Add($"Влияние–{reward[ResourcesEnum.Influence]}"); }
        if (reward[ResourcesEnum.Building] != 0) { rewardList.Add($"Стройматериалы–{reward[ResourcesEnum.Building]}"); }
        if (reward[ResourcesEnum.Weapon] != 0) { rewardList.Add($"Вооружение–{reward[ResourcesEnum.Weapon]}"); }
        if (reward[ResourcesEnum.Transport] != 0) { rewardList.Add($"Транспорт–{reward[ResourcesEnum.Transport]}"); }
        if (reward[ResourcesEnum.People] != 0) { rewardList.Add($"Люди–{reward[ResourcesEnum.People]}"); }

        return rewardList;
    }
}
