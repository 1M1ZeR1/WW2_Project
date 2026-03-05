using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class ListScript : MonoBehaviour
{
    private enum ListDirection
    {
        None,
        Up,
        Down
    }

    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private GameObject[] panels;
    [SerializeField] private float offset;

    private Dictionary<GameObject, int> panelInformationIndex = new();

    private RectTransform closedStart_Current, closedEnd_Current;

    private float scrollTop;
    private float viewPortSize;

    private RectTransform contentSize;

    private List<float> panelsHeights = new();

    private VirtualList virtualList;

    private void Awake()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }
    void Start()
    {
        contentSize = GetComponent<RectTransform>();
        viewPortSize = scrollRect.viewport.rect.height;

        for (int i = 0; i < 2; i++) { panelsHeights.Add(Random.Range(200, 450)); }

        virtualList = new VirtualList(panels,contentSize,scrollRect.viewport.rect.height,offset,panelsHeights.ToArray(),this,200,false);
    }

    float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer>=3f)
        {
            timer = 0f;
            virtualList.AddNewHeight(Random.Range(200, 450));
        }
        virtualList.WorkingState();
    }
}

public class VirtualList
{
    private enum ListDirection
    {
        None,
        Up,
        Down
    }
    private bool autoScroll = true;
    private int autoscrollingSpeed;
    private bool autoScroll_enable = false;

    private Coroutine scrollingCoroutine;

    private GameObject[] panels;
    private RectTransform contentRect;
    private float viewPortSize;
    private float offset;
    private MonoBehaviour listTaker;

    private Dictionary<GameObject, int> panelInformationIndex = new();

    private RectTransform closedStart_Current, closedEnd_Current;

    private float scrollTop;


    private List<float> panelHeights;
    public VirtualList(GameObject[] panels, RectTransform contentRect, float viewPortSize, float offset, float[] startPanelHeights,
        MonoBehaviour listTakerMonobehaviour = null, int autoscrollingSpeed = 200, bool autoScroll_enable = true, bool needStartConstructor = false)
    {
        this.autoScroll_enable = autoScroll_enable;

        this.panels = panels;
        this.contentRect = contentRect;
        this.viewPortSize = viewPortSize;
        this.offset = offset;
        this.listTaker = listTakerMonobehaviour;
        this.autoscrollingSpeed = autoscrollingSpeed;

        closedStart_Current = panels[0].GetComponent<RectTransform>();

        panelHeights = startPanelHeights.ToList();

        if(needStartConstructor)StartConstructor(startPanelHeights);
    }

    public void StartCoroutine(IEnumerator coroutine) 
    {
        if (listTaker != null) { listTaker.StartCoroutine(coroutine); }
        else { ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(coroutine); }
    }
    private void StartConstructor(float[] startPanelsHeights)
    {

        float currentOffset = 0;

        int panelsCount = panels.Length;
        int currentPanelIndex = 0;

        while (true)
        {
            float y_position_panel = currentOffset;

            var panelRect = panels[currentPanelIndex].GetComponent<RectTransform>();

            float heightDifferens = viewPortSize - (currentOffset + panelRect.sizeDelta.y);
            if (heightDifferens < 0 && -heightDifferens > panelRect.sizeDelta.y)
            {
                closedEnd_Current = panels[currentPanelIndex - 1].GetComponent<RectTransform>();

                AbstractPanelSizer(panels[currentPanelIndex], currentPanelIndex);

                panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, -(currentOffset + panelRect.sizeDelta.y / 2));

                currentOffset += panelRect.sizeDelta.y + offset;
                panels[currentPanelIndex].SetActive(true);
                panelInformationIndex[panels[currentPanelIndex]] = currentPanelIndex;

                if (listTaker != null) { listTaker.StartCoroutine(UpdateContentSize(panels[currentPanelIndex].GetComponent<RectTransform>())); }
                else { ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(UpdateContentSize(panels[currentPanelIndex].GetComponent<RectTransform>())); }

                return;
            }

            AbstractPanelSizer(panels[currentPanelIndex], currentPanelIndex);

            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, -(currentOffset + panelRect.sizeDelta.y / 2));

            currentOffset += panelRect.sizeDelta.y + offset;
            panels[currentPanelIndex].SetActive(true);
            panelInformationIndex[panels[currentPanelIndex]] = currentPanelIndex;

            currentPanelIndex++;

            if (currentPanelIndex >= panelsCount) { return; }

