using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadquartersCellInArea : MonoBehaviour
{
    [SerializeField] private GameObject cellInArea;
    [SerializeField] private GameObject cellIsHard;

    [SerializeField] private CanvasGroup canvasGroup;

    protected List<GameObject> createdImages = new();

    private void Start()
    {
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<HeadquartersPanel, List<GameObject>, List<GameObject>>((sender, list1, list2) =>
        {
            ShowCells(list1, list2);
        });
    }

    public void ShowCells(List<GameObject> cellsInArea, List<GameObject> cellsHard)
    {
        canvasGroup.alpha = 0.1f;

        foreach (var cell in cellsInArea) 
        {
            GameObject newImage = Instantiate(cellInArea,cell.transform.position,Quaternion.Euler(90f,0,0));
            newImage.transform.parent = cellInArea.transform.parent;

            newImage.SetActive(true);

            createdImages.Add(newImage);
        }
        foreach (var cell in cellsHard)
        {
            GameObject newImage = Instantiate(cellIsHard, cell.transform.position, Quaternion.Euler(90f, 0, 0));
            newImage.transform.parent = cellIsHard.transform.parent;

            newImage.SetActive(true);

            createdImages.Add(newImage);
        }
    }
    public void RemoveCells()
    {
        canvasGroup.alpha = 1;
        foreach(var cell in createdImages) { Destroy(cell.gameObject); }
    }
}
