using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourcesTopPanelScript : MonoBehaviour
{
    [Header("Контроллер ресурсов")]
    [SerializeField] private GameObject resourcesControllerObject;
    private ResourcesController resourcesController;

    [Header("Кол-во стройматериалов")]
    [SerializeField] private GameObject buildMaterialPanel;
    private TextMeshProUGUI buildMaterialText;

    [Header("Кол-во боеприпасов")]
    [SerializeField] private GameObject weaponMaterialPanel;
    private TextMeshProUGUI weaponMaterialText;

    [Header("Кол-во людей")]
    [SerializeField] private GameObject peopleMaterialPanel;
    private TextMeshProUGUI peopleMaterialText;

    [Header("Кол-во транспорта")]
    [SerializeField] private GameObject transportMaterialPanel;
    private TextMeshProUGUI transportMaterialText;
    
    [Header("Влияние")]
    [SerializeField] private GameObject influencePanel;
    private TextMeshProUGUI influenceText;

    private void Awake()
    {
        resourcesControllerObject.TryGetComponent(out resourcesController);

        buildMaterialPanel.TryGetComponent(out buildMaterialText);
        weaponMaterialPanel.TryGetComponent(out weaponMaterialText);
        influencePanel.TryGetComponent(out influenceText);
        peopleMaterialPanel.TryGetComponent(out peopleMaterialText);
        transportMaterialPanel.TryGetComponent(out transportMaterialText);

        resourcesController.ChangedResourceCount += ChangeData;
    }

    public void ChangeData(int count,ResourcesEnum type)
    {
        if (type == ResourcesEnum.Weapon) { weaponMaterialText.text = count.ToString(); }
        if (type == ResourcesEnum.Building) { buildMaterialText.text = count.ToString(); }
        if (type == ResourcesEnum.People) { peopleMaterialText.text = count.ToString(); }
        if (type == ResourcesEnum.Transport) { transportMaterialText.text = count.ToString(); }
        if (type == ResourcesEnum.Influence) { influenceText.text = count.ToString(); }
    }
}
