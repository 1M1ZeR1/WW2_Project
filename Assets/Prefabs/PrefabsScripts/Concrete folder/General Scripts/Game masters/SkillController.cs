using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    private GameObject selectedObject;
    private bool inChoosingMode = false;

    private bool confirmation = false;

    private Coroutine choiseCoroutine;

    private void Start()
    {
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject, bool>((sender, cell, UI_resolution) =>
        {
            if(inChoosingMode)selectedObject = cell;
        });

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<ChoosingScript, SkillController>((sender, key) =>
        {
            if (inChoosingMode)
            {
                inChoosingMode = false;

                ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStopper(choiseCoroutine);
            }
        });

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<SkillController, bool>((key1, key2) =>
        {
            confirmation = true;
        });
    }
    public void FixSkill(AbstractSquad squad)
    {
        if(squad.SkillWithChoise)choiseCoroutine = ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(SkillWithChoise(squad));
        else SkillWithoutChoise(squad);
    }
    private void SkillWithoutChoise(AbstractSquad squadUsedSkill)
    {
        squadUsedSkill.UseClassSkill()?.Invoke();
    }
    private IEnumerator SkillWithChoise(AbstractSquad squadUsedSkill)
    {
        inChoosingMode = true;


        switch (squadUsedSkill.Type)
        {
            case SquadEnum.Scouts:
                ServiceRegistry.WorkWithController<ChoosingScript>().CreateChoiseState(ChoosingScript.ChoosingMode.Exploration);
                break;
        }

        yield return new WaitUntil(() => selectedObject != null && inChoosingMode && confirmation);

        ServiceRegistry.WorkWithService<EventBus>().Publish<SkillController, AbstractSquad, GameObject>(this, squadUsedSkill, selectedObject);

        ServiceRegistry.WorkWithController<ChoosingScript>().ExitChoiseState();

        squadUsedSkill.UseClassSkill()?.Invoke();

        selectedObject = null;
        inChoosingMode=false;
        confirmation=false;
    }
}