            if (currentPanelIndex >= panelHeights.Count) { closedEnd_Current = panels[currentPanelIndex - 1].GetComponent<RectTransform>();

                if (listTaker != null) { listTaker.StartCoroutine(UpdateContentSize(panels[currentPanelIndex-1].GetComponent<RectTransform>())); }
                else { ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(UpdateContentSize(panels[currentPanelIndex-1].GetComponent<RectTransform>())); }return; }
        }
    }

    private IEnumerator UpdateContentSize(RectTransform lastPanel)
    {
        yield return null;

        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, -lastPanel.anchoredPosition.y + lastPanel.sizeDelta.y / 2);
    }
    private void FixContent(float y_position)
    {
        contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, y_position);
    }


    public void AddNewHeight(float height) 
    {
        panelHeights.Add(height);

        if(closedEnd_Current == null || !panelInformationIndex.ContainsKey(closedEnd_Current.gameObject)) { return; }
        if (panelHeights.Count - (panelInformationIndex[closedEnd_Current.gameObject]+1) >= 1)
        {
            var newClosedEnd = EnableNextPanel();
            if(newClosedEnd != null) { closedEnd_Current = newClosedEnd.GetComponent<RectTransform>(); }
        }
    }

    private void AutoScrolling(GameObject nextPanel)
    {
        var panelRect = nextPanel.GetComponent<RectTransform>();

        if(listTaker != null)
        {
            if(scrollingCoroutine != null) { StopScrollingCoroutine(); }
            scrollingCoroutine = listTaker.StartCoroutine(SmoothScrolling(contentRect.anchoredPosition.y +
                        (panelRect.sizeDelta.y / 2 + (-panelRect.anchoredPosition.y) - viewPortSize - offset)));
        }
        else
        {
            if(scrollingCoroutine != null) { StopScrollingCoroutine(); }
            scrollingCoroutine = ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(SmoothScrolling(contentRect.anchoredPosition.y +
                        (panelRect.sizeDelta.y / 2 + (-panelRect.anchoredPosition.y) - viewPortSize - offset)));
        }
    }
    private void StopScrollingCoroutine() 
    {
        if (!autoScroll_enable) return;

        if(listTaker != null) { listTaker.StopCoroutine(scrollingCoroutine); }
        else { ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStopper(scrollingCoroutine); }
    }

    private IEnumerator SmoothScrolling(float y_position)
    {
        while (contentRect.anchoredPosition.y < y_position)
        {
            contentRect.anchoredPosition += new Vector2(0f, Time.deltaTime*autoscrollingSpeed);

            yield return null;
        }

        contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, y_position);
    }

    public void WorkingState()
    {
        if(panelHeights.Count == 0) return;

        if (contentRect.offsetMin.x != 0 || contentRect.offsetMax.x != 0) 
        {
            contentRect.offsetMax = new Vector2(0f,contentRect.offsetMax.y);
            contentRect.offsetMin = new Vector2(0f, contentRect.offsetMin.y);
        }

        if (scrollTop != -contentRect.anchoredPosition.y)
        {
            if (contentRect.anchoredPosition.y < 0) { FixContent(0); return; }
            float maxScroll = contentRect.sizeDelta.y - viewPortSize;
            if (contentRect.anchoredPosition.y > maxScroll && maxScroll>=0)
            {
                FixContent(maxScroll);
                return;
            }

            bool inUp = scrollTop > -contentRect.anchoredPosition.y;
            scrollTop = -contentRect.anchoredPosition.y;

            if (inUp) { UpdateConstructor(ListDirection.Up); }
            else { UpdateConstructor(ListDirection.Down); }
        }
    }

    private void UpdateConstructor(ListDirection listDirection)
    {
        if (listDirection == ListDirection.Up)
        {

            if (closedStart_Current.sizeDelta.y / 2 + (-closedStart_Current.anchoredPosition.y) < -scrollTop)
            {
                DisableLastPanel(closedStart_Current);

                var nextIndex_closedStart = System.Array.IndexOf(panels, closedStart_Current.gameObject) + 1;

                if (nextIndex_closedStart >= panels.Length) { nextIndex_closedStart = 0; }

                closedStart_Current = panels[nextIndex_closedStart].GetComponent<RectTransform>();
            }

            if (closedEnd_Current.sizeDelta.y / 2 + (-closedEnd_Current.anchoredPosition.y) < viewPortSize + (-scrollTop))
            {
                if (panelInformationIndex[closedEnd_Current.gameObject] + 1 >= panelHeights.Count) { return; }

                var nextIndex_closedEnd = System.Array.IndexOf(panels, closedEnd_Current.gameObject) + 1;

                if (nextIndex_closedEnd >= panels.Length) { nextIndex_closedEnd = 0; }

                closedEnd_Current = panels[nextIndex_closedEnd].GetComponent<RectTransform>();

                EnableNextPanel();
            }
        }

        if (listDirection == ListDirection.Down)
        {
            if ((closedEnd_Current.sizeDelta.y / 2 + (-closedEnd_Current.anchoredPosition.y) > viewPortSize + (-scrollTop)))
            {
                DisableNextPanel(closedEnd_Current);

                var lastIndex_closedEnd = System.Array.IndexOf(panels, closedEnd_Current.gameObject) - 1;

                if (lastIndex_closedEnd < 0) { lastIndex_closedEnd = panels.Length - 1; }

                closedEnd_Current = panels[lastIndex_closedEnd].GetComponent<RectTransform>();
            }

            if ((-closedStart_Current.anchoredPosition.y) - closedStart_Current.sizeDelta.y / 2 > (-scrollTop))
            {
                var lastIndex_closedStart = System.Array.IndexOf(panels, closedStart_Current.gameObject) - 1;

                if (lastIndex_closedStart < 0) { lastIndex_closedStart = panels.Length - 1; }

                closedStart_Current = panels[lastIndex_closedStart].GetComponent<RectTransform>();

                EnableLastPanel();
            }
        }
    }

    private void DisableLastPanel(RectTransform rectPanel) => panels[GetNeighborIndex_Last(System.Array.IndexOf(panels, rectPanel.gameObject))].gameObject.SetActive(false);
    private void DisableNextPanel(RectTransform rectPanel) => panels[GetNeighborIndex_Next(System.Array.IndexOf(panels, rectPanel.gameObject))].gameObject.SetActive(false);

    private int GetNeighborIndex_Next(int initialIndex)
    {
        if (initialIndex + 1 >= panels.Length) { return 0; }
        return initialIndex + 1;
    }
    private int GetNeighborIndex_Last(int initialIndex)
    {
        if (initialIndex - 1 < 0) { return panels.Length - 1; }
        return initialIndex - 1;
    }

    private GameObject EnableNextPanel()
    {
        var nextPanelIndex = GetNeighborIndex_Next(System.Array.IndexOf(panels, closedEnd_Current.gameObject));

        if (!SizeConstructor(closedEnd_Current.gameObject, ListDirection.Up)) return null;
        //Debug.LogError("Здесь собака зарыта");
        ActivationConstructor(closedEnd_Current, panels[nextPanelIndex].GetComponent<RectTransform>(), ListDirection.Up);

        //Debug.LogError($"Try to activate:{panels[nextPanelIndex].name}");
        panels[nextPanelIndex].gameObject.SetActive(true);

        if (autoScroll && autoScroll_enable) { AutoScrolling(panels[nextPanelIndex]); }

        return panels[nextPanelIndex];
    }

    private void EnableLastPanel()
    {
        autoScroll = false; StopScrollingCoroutine();

        var lastPanelIndex = GetNeighborIndex_Last(System.Array.IndexOf(panels, closedStart_Current.gameObject));

        if (!SizeConstructor(closedStart_Current.gameObject, ListDirection.Down)) return;

        ActivationConstructor(closedStart_Current, panels[lastPanelIndex].GetComponent<RectTransform>(), ListDirection.Down);

        panels[lastPanelIndex].SetActive(true);
    }

    private bool SizeConstructor(GameObject mainPanel, ListDirection listDirection)
    {
        int indexOfInformation;

        if (listDirection == ListDirection.Up)
        {
            indexOfInformation = panelInformationIndex[mainPanel] + 1;

            //Debug.Log($"Panel height count:{panelHeights.Count}| Index:{indexOfInformation} | Panel:{mainPanel.name}");
            if (indexOfInformation >= panelHeights.Count) { autoScroll = true; return false; }
            //Debug.LogError("Going");
            var nextPanel = panels[GetNeighborIndex_Next(System.Array.IndexOf(panels, mainPanel))];

            AbstractPanelSizer(nextPanel, indexOfInformation);
            //Debug.LogError("GoingX2");
            if (listTaker != null) { listTaker.StartCoroutine(UpdateContentSize(nextPanel.GetComponent<RectTransform>())); }
            else { ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(UpdateContentSize(nextPanel.GetComponent<RectTransform>())); }
        }
        if (listDirection == ListDirection.Down)
        {
            indexOfInformation = panelInformationIndex[mainPanel] - 1;

            if (indexOfInformation < 0) return false;

            AbstractPanelSizer(panels[GetNeighborIndex_Last(System.Array.IndexOf(panels, mainPanel))], indexOfInformation);

            if (listTaker != null) {listTaker.StartCoroutine(UpdateContentSize(panels[GetNeighborIndex_Next(System.Array.IndexOf(panels, closedEnd_Current.gameObject))].GetComponent<RectTransform>())); }
            else { ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(UpdateContentSize(panels[GetNeighborIndex_Next(System.Array.IndexOf(panels, closedEnd_Current.gameObject))].GetComponent<RectTransform>())); }
        }

        return true;
    }

    public virtual void AbstractPanelSizer(GameObject panel, int informationIndex)
    {
        var rect = panel.GetComponent<RectTransform>();

        rect.sizeDelta = new Vector2(rect.sizeDelta.x, panelHeights[informationIndex]);

        panelInformationIndex[panel] = informationIndex;
        //Debug.Log($"Name:{panel.name}|New index:{informationIndex}");
    }

    public virtual void ChangeContext(List<float> panelHeights, bool beOnTop = false) 
    {
        this.panelHeights = panelHeights.ToList();

        if (!beOnTop) { StartConstructor(panelHeights.ToArray()); return; }

        if(panelHeights.Count >= 10) { ReConstructAbove(panelHeights.ToArray()); }
        else { StartConstructor(panelHeights.ToArray()); }
    }
    public virtual void ReConstructAbove(float[] panelHeights)
    {
        float currentOffset = viewPortSize;
        int currentPanelIndex = panels.Length - 1;
        int panelsCount = panels.Length;
        int informationIndex = panelHeights.Length - 1;

        Debug.LogError(informationIndex);

        while (true)
        {
            var panelRect = panels[currentPanelIndex].GetComponent<RectTransform>();

            currentOffset -= panelRect.sizeDelta.y;

            if (currentOffset < -panelRect.sizeDelta.y)
            {
                closedEnd_Current = panels[currentPanelIndex + 1].GetComponent<RectTransform>();

                AbstractPanelSizer(panelRect.gameObject, informationIndex);

                panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x,
                    -currentOffset - panelRect.sizeDelta.y / 2);

                currentOffset += panelRect.sizeDelta.y;
                panels[currentPanelIndex].SetActive(true);
                panelInformationIndex[panels[currentPanelIndex]] = informationIndex;

                if (listTaker != null) { listTaker.StartCoroutine(UpdateContentSize(panelRect));}
                else { ServiceRegistry.WorkWithService<MonobehaviourMaster>().CoroutineStarter(UpdateContentSize(panelRect));}

                return;
            }

            AbstractPanelSizer(panelRect.gameObject, informationIndex);

            Debug.LogError(informationIndex);

            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x,
                -currentOffset - panelRect.sizeDelta.y / 2);

            panels[currentPanelIndex].SetActive(true);
            panelInformationIndex[panels[currentPanelIndex]] = informationIndex;

            currentOffset -= offset;
            currentPanelIndex--;
            informationIndex--;

            if (currentPanelIndex < 0) { return; }
        }
    }

    public void DisableAllPanels() {
        foreach (var item in panels)
        {
            item.SetActive(false);
        }
    }

    private void ActivationConstructor(RectTransform rectPanel, RectTransform panelToConstruct, ListDirection listDirection)
    {
        if (listDirection == ListDirection.Up)
        {
            panelToConstruct.anchoredPosition = new Vector2(panelToConstruct.anchoredPosition.x, -(
                (-rectPanel.anchoredPosition.y) + rectPanel.sizeDelta.y / 2 +
                offset + panelToConstruct.sizeDelta.y / 2));
        }

        if (listDirection == ListDirection.Down)
        {
            panelToConstruct.anchoredPosition = new Vector2(panelToConstruct.anchoredPosition.x, -(
                (-rectPanel.anchoredPosition.y) - rectPanel.sizeDelta.y / 2 -
                offset - panelToConstruct.sizeDelta.y / 2));
        }
    }
}
