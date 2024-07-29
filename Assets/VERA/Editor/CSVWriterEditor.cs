using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CSVWriter))]
public class CSVWriterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CSVWriter csvWriter = (CSVWriter)target;

        if (csvWriter.columnDefinition == null)
        {
            if (GUILayout.Button("Create Column Definition"))
            {
                csvWriter.CreateColumnDefinition();
            }
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Column Definitions", EditorStyles.boldLabel);

            foreach (var column in csvWriter.columnDefinition.columns)
            {
                EditorGUILayout.BeginHorizontal();
                column.name = EditorGUILayout.TextField("Name", column.name);
                column.type = (CSVColumnDefinition.DataType)EditorGUILayout.EnumPopup("Type", column.type);
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Column"))
            {
                csvWriter.columnDefinition.columns.Add(new CSVColumnDefinition.Column());
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Simulate Entry"))
            {
                csvWriter.SimulateEntry();
            }

            if (GUILayout.Button("Submit CSV"))
            {
                csvWriter.SubmitCSV();
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(csvWriter.columnDefinition);
        }
    }
}
