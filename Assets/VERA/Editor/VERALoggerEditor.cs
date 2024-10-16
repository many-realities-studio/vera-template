#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VERALogger))]
public class VERALoggerEditor : Editor
{
  private SerializedProperty columnDefinitionProperty;

  private void OnEnable()
  {
    columnDefinitionProperty = serializedObject.FindProperty("columnDefinition");
  }

  public override void OnInspectorGUI()
  {
    // Display help text
        EditorGUILayout.HelpBox(
            "Input your API key and study UUID below.\n",
            MessageType.Info);

        // Draw default inspector
        DrawDefaultInspector();
        // DrawDefaultInspector();

    VERALogger csvWriter = (VERALogger)target;

    if (csvWriter.columnDefinition == null)
    {
      if (GUILayout.Button("Create Column Definition"))
      {
        csvWriter.CreateColumnDefinition(true);
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

        if (column.name == "ts" || column.name == "eventId")
        {
          EditorGUILayout.LabelField($"Required: {column.name}", EditorStyles.boldLabel);
          EditorGUILayout.LabelField("Type", column.type.ToString());
        }
        else
        {
          EditorGUILayout.LabelField($"Column {i + 1}: {column.name}", EditorStyles.boldLabel);
          column.name = EditorGUILayout.TextField("Name ", column.name);
          column.type = (VERAColumnDefinition.DataType)EditorGUILayout.EnumPopup("Type", column.type);

          EditorGUILayout.BeginHorizontal();

          // Add up arrow button
          if (i > 2 && GUILayout.Button("Up", GUILayout.Width(50))) // Ensure it cannot move above index 2
          {
            var temp = csvWriter.columnDefinition.columns[i];
            csvWriter.columnDefinition.columns[i] = csvWriter.columnDefinition.columns[i - 1];
            csvWriter.columnDefinition.columns[i - 1] = temp;
          }

          // Add down arrow button
          if (i < csvWriter.columnDefinition.columns.Count - 1 && GUILayout.Button("Down", GUILayout.Width(50)))
          {
            var temp = csvWriter.columnDefinition.columns[i];
            csvWriter.columnDefinition.columns[i] = csvWriter.columnDefinition.columns[i + 1];
            csvWriter.columnDefinition.columns[i + 1] = temp;
          }

          EditorGUILayout.EndHorizontal();

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
        csvWriter.columnDefinition.columns.Add(new VERAColumnDefinition.Column());
      }
      if (GUILayout.Button("Create New Column Definitions"))
      {
        csvWriter.CreateColumnDefinition(true);
      }

      EditorGUILayout.Space();

      if (GUILayout.Button("Simulate Entry"))
      {
        csvWriter.SimulateEntry();
      }

      if (GUILayout.Button("Submit CSV"))
      {
        csvWriter.SubmitCSV(csvWriter.filePath, true);
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
#endif