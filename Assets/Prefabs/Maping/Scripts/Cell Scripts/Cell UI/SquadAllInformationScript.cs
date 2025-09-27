using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SquadAllInformationScript : MonoBehaviour
{
    [Header("Сама информационная панель")]
    [SerializeField] private GameObject informationSquadPanel;

    [Header("Имя отряда")]
    [SerializeField] private GameObject nameSquadTaker;

    [Header("Кол-во человек в отряде")]
    [SerializeField] private GameObject countSquadTaker;

    [Header("Параметр атаки")]
    [SerializeField] private GameObject attackSquadTaker;

    [Header("Параметр защиты")]
    [SerializeField] private GameObject protectionSquadTaker;

    [Header("Скорость")]
    [SerializeField] private GameObject speedSquadTaker;

    [Header("Способ перемещения")]
    [SerializeField] private GameObject transportSquadTaker;

    [Header("Вооружение")]
    [SerializeField] private GameObject weaponSquadTaker;

    public void ShowAllInfromation(AbstractSquad squad)
    {

        if (!informationSquadPanel.activeSelf)
        {
            informationSquadPanel.SetActive(true);
        }

        nameSquadTaker.GetComponent<TextMeshProUGUI>().text = $"Имя отряда:{squad.Name}";

        countSquadTaker.GetComponent<TextMeshProUGUI>().text = $"Кол-во человек в отряде:{squad.PeopleCount}";

        attackSquadTaker.GetComponent<TextMeshProUGUI>().text = $"Атака:{squad.GetAllAttack()}";

        protectionSquadTaker.GetComponent<TextMeshProUGUI>().text = $"Защита:{squad.GetAllProtection()}";

        speedSquadTaker.GetComponent<TextMeshProUGUI>().text = $"Скорость:{squad.GetAllSpeedOfMovement()}";

        transportSquadTaker.GetComponent<TextMeshProUGUI>().text = $"Средство передвижения:{TypesConverter.TransportDicription[squad.Transport]}";

        weaponSquadTaker.GetComponent<TextMeshProUGUI>().text = $"Вооружение:{TypesConverter.WeaponDiscription[squad.Weapon]}";
    }
}
