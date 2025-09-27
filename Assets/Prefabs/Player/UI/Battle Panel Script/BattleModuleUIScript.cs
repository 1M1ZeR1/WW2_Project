using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleModuleUIScript : MonoBehaviour
{
    [Header("Общая панель битвы")]
    [SerializeField] private GameObject battlePanelMain;

    private Dictionary<GameObject, GameObject> cellToBattlePanel = new Dictionary<GameObject, GameObject>();

    protected Dictionary<GameObject,GameObject> _cellToAllInfoPanel = new Dictionary<GameObject, GameObject>();
    protected Dictionary<GameObject, AllInfoPanelScript> _cellToAllInfoScript = new Dictionary<GameObject, AllInfoPanelScript>();

    [Header("Держащий content")]
    [SerializeField] private GameObject battlesPanelTaker;
    private RectTransform contentRectTransform;

    private void Start()
    {
        battlesPanelTaker.TryGetComponent(out contentRectTransform);
    }

    public void StartBattle(GameObject cell)
    {
        GameObject battlePanel = Instantiate(battlePanelMain,battlePanelMain.transform.parent);

        GameObject allFunctions = battlePanel.transform.GetChild(0).gameObject;

        cellToBattlePanel.Add(cell, battlePanel);
        _cellToAllInfoPanel.Add(cell, battlePanel.transform.GetChild(1).gameObject);
        _cellToAllInfoScript.Add(cell, battlePanel.transform.GetChild(1).GetComponent<AllInfoPanelScript>());

        allFunctions.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Битва за {cell.GetComponent<CellTypeScript>().GetCellName()}";
        allFunctions.transform.GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {

        });
        allFunctions.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(() => 
        {
            if (_cellToAllInfoPanel[cell].activeSelf) { _cellToAllInfoPanel[cell].SetActive(false); }
            else { _cellToAllInfoPanel[cell].SetActive(true); LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform); }
        });

        battlePanel.SetActive(true);
    }

    public void TryAddSquad(GameObject cell,AbstractSquad squad, bool inAttack)
    {
        _cellToAllInfoScript[cell].AddSquad(squad, inAttack);
    }
    public void TryRemoveSquad(GameObject cell, AbstractSquad squad)
    {
        _cellToAllInfoScript[cell].RemoveSquad(squad);
    }
    public void BattleIsOver(GameObject cell)
    {
        _cellToAllInfoScript.Remove(cell);

        Destroy(_cellToAllInfoPanel[cell]);
        _cellToAllInfoPanel.Remove(cell);

        Destroy(cellToBattlePanel[cell]);
        cellToBattlePanel.Remove(cell);
    }
    public void UpdateAllParameters_UI(GameObject cell, int attackRatio, int defenceRation)
    {
        _cellToAllInfoScript[cell].UpdateGlobalParameters(attackRatio, defenceRation);
    }
}
