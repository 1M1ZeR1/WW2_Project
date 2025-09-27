using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingModule : MonoBehaviour
{
    [SerializeField] private Image circleImage;

    [SerializeField] private Sprite[] buildingsImages;

    private Dictionary<BuildsEnum, Sprite> keyValuePairs;


    private Image buildImage;

    private void Awake()
    {
        buildImage = transform.GetChild(0).GetComponent<Image>();

        keyValuePairs = new ()
        {
            {BuildsEnum.HeadQuarters,buildingsImages[0] },
            {BuildsEnum.Camp,buildingsImages[1]},
            {BuildsEnum.Fort,buildingsImages[2]},
            {BuildsEnum.MilitaryAcademy,buildingsImages[3]},
        };
    }

    public void SetBuildingType(BuildsEnum buildingType)
    {
        buildImage.sprite = keyValuePairs[buildingType];
    }
    public void ChangeFillAmount(float amount)
    {
        circleImage.fillAmount = amount;
        buildImage.fillAmount = amount;
    }

    public void InvokeDestroy() { Destroy(gameObject); }
}
