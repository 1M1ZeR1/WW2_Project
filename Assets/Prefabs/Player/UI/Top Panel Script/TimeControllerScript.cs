using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeControllerScript : MonoBehaviour
{
    [Header("Главный контроллер")]
    [SerializeField] private GameObject gameControllerObject;
    private GameController gameControllerScript;

    [Header("Год панель")]
    [SerializeField] private GameObject yearPanelObject;
    private TextMeshProUGUI yearPanelText;

    [Header("Месяц/день панель")]
    [SerializeField] private GameObject monthDayPanelObject;
    private TextMeshProUGUI monthDayPanelText;

    [Header("Час панель")]
    [SerializeField] private GameObject hourPanelObject;
    private TextMeshProUGUI hourPanelText;

    public delegate void HourHasPassed(DateTime currentTime);
    public event HourHasPassed OnHourHasPassed;

    protected static DateTime _dateTime = new DateTime(1944,1,1,0,0,0);

    private void Start()
    {
        yearPanelObject.TryGetComponent( out yearPanelText);
        monthDayPanelObject.TryGetComponent( out monthDayPanelText);
        hourPanelObject.TryGetComponent( out hourPanelText);

        gameControllerObject.TryGetComponent(out gameControllerScript);
        gameControllerScript.oneHourLeft += OtherTimeTaker;
    }

    private void OtherTimeTaker()
    {
        _dateTime = _dateTime.AddHours(1);

        yearPanelText.text = $"{_dateTime.Year} г.";
        monthDayPanelText.text = $"{_dateTime.Day}.{_dateTime.Month}";
        hourPanelText.text = $"{_dateTime.Hour}:00";

        if(OnHourHasPassed != null)
        {
            OnHourHasPassed.Invoke(_dateTime);
        }
    }
    public static string GetCurrentTime()
    {
        return $"{_dateTime.Day}:{_dateTime.Month}.{_dateTime.Year}";
    }
    public static DateTime GetCurrentDateTime() { return _dateTime; }
}
