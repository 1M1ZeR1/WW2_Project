using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MonobehaviourHandler : MonoBehaviour
{
    [Header("Build Prototype Object")]
    [SerializeField] private Transform[] buildPrototype;
    [Header("Build Prototype Needers")]
    [SerializeField] private Transform[] targetPrototype;
    [SerializeField] private PanelFactory_BuildPanel.PanelType[] targetPanelType;

    [Header("DropDown squad for training")]
    [SerializeField] private TMP_Dropdown dropdown;

    [SerializeField] private GameObject trainingPanel;

    private void Start()
    {
        ServiceRegistry.WorkWithService<PanelFactory_BuildPanel>().SetObjects(buildPrototype,targetPrototype,targetPanelType);
        ServiceRegistry.WorkWithService<PanelFactory_BuildPanel>().StartFactory();

        ServiceRegistry.WorkWithService<PanelFactory_SquadPanel>().SetObjects(dropdown);
        ServiceRegistry.WorkWithService<PanelFactory_SquadPanel>().StartFactory();

        ServiceRegistry.WorkWithController<AlliesSpawner>().Initialize(trainingPanel);
    }
}
