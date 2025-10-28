using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldOnCanvasScript : MonoBehaviour
{
    [SerializeField] private GameObject battleModule;
    public GameObject CreateBattleModule()
    {
        GameObject newBattleModule = Instantiate(battleModule,gameObject.transform);
        newBattleModule.SetActive(true);

        return newBattleModule; 
    }


    [SerializeField] private GameObject buildingModule;
    public BuildingModule CreateBuildingModule(GameObject cell, string id_Build)
    {
        GameObject newBuildingModule = Instantiate(buildingModule,cell.transform.position, Quaternion.Euler(90f,0f,0f), gameObject.transform);
        newBuildingModule.SetActive(true);

        var component = newBuildingModule.GetComponent<BuildingModule>();

        BuildPanelSO buildPanelById = (BuildPanelSO)ServiceRegistry.WorkWithService<DataHolder>().GetDataFromHolder($"{id_Build}_panel");
        component.SetBuildingSprite(buildPanelById.buildSprite);

        return component;
    }
}
