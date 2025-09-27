using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ResourcesUIScript : MonoBehaviour
{
    [Header("Панель с прибавками")]
    [SerializeField] private GameObject resorcesChangerPanel;
    private RectTransform rectTransform;

    protected List<GameObject> _resourcesPanels = new List<GameObject>();
    protected List<TextMeshProUGUI> _resourcesText = new List<TextMeshProUGUI>();

    protected List<int> currentBatch = new List<int>{0,0,0,0,0};
    protected List<Action> currentActions = new List<Action>();

    private Queue<IEnumerator> changesResources = new Queue<IEnumerator>();


    protected bool _inOneWave;

    [SerializeField] private int timeToTakeChanges = 0;
    protected float _timer;

    private void Start()
    {
        resorcesChangerPanel.TryGetComponent(out rectTransform);

        foreach(Transform panel in resorcesChangerPanel.transform)
        {
            _resourcesPanels.Add(panel.gameObject);
            _resourcesText.Add(panel.GetChild(0).GetComponent<TextMeshProUGUI>());
        }

        StartCoroutine(CollectDataRoutine());
    }

    private void Update()
    {
        if(!_inOneWave && changesResources.Count != 0)
        {
            StartCoroutine(changesResources.Dequeue());
        }
    }

    public void ChangeResource(ResourcesEnum resourceType,int count, Action updatingAction)
    {
        currentActions.Add(updatingAction);

        switch (resourceType)
        {
            case ResourcesEnum.Influence:
                currentBatch[0] += count;
                break;
            case ResourcesEnum.Building:
                currentBatch[1] += count;
                break;
            case ResourcesEnum.Weapon:
                currentBatch[2] += count;
                break;
            case ResourcesEnum.Transport:
                currentBatch[3] += count;
                break;
            case ResourcesEnum.People:
                currentBatch[4] += count;
                break;
        }
    }

    private IEnumerator OpenChangePanel(List<int> currentList, List<Action> currentActionList)
    {
        CloseAllPanels();

        _inOneWave = true;

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x,-55f);

        for(int i = 0;i < currentList.Count; i++)
        {
            if (currentList[i] != 0)
            {
                _resourcesPanels[i].gameObject.SetActive(true);

                if(currentList[i] > 0) { _resourcesText[i].text = $"+{currentList[i]}"; }
                else { _resourcesText[i].text = $"{currentList[i]}"; }
            }
        }

        while (true)
        {
            if (rectTransform.anchoredPosition.y >= 0){ break;}

            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + 0.001f ) * Time.deltaTime;

            yield return null;
        }

        foreach(var action in currentActionList) { action.Invoke();}

        _inOneWave = false;

        yield return null;
    }
    private void CloseAllPanels()
    {
        foreach (var panel in _resourcesPanels) { panel.gameObject.SetActive(false); }
    }
    private IEnumerator CollectDataRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeToTakeChanges);

            if (currentBatch.All(x => x == 0)) continue;
            changesResources.Enqueue(OpenChangePanel(currentBatch, new List<Action>(currentActions)));

            currentBatch = new List<int> 
            {
                0,
                0,
                0,
                0,
                0
            };
            currentActions.Clear();

        }
    }
}
