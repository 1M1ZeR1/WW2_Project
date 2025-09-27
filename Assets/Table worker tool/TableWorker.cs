using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
public class TableWorker : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private TextAsset csvFile;
    private List<string[]> csvData = new List<string[]>();
    private Vector2 scrollPos;

    private List<string[]> modifiedData = new List<string[]>();


    [MenuItem("Window/UI Toolkit/TableWorker")]
    public static void ShowExample()
    {
        TableWorker wnd = GetWindow<TableWorker>("Table Worker");
        wnd.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Выбери CSV-файл:", EditorStyles.boldLabel);
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV-файл:", csvFile, typeof(TextAsset), false);

        if (csvFile != null && GUILayout.Button("Загрузить CSV"))
        {
            LoadCSV(csvFile.text);
        }

        GUILayout.Space(10);

        if (modifiedData.Count > 0)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for (int i = 0; i < modifiedData.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < modifiedData[i].Length; j++)
                {
                    modifiedData[i][j] = EditorGUILayout.TextField(modifiedData[i][j]);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Сохранить CSV"))
            {
                SaveCSV(modifiedData);
            }
        }
    }


    private void LoadCSV(string csvText)
    {
        csvData.Clear();
        modifiedData.Clear(); 

        string[] lines = csvText.Split('\n');
        foreach (string line in lines)
        {
            string[] row = line.Split(",,");
            csvData.Add(row);
            modifiedData.Add(row.Clone() as string[]);
        }
    }

    private void SaveCSV(List<string[]> csvData)
    {
        string filePath = Application.dataPath + "/Prefabs/Events/EventsTable/RandomEventTable.csv";

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < csvData.Count; i++)
            {
                writer.Write(string.Join(",,", csvData[i]));

                if (i < csvData.Count - 1) writer.Write("\n");
            }
        }
    }

}
#endif