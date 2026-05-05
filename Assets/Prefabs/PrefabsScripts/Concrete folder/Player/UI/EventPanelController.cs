using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventPanelController : MonoBehaviour
{
    [Header("Картинка ивента")]
    [SerializeField] private GameObject eventImageObject;
    private Image eventImage;

    [Header("Текст ивента")]
    [SerializeField] private GameObject eventTextObject;
    private TextMeshProUGUI eventText;

    [Header("Кнопка действия 1")]
    [SerializeField] private GameObject buttonAction1;
    [Header("Кнопка действия 2")]
    [SerializeField] private GameObject buttonAction2;
    [SerializeField] private Sprite[] eventSprites;

    private Button buttonAction1_Button, buttonAction2_Button;

    public void ShowEvent(AbsctractEvent currentEvent)
    {
        if(eventText == null)
        {
            eventImageObject.TryGetComponent(out eventImage);
            eventTextObject.TryGetComponent(out eventText);

            buttonAction1_Button = buttonAction1.GetComponent<Button>();
            buttonAction2_Button = buttonAction2.GetComponent<Button>();
        }

        eventText.text = currentEvent.EventDiscription;

        var actions = currentEvent.EventActions;

        buttonAction1_Button.onClick.RemoveAllListeners();
        buttonAction2_Button.onClick.RemoveAllListeners();

        if (actions.Length == 2)
        {
            buttonAction1_Button.onClick.AddListener(() => actions[0].Invoke());
            buttonAction1_Button.onClick.AddListener(() => gameObject.SetActive(false));
            buttonAction2_Button.onClick.AddListener(() => actions[1].Invoke());
            buttonAction2_Button.onClick.AddListener(() => gameObject.SetActive(false));

            buttonAction1.transform.GetComponentInChildren<TextMeshProUGUI>().text = currentEvent.EventActionsDicriptions[0];
            buttonAction2.transform.GetComponentInChildren<TextMeshProUGUI>().text = currentEvent.EventActionsDicriptions[1];

            buttonAction2.SetActive(true);
        }
        else
        {
            buttonAction1_Button.onClick.AddListener(() => actions[0].Invoke());
            buttonAction1_Button.onClick.AddListener(()=>gameObject.SetActive(false));
            buttonAction1.transform.GetComponentInChildren<TextMeshProUGUI>().text = currentEvent.EventActionsDicriptions[0];

            buttonAction2.SetActive(false);
        }
    }
}
