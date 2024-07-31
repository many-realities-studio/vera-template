using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine.Events;

public class VERALogger : MonoBehaviour
{
  public string API_KEY; // Add a public field to set the API key in the Inspector
  public string study_UUID;
  public string filePath="";
  private List<VERAColumnDefinition.Column> columns = new List<VERAColumnDefinition.Column>();
  public string participant_UUID;
  private string host_live = "https://sherlock.gaim.ucf.edu";
  private string host_dev = "http://localhost:4001";
  public bool development = false;
  public bool simulateOffline = false;
  public bool collecting = true;

  public static VERALogger Instance;
  [HideInInspector]
  public VERAColumnDefinition columnDefinition;

  public List<string> cache = new List<string>();
  private const int cacheSizeLimit = 100; // Adjust the limit as needed
  private const float flushInterval = 5.0f; // Adjust the interval as needed
  private float timeSinceLastFlush = 0f;
  public UnityEvent onFileUploaded = new UnityEvent();
  public UnityEvent onInitialized = new UnityEvent();
  public bool initialized;


  public void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    DontDestroyOnLoad(gameObject);
    // Get the list of uploaded files from the json file "uploaded.json" on the persistent data path which contains an array
    // of file names that have been uploaded to the server into "loaded"
    if (File.Exists(Path.Combine(Application.persistentDataPath, "uploaded.txt")))
    {
      Debug.Log("uploaded.txt exists");
      string[] loadedFiles = File.ReadAllLines(Path.Combine(Application.persistentDataPath, "uploaded.txt"));
      // Loop through the files and upload all that aren't in the list
      foreach (string file in Directory.GetFiles(Application.persistentDataPath, "*.csv"))
      {
        if (file != "" && !Array.Exists(loadedFiles, element => element == Path.GetFileName(file)))
        {
          Debug.Log("Path exists, submitting..." + file);
          StartCoroutine(SubmitCSVCoroutine(file));
        }
      }
    }
    else
    {
      Debug.Log("uploaded.txt does not exist, creating it");
      File.Create(Path.Combine(Application.persistentDataPath, "uploaded.txt"));
    }
    Initialize();
    // Do not destroy on scene change
  }

  public void SimulateEntry()
  {
    List<object> fakeData = new List<object>();

    foreach (var column in columnDefinition.columns)
    {
      if (column.name != "ts" && column.name != "eventId")
      {

        switch (column.type)
        {
          case VERAColumnDefinition.DataType.String:
            fakeData.Add("FakeString");
            break;
          case VERAColumnDefinition.DataType.Number:
            fakeData.Add(UnityEngine.Random.Range(0, 100));
            break;
          case VERAColumnDefinition.DataType.JSON:
            fakeData.Add(new { key = "value" });
            break;
          case VERAColumnDefinition.DataType.Transform:
            fakeData.Add(new { position = new { x = 1, y = 2, z = 3 }, rotation = new { x = 0, y = 0, z = 0, w = 1 } });
            break;
        }
      }
    }

    CreateEntry(UnityEngine.Random.Range(0, 100), fakeData.ToArray());
  }

  public void SubmitCSV()
  {
    Debug.Log(filePath);
    StartCoroutine(SubmitCSVCoroutine(filePath));
  }
  public void SubmitCSV(string file)
  {
    Debug.Log(file);
    StartCoroutine(SubmitCSVCoroutine(file));
  }

  private IEnumerator SubmitCSVCoroutine(string file)
  {
    string host = development ? host_dev : host_live;
    if(file.Length>0 && file.Contains("-")) {

    var participant_UDID = file.Split('-')[1].Split('.')[0];
    } else {
      Debug.LogError("Invalid file name");
      yield break;
    }
    string url = host + "/api/logs/" + study_UUID + "/" + participant_UUID;

    byte[] fileData = null;
    bool fileReadSuccess = false;

    // Retry mechanism to handle sharing violation
    for (int i = 0; i < 3; i++)
    {
      try
      {
        Debug.Log("Reading file: " + filePath);
        fileData = File.ReadAllBytes(filePath);
        fileReadSuccess = true;
        break;
      }
      catch (IOException ex)
      {
        Debug.LogWarning($"Attempt {ex.Message} {i + 1}: Failed to read file due to sharing violation. Retrying...");
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
    if (!simulateOffline)
    {
      yield return www.SendWebRequest();
    }
    else
    {
      yield return new WaitForSeconds(1);
    }
    if (!simulateOffline && www.result == UnityWebRequest.Result.Success)
    {
      Debug.Log("Upload complete! Response: " + www.downloadHandler.text);
      // Append the uploaded file name to the "uploaded.txt" file as a new line
      var uploaded = File.ReadAllLines(Path.Combine(Application.persistentDataPath, "uploaded.txt"));
      if (!Array.Exists(uploaded, element => element == Path.GetFileName(filePath)))
      {

        File.AppendAllText(Path.Combine(Application.persistentDataPath, "uploaded.txt"), Path.GetFileName(filePath) + Environment.NewLine);
        Debug.Log("Updated uploaded.txt");
      }
      // Print out uploaded.txt
      Debug.Log(File.ReadAllText(Path.Combine(Application.persistentDataPath, "uploaded.txt")));
      onFileUploaded?.Invoke();

    }
    else if (simulateOffline)
    {
      Debug.Log("Simulated offline mode, not uploading file");
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
              Debug.LogWarning($"Attempt {ex.Message} {i + 1}: Failed to delete file {file.Name}. Retrying...");
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
    participant_UUID = Guid.NewGuid().ToString().Replace("-", "");
    if(filePath == "") {
      filePath = Path.Combine(Application.persistentDataPath, study_UUID + "-" + participant_UUID + ".csv");
    }

    using (StreamWriter writer = new StreamWriter(filePath))
    {
      columns.Clear();
      List<string> columnNames = new List<string>();

      foreach (var column in columnDefinition.columns)
      {
        columns.Add(column);
        columnNames.Add(column.name);
      }

      Debug.Log(columnNames);
      writer.WriteLine(string.Join(",", columnNames));
      writer.Flush();
    }
    Debug.Log("CSV File created and saved at " + filePath);
    initialized = true;
    onInitialized?.Invoke();
  }

  public void CreateEntry(int eventId, params object[] values)
  {
    if(!collecting) {
      return;
    }
    if (values.Length != columns.Count - 2)
    {
      Debug.LogError("Values count does not match columns count. values" + values.Length + " cols " + columns.Count);
      return;
    }

    List<string> entry = new List<string>();
    // Add timestamp first.
    entry.Add(EscapeForCsv(DateTime.UtcNow.ToString("o")));
    entry.Add(Convert.ToString(eventId));

    for (int i = 0; i < values.Length; i++)
    {
      object value = values[i];
      VERAColumnDefinition.Column column = columns[i + 2];

      string formattedValue = "";
      Debug.Log("Column type: " + column.type);
      switch (column.type)
      {
        case VERAColumnDefinition.DataType.Number:
          formattedValue = Convert.ToString(value);
          break;
        case VERAColumnDefinition.DataType.String:
          formattedValue = EscapeForCsv(value.ToString());
          break;
        case VERAColumnDefinition.DataType.JSON:
        case VERAColumnDefinition.DataType.Transform:
          Debug.Log("Formatting JSON or Transform");
          var json = JsonConvert.SerializeObject(value);
          Debug.Log(value);
          Debug.Log(json);
          formattedValue = EscapeForCsv(JsonConvert.SerializeObject(value));
          break;
        default:
          formattedValue = EscapeForCsv(value.ToString());
          break;
      }

      entry.Add(formattedValue);
    }

    cache.Add(string.Join(",", entry));
    FlushIfNeeded();
  }

  private void FlushIfNeeded()
  {
    if (cache.Count >= cacheSizeLimit || timeSinceLastFlush >= flushInterval)
    {
      Flush();
      timeSinceLastFlush = 0f;
    }
  }

  private void Flush()
  {
    if (cache.Count == 0)
    {
      return;
    }

    using (StreamWriter writer = new StreamWriter(filePath, true))
    {
      foreach (var entry in cache)
      {
        writer.WriteLine(entry);
      }
    }
    cache.Clear();
  }

  private string EscapeForCsv(string value)
  {
    Debug.Log(value);
    if (string.IsNullOrEmpty(value))
    {
      return "\"\"";
    }

    value = value.Replace("\"", "\"\"");
    return $"\"{value}\"";
  }

#if UNITY_EDITOR
  public void CreateColumnDefinition()
  {
    if (columnDefinition == null)
    {
      columnDefinition = ScriptableObject.CreateInstance<VERAColumnDefinition>();
      string assetPath = $"Assets/VERA/Columns/{study_UUID}_ColumnDefinition.asset";
      UnityEditor.AssetDatabase.CreateAsset(columnDefinition, assetPath);

      // Add required columns
      columnDefinition.columns.Add(new VERAColumnDefinition.Column { name = "ts", type = VERAColumnDefinition.DataType.Date });
      columnDefinition.columns.Add(new VERAColumnDefinition.Column { name = "eventId", type = VERAColumnDefinition.DataType.Number });

      UnityEditor.AssetDatabase.SaveAssets();
      UnityEditor.AssetDatabase.Refresh();
      UnityEditor.EditorUtility.FocusProjectWindow();
      // UnityEditor.Selection.activeObject = columnDefinition;
    }
  }
#endif

  private void Update()
  {
    timeSinceLastFlush += Time.deltaTime;
    FlushIfNeeded();
  }

  private void OnDestroy()
  {
    Flush();
  }
}
