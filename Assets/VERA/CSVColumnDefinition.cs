using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class CSVWriter : MonoBehaviour
{
    public string filePath;
    private List<CSVColumnDefinition.Column> columns = new List<CSVColumnDefinition.Column>();
    public string study_UUID;
    public string participant_UUID;
    public static CSVWriter Instance;
    public CSVColumnDefinition columnDefinition;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        Initialize();
    }

    public void SimulateEntry()
    {
        CreateEntry(1, "Test", new { test = "test" });
    }

    public void SubmitCSV()
    {
        StartCoroutine(SubmitCSVCoroutine());
    }

    private IEnumerator SubmitCSVCoroutine()
    {
        string url = "http://sherlock.gaim.ucf.edu/api/" + study_UUID + "/" + participant_UUID;
        byte[] fileData = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddField("study_UUID", study_UUID);
        form.AddField("participant_UUID", participant_UUID);
        form.AddBinaryData("file", fileData, study_UUID + "-" + participant_UUID + ".csv", "text/csv");

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

    public void ClearFiles()
    {
        DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath);
        foreach (FileInfo file in di.GetFiles())
        {
            if (file.Extension == ".csv")
            {
                file.Delete();
            }
        }
    }

    public void Initialize()
    {
        participant_UUID = Guid.NewGuid().ToString();
        filePath = Path.Combine(Application.persistentDataPath, study_UUID + "-" + participant_UUID + ".csv");

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            columns.Clear();
            List<string> columnNames = new List<string>();

            foreach (var column in columnDefinition.columns)
            {
                Debug.Log("Adding in column " + column.name);
                columns.Add(column);
                columnNames.Add(column.name);
            }

            Debug.Log(columnNames);
            writer.WriteLine(string.Join(",", columnNames));
            writer.Flush();
        }
        Debug.Log("CSV File created and saved at " + filePath);
    }

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
            CSVColumnDefinition.Column column = columns[i];

            switch (column.type)
            {
                case CSVColumnDefinition.DataType.Number:
                    entry.Add(Convert.ToString(value));
                    break;
                case CSVColumnDefinition.DataType.String:
                    entry.Add($"\"{value.ToString()}\"");
                    break;
                case CSVColumnDefinition.DataType.JSON:
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
