using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquadPanelScript : MonoBehaviour
{
    [SerializeField] private InteractableScript interactableScript;
    [SerializeField] private SquadAllInformationScript allInformationScript;
    [SerializeField]private SkillController skillControllerScript;

    public void BindButton_Interaction(GameObject cell, AbstractSquad squad)
    {
        ServiceRegistry.WorkWithService<SelectedSquadsBuffer>().AddToSelectedSquads_Guaranteed(squad);

        ServiceRegistry.WorkWithController<InteractableScript>().WantToChooseCell();
    }

    public void BindButton_ShowInformation(AbstractSquad squad)
    {
        allInformationScript.ShowAllInfromation(squad);
    }

    public void BintButton_UseSkill(AbstractSquad squad)
    {
        skillControllerScript.FixSkill(squad);
    }

    public void BindImageEvent_AddToSelectedList(AbstractSquad squad)
    {
        ServiceRegistry.WorkWithService<SelectedSquadsBuffer>().AddToSelectedSquads(squad);
    }
}
