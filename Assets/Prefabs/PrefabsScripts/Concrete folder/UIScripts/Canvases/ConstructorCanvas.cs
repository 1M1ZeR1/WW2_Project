using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class ConstructorCanvas : MonoBehaviour
{
    private Canvas canvas;

    [SerializeField] private GameObject[] panelsToConstruct;

    private Dictionary<ConstructorCanvasTicket, GameObject> ticketToPanel = new();

    private Dictionary<ConstructorCanvasTicket, float[]> constructorConfig = new()
    {
        {ConstructorCanvasTicket.Expl,new float[]{ -30,-10} }
    };

    private void Start()
    {
        canvas = GetComponent<Canvas>();

        for (int i = 0; i < panelsToConstruct.Length; i++) 
        {
            ticketToPanel.Add((ConstructorCanvasTicket)i,panelsToConstruct[i]);
        }
    }

    public (float panelHeight, Vector2[] textsPos) CalculateLayoutWithText((string text, float fontFize)[] textElements, ConstructorCanvasTicket canvasTicket)
    {
        switch (canvasTicket) 
        {
            case ConstructorCanvasTicket.Expl:return CalculateLayoutExploration(textElements[0].fontFize,textElements[0].text, textElements[1].text);
        }

        return (0f,null);
    }
    private (float panelHeight, Vector2[] textsPos) CalculateLayoutExploration(float fontize, string timeText, string explText)
    {
        GameObject explPanel = ticketToPanel[ConstructorCanvasTicket.Expl];

        RectTransform panelRect = explPanel.GetComponent<RectTransform>();

        TextMeshProUGUI timeText_UGUI = explPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI explText_UGUI = explPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        timeText_UGUI.fontSize = fontize;
        explText_UGUI .fontSize = fontize;

        timeText_UGUI.text = timeText;
        explText_UGUI.text = explText;

        timeText_UGUI.ForceMeshUpdate(); explText_UGUI.ForceMeshUpdate();

        timeText_UGUI.rectTransform.anchoredPosition = new Vector2(timeText_UGUI.rectTransform.anchoredPosition.x, constructorConfig[ConstructorCanvasTicket.Expl][0]);

        explText_UGUI.rectTransform.anchoredPosition = new Vector2(explText_UGUI.rectTransform.anchoredPosition.x,timeText_UGUI.rectTransform.anchoredPosition.y 
            + constructorConfig[ConstructorCanvasTicket.Expl][1] * 2
            - explText_UGUI.preferredHeight / 2);


        return (-explText_UGUI.rectTransform.anchoredPosition.y + explText_UGUI.preferredHeight / 2 + (-constructorConfig[ConstructorCanvasTicket.Expl][1]),
            new Vector2[]
            {
                timeText_UGUI.rectTransform.anchoredPosition,
                explText_UGUI.rectTransform.anchoredPosition
            });
    }
}
public enum ConstructorCanvasTicket
{
    Expl,
}
