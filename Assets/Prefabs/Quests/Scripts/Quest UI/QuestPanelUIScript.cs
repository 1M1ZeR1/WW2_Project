using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestPanelUIScript : MonoBehaviour
{
    private AbstractQuest currentQuest;

    private QuestUIScript questUIScript;

    private void OnEnable()
    {
        questUIScript = transform.parent.GetComponent<QuestUIScript>();
    }
    public void SetQuestWhatTake(AbstractQuest quest){ currentQuest = quest;}
    public void ClickEvent()
    {
        questUIScript.OpenCloseQuestDiscription(currentQuest);
    }
}
