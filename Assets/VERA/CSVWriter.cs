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
    public string API_KEY; // Add a public field to set the API key in the Inspector

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
        List<object> fakeData = new List<object>();

        foreach (var column in columnDefinition.columns)
        {
          if(column.name != "ts") {

            switch (column.type)
            {
                case CSVColumnDefinition.DataType.Number:
                    fakeData.Add(UnityEngine.Random.Range(0, 100));
                    break;
                case CSVColumnDefinition.DataType.String:
                    fakeData.Add("FakeString");
                    break;
                case CSVColumnDefinition.DataType.JSON:
                    fakeData.Add(new { key = "value" });
                    break;
            }
          }
        }

        CreateEntry(fakeData.ToArray());
    }

    public void SubmitCSV()
    {
        StartCoroutine(SubmitCSVCoroutine());
    }

private IEnumerator SubmitCSVCoroutine()
{
    string url = "https://sherlock.gaim.ucf.edu/api/logs/" + study_UUID + "/" + participant_UUID;

    byte[] fileData = null;
    bool fileReadSuccess = false;

    // Retry mechanism to handle sharing violation
    for (int i = 0; i < 3; i++)
    {
        try
        {
            fileData = File.ReadAllBytes(filePath);
            fileReadSuccess = true;
            break;
        }
        catch (IOException ex)
        {
            Debug.LogWarning($"Attempt {i + 1}: Failed to read file due to sharing violation. Retrying...");
        }

        if (!fileReadSuccess)
        {
            yield return new WaitForSeconds(0.1f); // Add a small delay before retrying
        }
    }

    if (!fileReadSuccess)
    {
        Debug.LogError("Failed to read the file after multiple attempts.");
        yield break;
    }

    WWWForm form = new WWWForm();
    form.AddField("study_UUID", study_UUID);
    form.AddField("participant_UUID", participant_UUID);
    form.AddBinaryData("file", fileData, study_UUID + "-" + participant_UUID + ".csv", "text/csv");

    UnityWebRequest www = UnityWebRequest.Post(url, form);
    www.SetRequestHeader("Authorization", "Bearer " + API_KEY);

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


private bool IsFileLocked(FileInfo file)
{
    FileStream stream = null;

    try
    {
        stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }
    catch (IOException)
    {
        // The file is locked by another process.
        return true;
    }
    finally
    {
        stream?.Close();
    }

    return false;
}


public void ClearFiles()
{
    string path = Application.persistentDataPath;
    Debug.Log("Clearing files in path: " + path);
    DirectoryInfo di = new DirectoryInfo(path);

    foreach (FileInfo file in di.GetFiles())
    {
        if (file.Extension == ".csv")
        {
            bool fileDeleted = false;

            for (int i = 0; i < 3; i++) // Retry mechanism
            {
                if (!IsFileLocked(file))
                {
                    try
                    {
                        file.Delete();
                        fileDeleted = true;
                        Debug.Log($"File {file.Name} deleted successfully.");
                        break;
                    }
                    catch (IOException ex)
                    {
                        Debug.LogWarning($"Attempt {i + 1}: Failed to delete file {file.Name}. Retrying...");
                        System.Threading.Thread.Sleep(100); // Add a small delay before retrying
                    }
                }
                else
                {
                    Debug.LogWarning($"Attempt {i + 1}: File {file.Name} is locked. Retrying...");
                    System.Threading.Thread.Sleep(100); // Add a small delay before retrying
                }
            }

            if (!fileDeleted)
            {
                Debug.LogError($"Failed to delete the file {file.Name} after multiple attempts.");
            }
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
        if (values.Length != columns.Count - 1)
        {
            Debug.LogError("Values count does not match columns count.");
            return;
        }

        List<string> entry = new List<string>();
        entry.Add(DateTime.UtcNow.ToString("o"));

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

#if UNITY_EDITOR
    public void CreateColumnDefinition()
    {
        if (columnDefinition == null)
        {
            columnDefinition = ScriptableObject.CreateInstance<CSVColumnDefinition>();
            string assetPath = $"Assets/VERA/{study_UUID}_ColumnDefinition.asset";
            UnityEditor.AssetDatabase.CreateAsset(columnDefinition, assetPath);

            // Add required columns
            columnDefinition.columns.Add(new CSVColumnDefinition.Column { name = "ts", type = CSVColumnDefinition.DataType.Date });
            columnDefinition.columns.Add(new CSVColumnDefinition.Column { name = "eventId", type = CSVColumnDefinition.DataType.Number });

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.EditorUtility.FocusProjectWindow();
            // UnityEditor.Selection.activeObject = columnDefinition;
        }
    }
#endif
}
