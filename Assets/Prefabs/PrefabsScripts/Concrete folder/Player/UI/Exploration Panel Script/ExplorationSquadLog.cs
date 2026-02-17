using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class ExplorationSquadLog : MonoBehaviour
{
    [SerializeField] private ConstructorCanvas constructorCanvas;

    [SerializeField] private ScrollRect mainScroll;
    [SerializeField] private GameObject[] LogPanels;
    [SerializeField] private RectTransform contentRect;

    [SerializeField] private bool autoScroll_enbale = false;
    [SerializeField] private float offset;


    private Dictionary<AbstractSquad, List<ExplorationLogText>> logsBySquad = new();

    private VirtualList_Exploration virtualList;

    private void Start()
    {
        virtualList = new(LogPanels, contentRect, mainScroll.viewport.rect.height, offset,new float[0],this);virtualList.enabled = true;

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<ExplorationModule, AbstractSquad, object>((sender,squad, exploratedObject) =>
        {
            if (!logsBySquad.ContainsKey(squad)) { logsBySquad[squad] = new(); }

            if (exploratedObject != null) { 
                ExplorationWithObject(squad,exploratedObject);
            }
            else { ExplorationWithoutObject(squad);}
        });
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<ExplorationSquadList, AbstractSquad>((sender, squad) =>
        {
            virtualList.SetCurrentSquad(squad);
        });
    }

    private void ExplorationWithObject(AbstractSquad scoutSquad,object obj)
    {
        switch (obj) 
        {
            case AbstractSquad squad:
                var newLogSquad = new ExplorationLogText(textWithObject[typeof(AbstractSquad)][UnityEngine.Random.Range(0, textWithObject[typeof(AbstractSquad)].Count)]);

                LogPanelConstructor(newLogSquad, scoutSquad);
                logsBySquad[scoutSquad].Add(newLogSquad);
                break;
            case AbstractBuildings build:
                var newLogBuild = new ExplorationLogText(textWithObject[typeof(AbstractBuildings)][UnityEngine.Random.Range(0, textWithObject[typeof(AbstractBuildings)].Count)]);

                LogPanelConstructor(newLogBuild, scoutSquad);
                logsBySquad[scoutSquad].Add(newLogBuild);
                break;
        }
    }
    private void ExplorationWithoutObject(AbstractSquad scoutSquad)
    {
        var newLog = new ExplorationLogText(textWithoutObject[UnityEngine.Random.Range(0, textWithoutObject.Count)]);

        LogPanelConstructor(newLog, scoutSquad);
        logsBySquad[scoutSquad].Add(newLog);
    }

    private void LogPanelConstructor(ExplorationLogText explorationLogText, AbstractSquad scoutSquad)
    {
        var result = constructorCanvas.CalculateLayoutWithText(new (string text, float fontSize)[] {(explorationLogText.Time,28),(explorationLogText.Text,28)}, ConstructorCanvasTicket.Expl);

        virtualList.AddExplorationLog(explorationLogText,scoutSquad,result);
    }

    private Dictionary<Type, List<string>> textWithObject = new()
    {
        {typeof(AbstractSquad),new()
        {
            "При проведении разведки был замечен небольшой боевой элемент, расположившийся на окраине лесного массива. Отряд действовал организованно, выставив наблюдателей и обеспечив прикрытие основных сил. В итоге было установлено, что это отряд",
            "В ходе разведки выявлен боевой элемент, расположившийся вблизи дороги и контролировавший движение по ней. Отряд действовал слаженно, выставив охранение и обеспечив прикрытие. В итоге определено, что это",
            "При обследовании местности был замечен отряд, занявший позиции на возвышенности и организовавший наблюдение за окрестностями. Их действия указывали на подготовку к возможному контакту. Впоследствии было установлено, что это"
        } },
        {typeof(AbstractBuildings),new()
        {
            "В ходе разведки было обнаружено строение, используемое личным составом для размещения и организации деятельности. В итоге установлено, что это объект",
            "В процессе разведки замечено строение, имеющее признаки использования в военных целях. В результате установлено, что это объект",
            "При обследовании местности выявлено здание, в котором фиксировалась активность и движение людей. Впоследствии определено, что это"
        } }
    };
    private List<string> textWithoutObject = new()
{
    "В ходе разведки были получены общие сведения о местности.",
    "Обнаружены признаки активности, требующие дальнейшего анализа.",
    "Зафиксированы движения, не имеющие точной идентификации.",
    "Выявлены объекты, не представляющие непосредственной угрозы.",
    "Информация носит предварительный характер и требует уточнения.",
    "Отмечены изменения в окружающей обстановке.",
    "Данные собраны в условиях ограниченной видимости.",
    "Наблюдалась деятельность, не имеющая чёткой классификации.",
    "Сведения подтверждают наличие определённых факторов.",
    "Результаты разведки требуют дополнительной проверки.",
    "Обстановка остаётся стабильной, без явных признаков угрозы.",
    "Полученные данные носят общий характер и не содержат конкретики."
};

    private AbstractSquad currentSquad;


    private void CloseAllLogs(AbstractSquad squad) {
        foreach (GameObject item in LogPanels){item.SetActive(false);}
    }
    private float scrollTop,viewportHeight;

    private void Update()
    {
        virtualList.Update();
    }

    private class VirtualList_Exploration : VirtualList
    {
        public bool enabled { set; private get; } = false;

        private AbstractSquad currentSquad;

        private Dictionary<AbstractSquad, List<(ExplorationLogText, Vector2[])>> panelsInformation = new();
        private Dictionary<AbstractSquad, List<float>> panelsHeight = new();

        public VirtualList_Exploration(GameObject[] panels, RectTransform contentRect, float viewPortSize, float offset, 
            float[] startPanelHeights, MonoBehaviour listTakerMonobehaviour = null, 
            int autoscrollingSpeed = 400, bool autoScroll_enable = true) 
            : base(panels, contentRect, viewPortSize, offset, startPanelHeights, listTakerMonobehaviour, autoscrollingSpeed, autoScroll_enable)
        {

        }

        public void SetCurrentSquad(AbstractSquad squad)
        {
            currentSquad = squad;

            if (panelsHeight.ContainsKey(currentSquad)) { base.ChangeContext(panelsHeight[currentSquad].ToList()); }
        }
        public void AddExplorationLog(ExplorationLogText newLog, AbstractSquad squadFor,(float panelHeight, Vector2[] textsPos) parameters)
        {
            if (!panelsInformation.ContainsKey(squadFor)) { panelsInformation[squadFor] = new(); }
            if (!panelsHeight.ContainsKey(squadFor)) { panelsHeight[squadFor] = new();}

            panelsHeight[squadFor].Add(parameters.panelHeight);
            panelsInformation[squadFor].Add((newLog, parameters.textsPos));

            base.AddNewHeight(parameters.panelHeight);
        }

        public override void AbstractPanelSizer(GameObject panel, int informationIndex)
        {
            //Debug.Log($"Information index in expl:{informationIndex}");
            if(informationIndex >= panelsInformation[currentSquad].Count) {  return; }
            //Debug.LogError("Не упал");
            base.AbstractPanelSizer(panel, informationIndex);

            var panelTime = panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var panelText = panel.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            panelTime.text = panelsInformation[currentSquad][informationIndex].Item1.Time;
            panelText.text = panelsInformation[currentSquad][informationIndex].Item1.Text;

            panelTime.rectTransform.anchoredPosition = new Vector2(panelTime.rectTransform.anchoredPosition.x, panelsInformation[currentSquad][informationIndex].Item2[0].y);
            panelText.rectTransform.anchoredPosition = new Vector2(panelText.rectTransform.anchoredPosition.x, panelsInformation[currentSquad][informationIndex].Item2[1].y);

            panelTime.ForceMeshUpdate();
            panelText.ForceMeshUpdate();
        }
        public void Update() {if(currentSquad!=null && enabled) base.WorkingState(); }
    }
}
public class ExplorationLogText
{
    public string Text { private set; get; }
    public string Time { private set; get; }

    public ExplorationLogText(string text)
    {
        Time = "["+TimeControllerScript.GetCurrentTime()+"]";

        Text = text;
    }
}
