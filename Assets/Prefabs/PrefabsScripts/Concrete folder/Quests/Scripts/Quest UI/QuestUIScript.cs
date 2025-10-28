using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuestUIScript : MonoBehaviour
{
    [Header("Quest Panel")]
    [SerializeField] private GameObject questPanel;

    [Header("Quest Discription")]
    [SerializeField] private GameObject questDiscriptionPanel;
    protected TextMeshProUGUI _questHeaderText;
    protected TextMeshProUGUI _questDiscriptionText;
    protected TextMeshProUGUI _questRewardText;
    protected TextMeshProUGUI _questDeadLineText;

    private void Awake()
    {
        questDiscriptionPanel.transform.GetChild(0).TryGetComponent(out _questHeaderText);
        questDiscriptionPanel.transform.GetChild(1).TryGetComponent(out _questDiscriptionText);
        questDiscriptionPanel.transform.GetChild(2).TryGetComponent(out _questRewardText);
        questDiscriptionPanel.transform.GetChild(3).TryGetComponent(out _questDeadLineText);
    }

    protected Dictionary<AbstractQuest, string[]> _questText = new Dictionary<AbstractQuest, string[]>();
    protected Dictionary<AbstractQuest, string[]> _questRewardDisc = new Dictionary<AbstractQuest, string[]>();

    protected Dictionary<AbstractQuest, GameObject> _questTopanel = new Dictionary<AbstractQuest, GameObject>();

    protected AbstractQuest _currentWorkingQuest;

    protected bool _IsQuestWindowOpen = false;

    public void CreateQuestPanel(AbstractQuest quest)
    {
        _questRewardDisc.Add(quest, quest.GetDiscriptionReward().ToArray());

        _questText.Add(quest, new string[]
        {
            quest.GetTitle(),
            quest.GetHeader(),
            quest.GetDiscription(),
            quest.GetDiscriptionDeadLine() 
        });

        GameObject newPanel = Instantiate(questPanel,transform);

        newPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = _questText[quest][0];
        DifficulryConverter(newPanel.transform.GetChild(1).GetComponent<Image>(), quest);

        _questTopanel.Add(quest, newPanel);

        
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => OpenCloseQuestDiscription(quest));

        newPanel.GetComponent<EventTrigger>().triggers.Add(entry);

        newPanel.SetActive(true);
    }
    private void DifficulryConverter(Image image, AbstractQuest quest)
    {
        int difficulty = quest.GetDifficulty();

        if (difficulty < 20) { image.color = new Color(127,255,0); return; }
        if (difficulty < 40) { image.color = new Color(255, 165, 0); return; }
        if (difficulty < 50) { image.color = new Color(255, 0, 0); return; }
    }
    public void OpenCloseQuestDiscription(AbstractQuest quest)
    {
        if (_currentWorkingQuest != null)
        {
            if(_currentWorkingQuest == quest)
            {
                if (questDiscriptionPanel.activeSelf) { questDiscriptionPanel.SetActive(false); }
                else { questDiscriptionPanel.SetActive(true); }
            }
            else
            {
                if (!questDiscriptionPanel.activeSelf) { TextConstructor(quest); questDiscriptionPanel.SetActive(true); }
                else { TextConstructor(quest); }
            }
        }
        else { _currentWorkingQuest = quest; TextConstructor(quest); }
    }
    private void TextConstructor(AbstractQuest quest)
    {
        var alltext = _questText[quest];

        _questHeaderText.text = alltext[1];
        _questDiscriptionText.text = alltext[2];
        _questRewardText.text = "Награда: ";
        _questDeadLineText.text = $"Нужно выполнить до:{alltext[3]}";

        foreach(var item in _questRewardDisc[quest])
        {
            _questRewardText.text += $"{item}\n";
        }
    }
    public void QuestIsComplited(AbstractQuest quest)
    {
        _questRewardDisc.Remove(quest);

        if(_currentWorkingQuest == quest) { questDiscriptionPanel.SetActive(false);}

        Destroy(_questTopanel[quest]);
        _questTopanel.Remove(quest);

        _questText.Remove(quest);
    }
}
