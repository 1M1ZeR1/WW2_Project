using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SkillController : MonoBehaviour
{

    private delegate void OneSkillSeconsPassed();
    private event OneSkillSeconsPassed timer;


    public void FixSkill(AbstractSquad squad)
    {
        if(squad.SkillWithChoise)ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(SkillWithChoise(squad));
        else SkillWithoutChoise(squad);
    }
    private void SkillWithoutChoise(AbstractSquad squadUsedSkill)
    {
        squadUsedSkill.UseClassSkill()?.Invoke();
    }
    private IEnumerator SkillWithChoise(AbstractSquad squadUsedSkill)
    {
        GameObject selectedObject = null;

        ChoosingScript.ChangeChooseState();
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject>((sender, cell) =>
        {
            selectedObject = cell; 
        });

        yield return new WaitUntil(() => selectedObject != null);

        ServiceRegistry.WorkWithService<EventBus>().Publish<SkillController, AbstractSquad, GameObject>(this, squadUsedSkill, selectedObject);

        ChoosingScript.ChangeChooseState();

        squadUsedSkill.UseClassSkill()?.Invoke();

        selectedObject = null;
    }
}
