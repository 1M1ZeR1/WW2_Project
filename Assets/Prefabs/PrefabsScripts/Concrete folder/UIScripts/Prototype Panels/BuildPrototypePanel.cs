using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildPrototypePanel : MonoBehaviour
{
    [Header("Tope panel")]
    [SerializeField]private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Image of build")]
    [SerializeField] private Image buildImage;

    [Header("Build button")]
    [SerializeField] private Button buildButton;
    [SerializeField] private TextMeshProUGUI buildButtonText;
    [SerializeField] private TextMeshProUGUI buildButtonCostText;

    [Header("Upgrade button")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [SerializeField] private TextMeshProUGUI upgradeButtonCostText;

    [Header("Description button")]
    [SerializeField] private Button descriptionButton;
    [SerializeField] private TextMeshProUGUI descriptionButtonText;

    [Header("Open build menu button")]
    [SerializeField] private Button openMenuButton;
    [SerializeField] private TextMeshProUGUI openMenuButtonText;

    [Header("Build description")]
    [SerializeField] private TextMeshProUGUI buildDescriptionText;

    public GameObject Clone(string buildName, Sprite buildSprite, int buildCost, int upgradeCost,string buildDescription,string id, Transform parent)
    {
        GameObject newCreatedPanel = Instantiate(gameObject, parent);

        BuildPrototypePanel newCreatedPanelPrototype = newCreatedPanel.GetComponent<BuildPrototypePanel>();

        newCreatedPanelPrototype.nameText.text = buildName;
        newCreatedPanelPrototype.buildImage.sprite = buildSprite;

        newCreatedPanelPrototype.buildButtonCostText.text = buildCost.ToString();
        newCreatedPanelPrototype.upgradeButtonCostText.text = upgradeCost.ToString();

        if (buildDescriptionText != null) { buildDescriptionText.text = buildDescription; }



        newCreatedPanel.GetComponent<BuildItemPanel>().id = id;
        newCreatedPanel.GetComponent<BuildItemPanel>().InstantiateComponent();

        newCreatedPanel.SetActive(true);

        return newCreatedPanel;
    }
}
