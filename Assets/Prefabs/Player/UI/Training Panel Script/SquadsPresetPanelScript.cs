using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SquadsPresetPanelScript : MonoBehaviour
{
    public enum SortMode
    {
        None,
        Favorite,
        Count_People,
        Count_Weapon,
        Count_Transport
    }
    private SortMode sortMode = SortMode.None;

    [SerializeField] private Button[] sortButtons;

    [SerializeField] private GameObject squadPresetPanel;
    [SerializeField] private Transform scrollViewCont;

    private List<SquadPresetPanel> latestPresets = new List<SquadPresetPanel>();

    [SerializeField] private int maxCountOfLatestSaved = 5;
    private int countOfLatestSaved = 0;
    [SerializeField] private CreatingPanelScript creatingPanelScript;

    public void AddToPresets_Latest(AbstractSquad squad, SquadPresetObject squadPresetObject)
    {
        SquadPresetPanel newSquadPresetPanel = new SquadPresetPanel(squad, squadPresetObject,
            RemoveSuperfluousElemets, InvokeIncriment, InvokeDisIncriment);

        if (!latestPresets.Contains(newSquadPresetPanel)) 
        {
            countOfLatestSaved++;

            RemoveSuperfluousElemets();

            latestPresets.Add(newSquadPresetPanel);

            GameObject newCreatedPanel = Instantiate(squadPresetPanel,scrollViewCont);
            newCreatedPanel.SetActive(true);

            newSquadPresetPanel.SetCurrentGameObject(newCreatedPanel,creatingPanelScript);
        }
    }
    
    public void RemoveSuperfluousElemets()
    {
        if (countOfLatestSaved == maxCountOfLatestSaved)
        {
            var firstBadPreset = latestPresets.FirstOrDefault(obj => !obj.IsFavorites());
            if (firstBadPreset != null) { latestPresets.Remove(firstBadPreset); firstBadPreset.InvokeDestroy(); }
        }
    }

    public void SetSortMode(int index)
    {
        ResetInteractableStatusButtons();

        sortButtons[index-1].interactable = false;
        sortMode = (SortMode)index;

        SortMethod();
    }

    public void ShowList()
    {
        sortMode = SortMode.None;
        ResetInteractableStatusButtons();

        SortMethod();
    }
    private void ResetInteractableStatusButtons()
    {
        foreach (var button in sortButtons) { button.interactable = true; }
    }

    private void SortMethod()
    {
        if(sortMode == SortMode.Favorite) 
        {
            var sortedPresets = latestPresets.OrderByDescending(item => item.IsFavorites()).ToList();

            for (int i = 0; i < sortedPresets.Count; i++)
            {
                sortedPresets[i].SetSortedIndex(i);
            }
        }
        else
        {
            Dictionary<SquadPresetPanel,int> classToCount = new Dictionary<SquadPresetPanel,int>();
            foreach(SquadPresetPanel item in latestPresets){classToCount.Add(item, item.GetResourceCount(sortMode));}

            var sortedPresets = classToCount.OrderBy(pair => pair.Value).Select(pair => pair.Key).ToList();

            for (int i = 0; i < classToCount.Count; i++)
            {
                sortedPresets[i].SetSortedIndex(i);
            }
        }
    }

    public void InvokeIncriment() { countOfLatestSaved++; Debug.Log(countOfLatestSaved); }
    public void InvokeDisIncriment() { countOfLatestSaved--; Debug.Log(countOfLatestSaved); }



    private class SquadPresetPanel
    {
        private GameObject currentGameObjectPanel;

        private string squadName;
        private SquadPresetObject squadPresetObject;

        private SquadEnum squadType;

        protected bool _isFavorites = false;

        private Action swipeFavoritesMode;
        private Action invokeIncriment, invokeDisIncriment;

        public SquadPresetPanel(AbstractSquad squad, SquadPresetObject squadPresetObject,
            Action removeSuperfluousElemets, Action invokeIncriment, Action invokeDisIncriment) 
        {
            squadName = squad.Name;
            this.squadPresetObject = squadPresetObject;

            squadType = squad.Type;

            swipeFavoritesMode = removeSuperfluousElemets;
            this.invokeIncriment = invokeIncriment;
            this.invokeDisIncriment = invokeDisIncriment;
        }

        public void SetCurrentGameObject(GameObject newPanel,CreatingPanelScript creatingPanelScript) 
        { 
            currentGameObjectPanel = newPanel; 
            ConfiguratePanel(); 

            EventTrigger.Entry newEntry = new EventTrigger.Entry();
            newEntry.eventID = EventTriggerType.PointerClick;

            newEntry.callback.AddListener((eventData) => { creatingPanelScript.AddSquadToTrainingWithPreset(squadPresetObject,squadType); });

            newPanel.GetComponent<EventTrigger>().triggers.Add(newEntry);


            EventTrigger.Entry newFavoritesEntry = new EventTrigger.Entry();
            newFavoritesEntry.eventID = EventTriggerType.PointerClick;

            newFavoritesEntry.callback.AddListener((eventData) =>
            {
                if (_isFavorites) { _isFavorites = false;invokeIncriment.Invoke();swipeFavoritesMode.Invoke(); }
                else { _isFavorites = true; invokeDisIncriment.Invoke(); }
            });

            newPanel.transform.GetChild(0).GetComponent<EventTrigger>().triggers.Add(newFavoritesEntry);
        }
        private void ConfiguratePanel()
        {
            currentGameObjectPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = squadName;
            currentGameObjectPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{squadPresetObject.GetCountOfPeople()} человек";

            Transform resourceCostPanel = currentGameObjectPanel.transform.GetChild(3);
            resourceCostPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = squadPresetObject.GetResourceCost_ByType(ResourcesEnum.People).ToString();
            resourceCostPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = squadPresetObject.GetResourceCost_ByType(ResourcesEnum.Weapon).ToString();
            resourceCostPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = squadPresetObject.GetResourceCost_ByType(ResourcesEnum.Transport).ToString();
        }

        public int GetResourceCount(SortMode sortMode) 
        {
            switch (sortMode) 
            {
                case SortMode.Count_People:return squadPresetObject.GetResourceCost_ByType(ResourcesEnum.People);
                case SortMode.Count_Transport:return squadPresetObject.GetResourceCost_ByType(ResourcesEnum.Transport);
                case SortMode.Count_Weapon:return squadPresetObject.GetResourceCost_ByType(ResourcesEnum.Weapon);
            }

            return 0;
        }

        public bool IsFavorites() { return _isFavorites; }
        public void SetSortedIndex(int index)
        {
            currentGameObjectPanel.transform.SetSiblingIndex(index);
        }

        public override bool Equals(object obj)
        {
            if (obj is SquadPresetPanel other) return squadName == other.squadName &&
                    squadPresetObject.GetResourceCost_ByType(ResourcesEnum.People) == other.squadPresetObject.GetResourceCost_ByType(ResourcesEnum.People) &&
                    squadPresetObject.GetResourceCost_ByType(ResourcesEnum.Weapon) == other.squadPresetObject.GetResourceCost_ByType(ResourcesEnum.Weapon) &&
                    squadPresetObject.GetResourceCost_ByType(ResourcesEnum.Transport) == other.squadPresetObject.GetResourceCost_ByType(ResourcesEnum.Transport);
            return false;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(squadName,
                squadPresetObject.GetResourceCost_ByType(ResourcesEnum.People), 
                squadPresetObject.GetResourceCost_ByType(ResourcesEnum.Weapon), 
                squadPresetObject.GetResourceCost_ByType(ResourcesEnum.Transport));
        }

        public void InvokeDestroy() { Destroy(currentGameObjectPanel); }
    }
}
