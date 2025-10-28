using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingModule : MonoBehaviour
{
    [SerializeField] private Image circleImage;

    private Image buildImage;

    private void Awake()
    {
        buildImage = transform.GetChild(0).GetComponent<Image>();
    }

    public void SetBuildingSprite(Sprite sprite)
    {
        buildImage.sprite = sprite;
    }
    public void ChangeFillAmount(float amount)
    {
        circleImage.fillAmount = amount;
        buildImage.fillAmount = amount;
    }

    public void InvokeDestroy() { Destroy(gameObject); }
}
