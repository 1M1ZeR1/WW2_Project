using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DescriptionScript : MonoBehaviour
{
    [SerializeField] private int timeToHide;

    [SerializeField] private GameObject textTaker;

    protected Image _panelImage;
    protected TextMeshProUGUI _textImage;

    protected Color _panelColor;
    protected Color _textColor;

    protected float _timer;

    protected bool _needShowMessage;

    private void OnEnable()
    {
        if (_panelImage == null)
        {
            _panelImage = GetComponent<Image>();
            _textImage = textTaker.GetComponent<TextMeshProUGUI>();
        }

        _panelColor = _panelImage.color;
        _textColor = _textImage.color;
    }

    private void Update()
    {
        if (_needShowMessage)
        {
            _timer += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, _timer / timeToHide);

            _panelColor.a = alpha;
            _textColor.a = alpha;

            _panelImage.color = _panelColor;
            _textImage.color = _textColor;

            if (_timer >= timeToHide/2)
            {
                _timer = 0f;
                _needShowMessage = false;

                gameObject.SetActive(false);
            }
        }
    }
    public void SendDescription(string buildId)
    {
        gameObject.SetActive(true);

        _timer = 0;

        _panelColor.a = 1;
        _textColor.a = 1;

        _panelImage.color = _panelColor;
        _textImage.color = _textColor;

        _textImage.text = _messages[buildId];

        _needShowMessage = true;
    }
    public void SendMessage_Custom(string message)
    {
        _timer = 0;

        _panelColor.a = 1;
        _textColor.a = 1;

        _panelImage.color = _panelColor;
        _textImage.color = _textColor;

        _textImage.text = message;

        _needShowMessage = true;
    }

    protected Dictionary<string,string> _messages = new()
    {
        {"CampBuild","Ћагерь позвол€ет тренировать новых бойцов.\r\n\r\nlvl 1: максимальное кол-во отр€дов 3\r\n\r\nlvl 2: максимальное кол-во отр€дов 5" },//0
        {"FortBuild","‘орт значительно повышает защиту.\r\n\r\nlvl 1: защита +20%\r\n\r\nlvl 2: защита +40%"},//1
        {"AcademyBuild","јкадеми€ позвол€ет обучать специализированные отр€ды бойцов.\r\n\r\nlvl 1: максимальное кол-во техники 3\r\n\r\nlvl 2: максимальное кол-во техники 5"},//2
        {"HeadquartersBuild","Ўтаб даЄт возможность координировать действи€ на клетках с единого места."},//3
        {"5",""},//4
        {"6",""},//5
    };

    public void EnableMessage() { gameObject.SetActive(true); }
}
