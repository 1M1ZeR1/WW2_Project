using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ExplorationSquadList : MonoBehaviour
{
    [SerializeField] private GameObject prototypePanel;

    private Dictionary<AbstractSquad, GameObject> squadToHisPanel = new();

    private void Start()
    {
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<AbstractSquad, Sprite>(AddToScoutsList);

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<GameController, int, AbstractSquad>((sender, value, squad) =>
        {
            if(value == 2) { squadToHisPanel.Remove(squad); }
        });
    }
    private void AddToScoutsList(AbstractSquad squad, Sprite squadImage)
    {
        Debug.LogError("Я тут был");

        GameObject newScoutPanel = Instantiate(prototypePanel,prototypePanel.transform.parent);

        if (squadImage != null) { newScoutPanel.transform.GetChild(0).GetComponent<UnityEngine.UIElements.Image>().sprite = squadImage; }

        newScoutPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = squad.Name;

        squadToHisPanel[squad] = newScoutPanel;

        newScoutPanel.SetActive(true);

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { ServiceRegistry.WorkWithService<EventBus>().Publish<ExplorationSquadList, AbstractSquad>(this,squad); });

        newScoutPanel.GetComponent<EventTrigger>().triggers.Add(entry);
    }
}
