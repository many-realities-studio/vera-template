using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CSVColumnDefinition))]
public class CSVColumnDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CSVColumnDefinition columnDefinition = (CSVColumnDefinition)target;

        if (GUILayout.Button("Add Column"))
        {
            columnDefinition.columns.Add(new CSVColumnDefinition.Column());
        }

        for (int i = 0; i < columnDefinition.columns.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            columnDefinition.columns[i].name = EditorGUILayout.TextField("Name", columnDefinition.columns[i].name);
            columnDefinition.columns[i].type = (CSVColumnDefinition.DataType)EditorGUILayout.EnumPopup("Type", columnDefinition.columns[i].type);

            if (GUILayout.Button("Remove"))
            {
                columnDefinition.columns.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(columnDefinition);
        }
    }
}
