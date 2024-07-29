using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVWriter : MonoBehaviour
{
    private static string filePath;
    private static List<Column> columns = new List<Column>();
    public string study_UUID;
    [Serializable]
    public class Column
    {
        public string name;
        public DataType type;

        public Column(string name, DataType type)
        {
            this.name = name;
            this.type = type;
        }
    }

    public enum DataType
    {
        Number,
        String,
        JSON
    }

    // Saves the CSV file to the applciation directory.
    public void SaveCSV()
    {
        string path = Path.Combine(Application.persistentDataPath, study_UUID + ".csv");
        File.Move(filePath, path);
    }

    // Sends the CSV to the server via a post request
    public void SubmitCSV()
    {
        string path = Path.Combine(Application.persistentDataPath, study_UUID + ".csv");
        string url = "http://sherlock.gaim.ucf.edu/api/"+study_UUID+"testudid";
        WWWForm form = new WWWForm();
        form.AddField("study_UUID", study_UUID);
        form.AddBinaryData("file", File.ReadAllBytes(path), study_UUID + ".csv", "text/csv");
        WWW www = new WWW(url, form);
        StartCoroutine(WaitForRequest(www));
    }

    // Add an interface button to trigger submitting csv file
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 100), "Submit CSV"))
        {
            SubmitCSV();
        }
    }

    // Initialize the CSV file with columns
    public static void Initialize(string fileName, ArrayList columnDefinitions)
    {
        filePath = Path.Combine(Application.persistentDataPath, fileName);
        
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            columns.Clear();
            List<string> columnNames = new List<string>();

            foreach (object obj in columnDefinitions)
            {
                Column column = (Column)obj;
                columns.Add(column);
                columnNames.Add(column.name);
            }

            writer.WriteLine(string.Join(",", columnNames));
        }
    }

    // Create a new entry in the CSV file
    public static void CreateEntry(params object[] values)
    {
        if (values.Length != columns.Count)
        {
            Debug.LogError("Values count does not match columns count.");
            return;
        }

        List<string> entry = new List<string>();

        for (int i = 0; i < values.Length; i++)
        {
            object value = values[i];
            Column column = columns[i];

            switch (column.type)
            {
                case DataType.Number:
                    entry.Add(Convert.ToString(value));
                    break;
                case DataType.String:
                    entry.Add($"\"{value.ToString()}\"");
                    break;
                case DataType.JSON:
                    entry.Add($"\"{JsonUtility.ToJson(value)}\"");
                    break;
                default:
                    entry.Add(value.ToString());
                    break;
            }
        }

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine(string.Join(",", entry));
        }
    }
}
