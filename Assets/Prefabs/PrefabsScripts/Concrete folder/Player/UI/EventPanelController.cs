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

    public void ShowEvent(AbsctractEvent currentEvent)
    {
        if(eventText == null)
        {
            eventImageObject.TryGetComponent(out eventImage);
            eventTextObject.TryGetComponent(out eventText);
        }

        eventText.text = currentEvent.EventDiscription;

        var actions = currentEvent.EventActions;

        if (actions.Length == 2)
        {
            buttonAction1.GetComponent<Button>().onClick.AddListener(() => actions[0].Invoke());
            buttonAction1.GetComponent<Button>().onClick.AddListener(() => gameObject.SetActive(false));
            buttonAction2.GetComponent<Button>().onClick.AddListener(() => actions[1].Invoke());
            buttonAction2.GetComponent<Button>().onClick.AddListener(() => gameObject.SetActive(false));

            buttonAction1.transform.GetComponentInChildren<TextMeshProUGUI>().text = currentEvent.EventActionsDicriptions[0];
            buttonAction2.transform.GetComponentInChildren<TextMeshProUGUI>().text = currentEvent.EventActionsDicriptions[1];

            buttonAction2.SetActive(true);
        }
        else
        {
            buttonAction1.GetComponent<Button>().onClick.AddListener(() => actions[0].Invoke());
            buttonAction1.GetComponent<Button>().onClick.AddListener(()=>gameObject.SetActive(false));
            buttonAction1.transform.GetComponentInChildren<TextMeshProUGUI>().text = currentEvent.EventActionsDicriptions[0];

            buttonAction2.SetActive(false);
        }
    }
}
