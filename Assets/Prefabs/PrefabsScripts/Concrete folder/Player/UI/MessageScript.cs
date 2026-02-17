using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageScript : MonoBehaviour
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
        if(_panelImage == null)
        {
            _panelImage = GetComponent<Image>();
            textTaker.TryGetComponent<TextMeshProUGUI>(out _textImage);
        }

        _panelColor = _panelImage.color;
        _textColor = _textImage.color;
    }

    private void Update()
    {
        if(_needShowMessage)
        {
            _timer += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, _timer / timeToHide);

            _panelColor.a = alpha;
            _textColor.a = alpha;

            _panelImage.color = _panelColor;
            _textImage.color = _textColor;

            if(_timer >= timeToHide)
            {
                _timer = 0f;
                _needShowMessage = false;

                gameObject.SetActive(false);
            }
        }
    }
    public void SendMessage(int messageType)
    {
        gameObject.SetActive(true);

        _timer = 0;

        _panelColor.a = 1;
        _textColor.a = 1;

        _panelImage.color = _panelColor;
        _textImage.color = _textColor;

        _textImage.text = _messages[messageType];

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

    protected string[] _messages = new string[]
    {
        " летка находитс€ под вражеским контролем.",//0
        "Ёта территори€ не поблизости.",//1
        "Ќе хватает людей на обучени€ этого отр€да.",//2
        "Ќет отр€дов дл€ постройки сооружени€.",//3
        " летка не под вашим контроллем, стройка недоступна.",//4
        "ƒл€ строительства здани€ не хватает ресурсов.",//5
        "¬ лагере уже тренируетс€ максимальное колличество отр€дов.",//6
        "Ќе хватает вооружени€.",//7
        "Ќе хватает транспорта.",//8
        "¬се отр€ды зан€ты.",//9
        "Ћагерь позвол€ет тренировать новых бойцов.\r\n\r\nlvl 1: максимальное кол-во отр€дов 3\r\n\r\nlvl 2: максимальное кол-во отр€дов 5",//10
        "‘орт значительно повышает защиту.\r\n\r\nlvl 1: защита +20%\r\n\r\nlvl 2: защита +40%",//11
        "јкадеми€ позвол€ет обучать специализированные отр€ды бойцов.\r\n\r\nlvl 1: максимальное кол-во техники 3\r\n\r\nlvl 2: максимальное кол-во техники 5",
        "",
        "",
        "",
        "¬ отр€де разведчиков не может быть больше 5 человек."//16
    };

    public void EnableMessage() { gameObject.SetActive(true); }
}
