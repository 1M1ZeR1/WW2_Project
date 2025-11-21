using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapConfigurator : MonoBehaviour
{
    private enum TypeOfConfiguration
    {
        None,
        Neighbores,
        Type,
        Builds,
        Bases,
        Allies,
        Enemys
    }
    [SerializeField] private TypeOfConfiguration workWith = TypeOfConfiguration.None;

    [SerializeField] private bool isInConfigureMode;

    [Header("Камера")]
    [SerializeField] private GameObject playerCamera;

    [Header("Канвас")]
    [SerializeField] private GameObject playerCanvas;

    [Header("Держатель контроллеров")]
    [SerializeField] private GameObject controllersTaker;

    [Header("Канвас конфигуратор")]
    [SerializeField] private GameObject configureCanvas;
    private TextMeshProUGUI typeText;

    private SelectedObjectScript selectedObjectScript;

    private ParametersCellsDataHolder parametersHolder;
    private Dictionary<GameObject, Parameters> currentDictionary;

    protected bool inNeighboreConfigure = false;
    protected GameObject currentSelectedCell;

    [Header("Базовый материал")]
    [SerializeField] private Material baseMaterial;

    [Header("Материал клетки с настроенными соседями")]
    [SerializeField] private Material cellsWithNeighbores;//осторожно, можем потерять

    [Header("Материал клетки с настроенными типами")]
    [SerializeField] private Material cellsWithType;//осторожно, можем потерять
    protected int _clicksToSelectType = 0;

    [Header("Материал клетки базы")]
    [SerializeField] private Material cellIsBase;

    [Header("Материал для показа соседей")]
    [SerializeField] private Material areaShow;

    [Header("Материал для клеток противника")]
    [SerializeField] private Material areaEnemy;

    [Header("Материал для клеток союзника")]
    [SerializeField] private Material areaAlly;

    [SerializeField] private PlayerInput playerInput;

    private void Start()
    {
        if (isInConfigureMode)
        {
            playerInput.SwitchCurrentActionMap("ConfiguratorMap");

            typeText = configureCanvas.GetComponentInChildren<TextMeshProUGUI>();

            parametersHolder = Resources.Load<ParametersCellsDataHolder>("CellsParameters_V1");
            currentDictionary = ListToDictionaryConverter(parametersHolder.GetCellsParameters());

            if(currentDictionary == null) { currentDictionary = new Dictionary<GameObject, Parameters>(); }

            foreach (var component in playerCamera.GetComponents<Component>())
            {
                if(component is MonoBehaviour behaivor)
                {
                    behaivor.enabled = false;

                    if(behaivor is CameraMovementScript || behaivor is PlayerInput)
                    {
                        behaivor.enabled = true;
                    }
                    if(behaivor is SelectedObjectScript)
                    {
                        selectedObjectScript = component.GetComponent<SelectedObjectScript>();
                        behaivor.enabled = true;
                        selectedObjectScript.SetInConfigureMode();
                    }
                }
            }

            playerCanvas.SetActive(false);

            controllersTaker.SetActive(false);

            configureCanvas.SetActive(true);

            selectedObjectScript.selectingCellsForConfigure += GetSelectedCell;

            ShowAllConfiguredCells();
        }
        else
        {
            playerInput.SwitchCurrentActionMap("Player");
        }
    }
    private void ShowAllConfiguredCells()
    {
        foreach(var cell in currentDictionary.Keys)
        {
            if(workWith == TypeOfConfiguration.Neighbores) { cell.GetComponent<MeshRenderer>().material = cellsWithNeighbores; }
            if(workWith == TypeOfConfiguration.Type) {
                if (currentDictionary[cell].cellType != CellTypes_enum.None)cell.GetComponent<MeshRenderer>().material = cellsWithType; 
            }
            if(workWith == TypeOfConfiguration.Bases)
            {
                if (currentDictionary[cell].isBase) { cell.GetComponent<MeshRenderer>().material = cellIsBase; }
            }
            if(workWith == TypeOfConfiguration.Builds)
            {
                if (currentDictionary[cell].buildings != null && currentDictionary[cell].buildings.Count != 0) { cell.GetComponent<MeshRenderer>().material = cellsWithType; }
            }
            if(workWith == TypeOfConfiguration.Enemys || workWith == TypeOfConfiguration.Allies)
            {
                if (currentDictionary[cell].controlSide == ControlSide.enemys) { cell.GetComponent<MeshRenderer>().material = areaEnemy; }
                if (currentDictionary[cell].controlSide == ControlSide.allies) { cell.GetComponent<MeshRenderer>().material = areaAlly; }
            }
        }
    }
    public void AddNeighboreMode(InputAction.CallbackContext context)
    {
        if( context.performed && isInConfigureMode)
        {
            if (!currentDictionary.ContainsKey(currentSelectedCell))
            {
                currentDictionary.Add(selectedObjectScript.GetSelectedObject(), new Parameters()
                {
                    currentCell = currentSelectedCell,
                    currentCellName = currentSelectedCell.name
                }) ;
            }

            if (inNeighboreConfigure) 
            { 
                inNeighboreConfigure = false; selectedObjectScript.selectingCellsForConfigure -= AddNeighbore;
                selectedObjectScript.selectingCellsForConfigure += GetSelectedCell;

                ShowAllConfiguredCells();
                return; }
            else
            {
                inNeighboreConfigure = true; selectedObjectScript.selectingCellsForConfigure += AddNeighbore;
                selectedObjectScript.selectingCellsForConfigure -= GetSelectedCell; 

                ShowArea(currentSelectedCell);
                return;
            }
        }
    }
    private void AddNeighbore(GameObject cell)
    {
        if (currentDictionary[currentSelectedCell].neighboresCells.Contains(cell)) { 
            currentDictionary[currentSelectedCell].neighboresCells.Remove(cell);
            currentDictionary[currentSelectedCell].neighboresCellsNames.Remove(cell.name);
            return;
        }
        currentDictionary[currentSelectedCell].neighboresCells.Add(cell);
        currentDictionary[currentSelectedCell].neighboresCellsNames.Add(cell.name);
    }
    public void AddType(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ShowAllConfiguredCells();
            if (!currentDictionary.ContainsKey(currentSelectedCell))
            {
                currentDictionary.Add(selectedObjectScript.GetSelectedObject(), new Parameters()
                {
                    currentCell = currentSelectedCell,
                    currentCellName = currentSelectedCell.name
                });
            }

            if(_clicksToSelectType > 3) { _clicksToSelectType = 0; }

            if(_clicksToSelectType == 0) { typeText.text = "Тип:равнина"; currentDictionary[currentSelectedCell].cellType = CellTypes_enum.Plain;
                currentDictionary[currentSelectedCell].height = Random.Range(10f,20f);
                _clicksToSelectType++; return; }
            if (_clicksToSelectType == 1) { typeText.text = "Тип:лес"; currentDictionary[currentSelectedCell].cellType = CellTypes_enum.Forest;
                currentDictionary[currentSelectedCell].height = Random.Range(20f, 30f);
                _clicksToSelectType++; return; }
            if (_clicksToSelectType == 2) { typeText.text = "Тип:река"; currentDictionary[currentSelectedCell].cellType = CellTypes_enum.River;
                currentDictionary[currentSelectedCell].height = Random.Range(40f, 50f);
                _clicksToSelectType++; return; }
            if (_clicksToSelectType == 3) { typeText.text = "Тип:берег"; currentDictionary[currentSelectedCell].cellType = CellTypes_enum.Beach;
                currentDictionary[currentSelectedCell].height = Random.Range(30f, 40f);
                _clicksToSelectType++; return; }
        }
    }
    public void SetCellBase(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentDictionary[currentSelectedCell].isBase) { currentDictionary[currentSelectedCell].isBase = false; }
            else { currentDictionary[currentSelectedCell].isBase = true; }
        }
    }
    private void GetSelectedCell(GameObject cell) { currentSelectedCell = cell; _clicksToSelectType = 0; typeText.text = ""; 
    if(workWith == TypeOfConfiguration.Builds) { CameraMovementScript.BlockMovement(); }
    }

    private void ShowArea(GameObject cell)
    {
        cell.GetComponent<MeshRenderer>().material = areaShow;
        foreach(var otherCell in currentDictionary[cell].neighboresCells)
        {
            otherCell.GetComponent<MeshRenderer>().material = areaShow;
        }
    }
    private void CloseArea(GameObject cell)
    {
        cell.GetComponent<MeshRenderer>().material = baseMaterial;
        foreach (var otherCell in currentDictionary[cell].neighboresCells)
        {
            otherCell.GetComponent<MeshRenderer>().material = baseMaterial;
        }
    }

    public void SaveChanges(InputAction.CallbackContext context)
    {
        if (context.performed) { Debug.Log("Попытка сохранения"); parametersHolder.SetNewParameters(DictionaryToListConverter(currentDictionary));}
    }
    private Dictionary<GameObject,Parameters> ListToDictionaryConverter(List<Parameters> list)
    {
        Dictionary<GameObject, Parameters> result = new Dictionary<GameObject, Parameters>();

        if (list.Count !=0 && list[0].currentCell == null)
        {
            foreach (Parameters param in list)
            {
                GameObject proccesingCell = GameObject.Find(param.currentCellName);
                param.currentCell = proccesingCell;
                result.Add(proccesingCell, param);

                result[proccesingCell].neighboresCells.Clear();
                foreach (string names in result[proccesingCell].neighboresCellsNames)
                {
                    result[proccesingCell].neighboresCells.Add(GameObject.Find(names));
                }
            }
            return result;
        }

        foreach (Parameters param in list)
        {
            result.Add(param.currentCell, param);
        }

        return result;
    }
    private List<Parameters> DictionaryToListConverter(Dictionary<GameObject, Parameters> dictionary)
    {
        List<Parameters> result = new List<Parameters>();

        foreach(var param in dictionary.Values)
        {
            param.currentCell = dictionary.FirstOrDefault(entry => entry.Value == param).Key;

            result.Add(param);
        }

        return result;
    }
    public void AddBuildingToCell_Foxhole(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //if (currentDictionary[currentSelectedCell].buildings.Any(x => x is BuildsEnum.Foxhole))
            //{
            //    Debug.Log($"Удаляю:Foxhole с клетки:{currentSelectedCell}");
            //    currentDictionary[currentSelectedCell].buildings.RemoveAll(x => x is BuildsEnum.Foxhole);
            //}
            //else
            //{
            //    Debug.Log($"Добавляю:Foxhole на клетку:{currentSelectedCell}");
            //    currentDictionary[currentSelectedCell].buildings.Add(BuildsEnum.Foxhole);
            //}
            CameraMovementScript.UnBlockMovement();
            ShowAllConfiguredCells();
        }
    }
    public void AddBuildingToCell_Camp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //if (currentDictionary[currentSelectedCell].buildings.Any(x => x is BuildsEnum.Camp))
            //{
            //    Debug.Log($"Удаляю:CampBuild с клетки:{currentSelectedCell}");
            //    currentDictionary[currentSelectedCell].buildings.RemoveAll(x => x is BuildsEnum.Camp);
            //}
            //else
            //{
            //    Debug.Log($"Добавляю:CampBuild на клетку:{currentSelectedCell}");
            //    currentDictionary[currentSelectedCell].buildings.Add(BuildsEnum.Camp);
            //}
            CameraMovementScript.UnBlockMovement();
            ShowAllConfiguredCells();
        }
    }
    public void AddBuildingToCell_Fort(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //if (currentDictionary[currentSelectedCell].buildings.Any(x => x is BuildsEnum.Fort))
            //{
            //    Debug.Log($"Удаляю:FortBuild с клетки:{currentSelectedCell}");
            //    currentDictionary[currentSelectedCell].buildings.RemoveAll(x => x is BuildsEnum.Fort);
            //}
            //else
            //{
            //    Debug.Log($"Добавляю:FortBuild на клетку:{currentSelectedCell}");
            //    currentDictionary[currentSelectedCell].buildings.Add(BuildsEnum.Fort);
            //}
            CameraMovementScript.UnBlockMovement();
            ShowAllConfiguredCells();
        }
    }

    public void SetControl_Enemys(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if (currentDictionary[currentSelectedCell].controlSide == ControlSide.enemys) { currentDictionary[currentSelectedCell].controlSide = ControlSide.none; }
            else { currentDictionary[currentSelectedCell].controlSide = ControlSide.enemys; }

            ShowAllConfiguredCells();
        }
    }
    public void SetControl_Allies(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentDictionary[currentSelectedCell].controlSide == ControlSide.allies) { currentDictionary[currentSelectedCell].controlSide = ControlSide.none; }
            else { currentDictionary[currentSelectedCell].controlSide = ControlSide.allies; }

            ShowAllConfiguredCells();
        }
    }
}


