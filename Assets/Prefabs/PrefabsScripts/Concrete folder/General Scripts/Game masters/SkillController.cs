using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    private GameObject selectedObject;
    private List<GameObject> selectedObjects = new();

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
            Debug.LogError(confirmation);
            Debug.LogError($"{selectedObjects.Count},{inChoosingMode},{confirmation}");
        });

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<ExplorationChoosingMode, SkillController, List<GameObject>>((sender, key, cells) =>
        {
            selectedObjects = cells;
            //Debug.LogError(selectedObjects.Count);
            //Debug.LogError($"{selectedObjects.Count},{inChoosingMode},{confirmation}");
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
                yield return new WaitUntil(() => selectedObjects.Count != 0 && inChoosingMode && confirmation);
                ServiceRegistry.WorkWithService<EventBus>().Publish<SkillController, AbstractSquad, List<GameObject>>(this, squadUsedSkill, selectedObjects);

                selectedObjects.Clear();
                break;
            default:
                yield return new WaitUntil(() => selectedObject != null && inChoosingMode && confirmation);
                ServiceRegistry.WorkWithService<EventBus>().Publish<SkillController, AbstractSquad, GameObject>(this, squadUsedSkill, selectedObject);

                selectedObject = null;
                break;
        }
        //Debug.LogError("Used skill");


        ServiceRegistry.WorkWithController<ChoosingScript>().ExitChoiseState();

        squadUsedSkill.UseClassSkill()?.Invoke();

        inChoosingMode=false;
        confirmation=false;
    }
}
