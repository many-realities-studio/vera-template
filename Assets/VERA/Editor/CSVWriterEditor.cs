using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CSVWriter))]
public class CSVWriterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CSVWriter csvWriter = (CSVWriter)target;

        if (GUILayout.Button("Save CSV"))
        {
            csvWriter.SaveCSV();
        }

        if (GUILayout.Button("Submit CSV"))
        {
            csvWriter.SubmitCSV();
        }
    }
}
