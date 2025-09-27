using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuffCircleScript : MonoBehaviour
{
    [Header("Описание бафа")]
    [SerializeField] private GameObject buffDiscriptionPanel;

    private AbstractBuffs buff;

    public void CustomOnMouseEnter()
    {
        TextMeshProUGUI buffText = buffDiscriptionPanel.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();

        buffText.text = buff.GetBuffDescription();

        buffDiscriptionPanel.SetActive(true);
        
    }
    public void CustomOnMouseExit()
    {
        buffDiscriptionPanel.SetActive(false);
    }
    public void SetBuffForDiscription(AbstractBuffs buff)
    {
        this.buff = buff;
    }
}
