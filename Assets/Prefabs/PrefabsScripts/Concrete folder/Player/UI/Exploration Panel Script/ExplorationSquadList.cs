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

    private List<AbstractSquad> squadWeCantRevoke = new();
    private Dictionary<AbstractSquad,(GameObject,GameObject)> fastSearchButtons = new();

    private void Start()
    {
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<ScoutSquad, Sprite>(AddToScoutsList);

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<ExplorationSquadList, ExplorationListMono>((key1,key2)=>CheckAllSquadsPanels());

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<ExplorationController, ExplorationSquadList, AbstractSquad>((Key1, key2, scoutSquad) => 
        { 
            squadWeCantRevoke.Add(scoutSquad);

            CheckAllSquadsPanels();
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

        fastSearchButtons[squad] = (newScoutPanel.transform.Find("RevokeSquad").gameObject, newScoutPanel.transform.Find("DeleteSquad").gameObject);

        fastSearchButtons[squad].Item2.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => 
        {
            RemoveScouts(squad);
        });
        fastSearchButtons[squad].Item1.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
        {
            ServiceRegistry.WorkWithController<ExplorationController>().SquadGoingRevoke = squad;

            ServiceRegistry.WorkWithController<InteractableScript>().RevokeSquad(squad,
                ServiceRegistry.WorkWithController<ExplorationController>().FinishExploration);

        });
    }

    
    private void CheckAllSquadsPanels()
    {
        foreach(var panel in squadToHisPanel)
        {
            if (squadWeCantRevoke.Contains(panel.Key)) 
            {
                fastSearchButtons[panel.Key].Item1.SetActive(false);
                fastSearchButtons[panel.Key].Item2.SetActive(true);
                continue;
            }

            fastSearchButtons[panel.Key].Item1.SetActive(true);
            fastSearchButtons[panel.Key].Item2.SetActive(false);
        }
    }

    private void RemoveScouts(AbstractSquad squad)
    {
        ServiceRegistry.WorkWithService<EventBus>().Publish<ExplorationSquadList, VirtualList, AbstractSquad>(this, null, squad);

        Destroy(squadToHisPanel[squad]);

        squadToHisPanel.Remove(squad);
        squadWeCantRevoke.Remove(squad);
        fastSearchButtons.Remove(squad);
    }
}
