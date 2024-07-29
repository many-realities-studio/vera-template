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

            EditorGUI.indentLevel++;

            for (int i = 0; i < csvWriter.columnDefinition.columns.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                csvWriter.columnDefinition.columns[i].name = EditorGUILayout.TextField("Name", csvWriter.columnDefinition.columns[i].name);
                csvWriter.columnDefinition.columns[i].type = (CSVColumnDefinition.DataType)EditorGUILayout.EnumPopup("Type", csvWriter.columnDefinition.columns[i].type);

                if (GUILayout.Button("Remove Column"))
                {
                    csvWriter.columnDefinition.columns.RemoveAt(i);
                    i--; // Adjust index to account for removed element
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUI.indentLevel--;

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
            if (GUILayout.Button("Clear other files"))
            {
                csvWriter.ClearFiles();
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(csvWriter.columnDefinition);
        }
    }
}
