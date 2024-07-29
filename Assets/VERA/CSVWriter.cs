using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class CSVWriter : MonoBehaviour
{
    public string filePath;
    private List<Column> columns = new List<Column>();
    public string study_UUID;
    public string participant_UUID;
    public static CSVWriter Instance;

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
    public void Awake() {
      // Add three simple columns: timestamp, event, and data
      if(Instance == null) {
        Instance = this;
      }
      var columnDefinitions = new List<Column> {
        new Column("timestamp", DataType.Number),
        new Column("event", DataType.String),
        new Column("data", DataType.JSON)
      };
      Initialize(columns);
    }

    public void SimulateEntry() {
      CreateEntry(1, "Test", new { test = "test" });
    }
    // Sends the CSV to the server via a post request
    public void SubmitCSV()
    {
        StartCoroutine(SubmitCSVCoroutine());
    }

    private IEnumerator SubmitCSVCoroutine()
    {;
        string url = "http://sherlock.gaim.ucf.edu/api/" + study_UUID + "/" + participant_UUID;
        
        byte[] fileData = File.ReadAllBytes(filePath);
        
        WWWForm form = new WWWForm();
        form.AddField("study_UUID", study_UUID);
        form.AddField("participant_UUID", participant_UUID);
        form.AddBinaryData("file", fileData, study_UUID+"-"+participant_UUID + ".csv", "text/csv");

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Upload complete! Response: " + www.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Upload failed: " + www.error);
        }
    }

    public void ClearFiles() {
      // Delete all csv files in the directory
      DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath);
      foreach (FileInfo file in di.GetFiles())
      {
          if (file.Extension == ".csv")
          {
              file.Delete();
          }
      }
    }

    // Initialize the CSV file with columns
    public void Initialize(List<Column> columnDefinitions)
    {
        participant_UUID = Guid.NewGuid().ToString();
        filePath = Path.Combine(Application.persistentDataPath, study_UUID+"-"+participant_UUID + ".csv");

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
            Debug.Log(columnNames);
            writer.WriteLine(string.Join(",", columnNames));
            writer.Close();
        }
        Debug.Log("CSV File created and saved at " + filePath);
    }

    // Create a new entry in the CSV file
    public void CreateEntry(params object[] values)
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
