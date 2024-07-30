using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CSVWriter))]
public class CSVWriterEditor : Editor
{
    private SerializedProperty columnDefinitionProperty;

    private void OnEnable()
    {
        columnDefinitionProperty = serializedObject.FindProperty("columnDefinition");
    }

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
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Column Definitions", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            for (int i = 0; i < csvWriter.columnDefinition.columns.Count; i++)
            {
                var column = csvWriter.columnDefinition.columns[i];
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField($"Column {i + 1}", EditorStyles.boldLabel);

                column.name = EditorGUILayout.TextField("Name", column.name);
                column.type = (CSVColumnDefinition.DataType)EditorGUILayout.EnumPopup("Type", column.type);

                if (column.name != "ts" && column.name != "eventId")
                {
                    if (GUILayout.Button("Remove Column"))
                    {
                        csvWriter.columnDefinition.columns.RemoveAt(i);
                        i--; // Adjust index to account for removed element
                    }
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

            if (GUILayout.Button("Clear Files"))
            {
                csvWriter.ClearFiles();
            }

            serializedObject.ApplyModifiedProperties();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(csvWriter.columnDefinition);
        }
    }
}