[CustomEditor(typeof(GameObject))]
public class GameObjectParametersEditor : Editor
{
    private Parameters currentEntry;
    private ParametersCellsDataHolder holder;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Найти параметры"))
        {
            GameObject selected = (GameObject)target;
            holder = Resources.Load<ParametersCellsDataHolder>("CellsParameters_V1");

            if (holder != null)
            {
                var parametersList = holder.GetCellsParameters();
                currentEntry = parametersList.FirstOrDefault(p => p.currentCellName == selected.name);
            }
        }

        if (currentEntry != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Редактирование параметров:", EditorStyles.boldLabel);

            currentEntry.currentCellName = EditorGUILayout.TextField("Имя", currentEntry.currentCellName);
            currentEntry.controlSide = (ControlSide)EditorGUILayout.EnumPopup("Сторона", currentEntry.controlSide);
            currentEntry.cellType = (CellTypes_enum)EditorGUILayout.EnumPopup("Тип локации", currentEntry.cellType);
            currentEntry.isFront = EditorGUILayout.Toggle("Фронт", currentEntry.isFront);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Список построек:", EditorStyles.boldLabel);

            for (int i = 0; i < currentEntry.buildings.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                currentEntry.buildings[i] = (BuildData)EditorGUILayout.ObjectField(
                    $"Build {i + 1}", currentEntry.buildings[i], typeof(BuildData), false);

                if (GUILayout.Button("Удалить", GUILayout.Width(60)))
                {
                    currentEntry.buildings.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Добавить постройку"))
            {
                currentEntry.buildings.Add(null);
            }

            if (GUI.changed && holder != null)
            {
                EditorUtility.SetDirty(holder);
            }
        }
    }
}