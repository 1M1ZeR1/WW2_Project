using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoutsSummaryUIScript : MonoBehaviour
{
    [Header("Информационная панель")]
    [SerializeField] private GameObject informationPanel;

    [Header("Контроллер разведки")]
    [SerializeField] private GameObject explorationControllerObject;
    private ExplorationController explorationControllerScript;

    [Header("Панель разведки")]
    [SerializeField] private GameObject explorationSummaryPanel;

    [Header("Content")]
    [SerializeField] private GameObject content;

    protected GameObject _currentInteractableCell;
    protected bool _seeRaportNow = false;

    protected GameObject _headerPanel;
    protected GameObject _buildingCountTypePanel;
    protected GameObject _squadsNameCountPanel;
    protected GameObject _reasonPanel;

    protected TextMeshProUGUI _explorationHeaderText;
    protected TextMeshProUGUI _explorationBuildingCountTypeText;
    protected TextMeshProUGUI _explorationSquadsNameCountText;
    protected TextMeshProUGUI _explorationReasonText;

    [Header("Держатель видов строений")]
    [SerializeField] private GameObject buildingTypeTaker;

    [Header("Держатель имён отрядов")]
    [SerializeField] private GameObject squadsNameTaker;

    private void Start()
    {
        explorationControllerObject.TryGetComponent(out explorationControllerScript);

        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject>((sender, cell) =>
        {
            PlayerInteractWith(cell);
        });

        _headerPanel = content.transform.GetChild(0).gameObject;
        _buildingCountTypePanel = content.transform.GetChild(1).gameObject;
        _squadsNameCountPanel = content.transform.GetChild(2).gameObject;
        _reasonPanel = content.transform.GetChild(3).gameObject;

        _explorationHeaderText = _headerPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _explorationBuildingCountTypeText = _buildingCountTypePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _explorationSquadsNameCountText = _squadsNameCountPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _explorationReasonText = _reasonPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    private void PlayerInteractWith(GameObject cell)
    {
        if (ChoosingScript.CheckInChoosing()) { return; }
        if(!ServiceRegistry.WorkWithController<CellController>().FastDrop_IsAllies(cell) && explorationControllerScript.CheckHasExploration(cell)) 
        {
            if (_currentInteractableCell == cell && _seeRaportNow == true) { explorationSummaryPanel.SetActive(false); _seeRaportNow = false;return; }

            CloseAllPanels();
            Cleaner(_buildingCountTypePanel.transform);
            Cleaner(_squadsNameCountPanel.transform);

            if (informationPanel.activeSelf) { informationPanel.SetActive(false); }

            PanelConstructor(explorationControllerScript.GetExplorationSummary(cell));

            _currentInteractableCell = cell;
            _seeRaportNow = true;
        }
        else
        {
            if (_seeRaportNow)
            {
                explorationSummaryPanel.SetActive(false);_seeRaportNow =false;
            }
            return;
        }
    }
    private void PanelConstructor(List<string[]> allInformation)
    {
        _explorationHeaderText.text = $"\t\t\tРАПОРТ\r\n\tДокладываю, что {allInformation[0][0]} года в районе {allInformation[0][1]} наблюдалась активнасть противника. Согласно заявлению гражданина {allInformation[0][2]}, замечены следующие элементы на территории:";

        for(int i = 1;i< allInformation.Count;i++)
        {
            if(i == 1)
            {
                if (allInformation[i] == null) { continue; }
                _explorationBuildingCountTypeText.text = $"В настоящее время известно о {allInformation[1][0]} строений на территории. Среди них мы можем выделить:";
                for(int j = 1; j < allInformation[i].Length; j++)
                {
                    GameObject newBuildPanel = Instantiate(buildingTypeTaker,_buildingCountTypePanel.transform);

                    newBuildPanel.GetComponent<TextMeshProUGUI>().text = allInformation[i][j];

                    newBuildPanel.SetActive(true);
                }

                _buildingCountTypePanel.SetActive(true);
            }
            if(i == 2)
            {
                if (allInformation[i] == null) { continue; }
                _explorationSquadsNameCountText.text = $"Так же распознали {allInformation[i][0]} отрядов. Подробная информация приведена ниже:";
                for (int j = 1; j < allInformation[i].Length; j++)
                {
                    GameObject newSquadPanel = Instantiate(squadsNameTaker, _squadsNameCountPanel.transform);

                    newSquadPanel.GetComponent<TextMeshProUGUI>().text = allInformation[i][j];

                    newSquadPanel.SetActive(true);
                }

                _squadsNameCountPanel.SetActive(true);
            }
        }

        //_explorationReasonText.text = "Разведка не даёт 100% гарантии о правдивой информации. Возможно понадобится дополнительная разведка через некоторое время";
        //_reasonPanel.SetActive(true);

        explorationSummaryPanel.SetActive(true);
    }

    private void CloseAllPanels()
    {
        _buildingCountTypePanel.SetActive(false);
        _squadsNameCountPanel.SetActive(false);
        _reasonPanel.SetActive(false);
    }
    private void Cleaner(Transform transformToClean)
    {
        for (int i = transformToClean.childCount - 1; i >= 0; i--)
        {
            Transform child = transformToClean.GetChild(i);
            if (child.name.Contains("Clone"))
            {
                Destroy(child.gameObject);
            }
        }
    }
    protected string[] _earlyPreparations = new string[]
    {
        "Нас почти засекли вражеские ищейки, нами принято решение отступить. Следует отправитьcя в разведку через некоторое время.",//0
        "Мы смогли подобраться поближе, но усталость дала своё. Следующий этап разведки будет очень полезен.",//1
        "Собрали и так много информации. Решили не рисковать и отступить. Думаю следующим шагом мы раскроем все карты врага."//2
    };
}
