using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    [Header("Контроллер взаимодействия")]
    [SerializeField] private GameObject interactableObject;
    private InteractableScript interactableScript;

    [Header("Контроллер бафов")]
    [SerializeField] private GameObject buffsControllerObject;

    [Header("Контроллер разведки")]
    [SerializeField] private GameObject explorationObject;

    [Header("Главный контроллер")]
    [SerializeField] private GameObject gameControllerObject;
    private GameController gameControllerScript;

    [Header("Контроллер ресурсов")]
    [SerializeField] private GameObject resourceControllerObject;
    private ResourcesController resourcesControllerScript;

    private delegate void OneSkillSeconsPassed();
    private event OneSkillSeconsPassed timer;

    void Start()
    {
        gameControllerObject.TryGetComponent(out gameControllerScript);

        interactableObject.TryGetComponent(out interactableScript);

        resourceControllerObject.TryGetComponent(out resourcesControllerScript);
    }

    public void FixSkill(AbstractSquad squad)
    {
        squad.SetMasters(gameControllerObject,buffsControllerObject,explorationObject);

        switch(squad)
        {
            case InfantrySquad infantrySquad:
                SkillWithoutChoise(infantrySquad); break;
            case EngineerSquad engineerSquad:
                SkillWithoutChoise(engineerSquad); break;
            case TankSquad tankSquad:
                SkillWithoutChoise(tankSquad); break;
            case ArtillerySquad artillerySquad:
                if (resourcesControllerScript.CheckReourcesToSkill(ResourcesController.SkillType_ForCost.Artillary)){StartCoroutine(SkillWithChoise(artillerySquad));}
                break;
            case ScoutSquad scoutSquad:
                StartCoroutine(SkillWithChoise(scoutSquad)); break;
        }
    }
    private void SkillWithoutChoise(AbstractSquad squadUsedSkill)
    {
        Action usedSkill = squadUsedSkill.UseClassSkill(this,gameControllerScript.GetCellWithThisSquad(squadUsedSkill));

        if(usedSkill != null) { usedSkill.Invoke();}
    }
    private IEnumerator SkillWithChoise(AbstractSquad squadUsedSkill)
    {
        GameObject selectedObject = null;

        ChoosingScript.ChangeChooseState();
        interactableScript.playerIsInteract += (obj) => { selectedObject = obj; };

        yield return new WaitUntil(() => selectedObject != null);

        ChoosingScript.ChangeChooseState();

        if (squadUsedSkill is ArtillerySquad artillerySquad)
        {
            artillerySquad.SetSelectedCell(selectedObject);
        }
        else if (squadUsedSkill is ScoutSquad scoutSquad)
        {
            scoutSquad.SetSelectedCell(selectedObject);

            Action usedSkill = scoutSquad.UseClassSkill(this, gameControllerScript.GetCellWithThisSquad(squadUsedSkill));

            if(usedSkill != null) { usedSkill.Invoke(); }
        }

        selectedObject = null;
    }
}
