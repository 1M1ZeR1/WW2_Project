using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AllInfoPanelScript : MonoBehaviour
{
    [Header("Панель атаки")]
    [SerializeField] private GameObject attackPanel;

    [Header("Панель защиты")]
    [SerializeField] private GameObject defencePanel;

    [Header("Панель всей атаки")]
    [SerializeField] private GameObject allAttackPanel;
    private TextMeshProUGUI allAttackText;

    [Header("Панель всей защиты")]
    [SerializeField] private GameObject allDefencePanel;
    private TextMeshProUGUI allDefenceText;

    [SerializeField] private GameObject attackContent;
    [SerializeField] private GameObject defenceContent;

    protected Dictionary<AbstractSquad,GameObject> _squadToTheirPanel = new Dictionary<AbstractSquad,GameObject>();
    protected Dictionary<AbstractSquad, bool> _inAttackSquad = new Dictionary<AbstractSquad, bool>();

    public void UpdateGlobalParameters(int attackRatio, int defenceRatio)
    {
        if(allAttackText == null)
        {
            allAttackPanel.TryGetComponent(out allAttackText);
            allDefencePanel.TryGetComponent(out allDefenceText);
        }

        allAttackText.text = $"Атака:{attackRatio}";
        allDefenceText.text = $"Защита:{defenceRatio}";
    }
    public void AddSquad(AbstractSquad squad, bool inAttack)
    {
        if (_squadToTheirPanel.ContainsKey(squad)) { UpdateInfo(squad); }
        else
        {
            if(inAttack)
            {
                GameObject newAttackPanel = Instantiate(attackPanel,attackContent.transform);

                newAttackPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = squad.Name;
                newAttackPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Колличество:{squad.PeopleCount}";
                newAttackPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"Атака:{squad.GetAllAttack()}";

                _inAttackSquad.Add(squad, true);
                _squadToTheirPanel.Add(squad, newAttackPanel);

                newAttackPanel.SetActive(true);
            }
            else
            {
                GameObject newDefencePanel = Instantiate(defencePanel, defenceContent.transform);

                newDefencePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = squad.Name;
                newDefencePanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Колличество:{squad.PeopleCount}";
                newDefencePanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"Защита:{squad.GetAllProtection()}";

                _inAttackSquad.Add(squad, false);
                _squadToTheirPanel.Add(squad, newDefencePanel);

                newDefencePanel.SetActive(true);
            }
        }
    }
    public void RemoveSquad(AbstractSquad squad)
    {
        _inAttackSquad.Remove(squad);
        Destroy(_squadToTheirPanel[squad]);

        _squadToTheirPanel.Remove(squad);
    }
    private void UpdateInfo(AbstractSquad squad)
    {
        GameObject squadPanel = _squadToTheirPanel[squad];

        squadPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Колличество:{squad.PeopleCount}";
        if (_inAttackSquad[squad])
        {
            squadPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"Атака:{squad.GetAllAttack()}";
        }
        else
        {
            squadPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"Защита:{squad.GetAllProtection()}";
        }
        
    }
}
