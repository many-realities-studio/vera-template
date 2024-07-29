using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CSVWriter))]
public class CSVWriterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CSVWriter csvWriter = (CSVWriter)target;

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
    }
}
