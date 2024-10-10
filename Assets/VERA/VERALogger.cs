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
  public string filePath = "";
  private List<VERAColumnDefinition.Column> columns = new List<VERAColumnDefinition.Column>();
  public string participant_UUID;
  private string host_live = "https://sherlock.gaim.ucf.edu";
  private string host_dev = "http://localhost:4001";
  public bool development = false;
  public bool simulateOffline = false;
  public bool collecting = true;

  // Enum for Choosing the progress of the experiment 
  public enum Study_Participant_Progress { recruited, accepted, waitlisted, in_experiment, terminated, ghosted, complete };
  // https://learn.microsoft.com/en-us/dotnet/api/system.enum.tostring?view=net-8.0

  // Set instance of study progress to be uploaded to server.
  public Study_Participant_Progress progressParam = Study_Participant_Progress.recruited;


  public static VERALogger Instance;
  [HideInInspector]
  public VERAColumnDefinition columnDefinition;

  public List<string> cache = new List<string>();
  private const int cacheSizeLimit = 100; // Adjust the limit as needed
  private const float flushInterval = 5.0f; // Adjust the interval as needed
  private float timeSinceLastFlush = 0f;
  public UnityEvent onBeginFileUpload = new UnityEvent();
  public UnityEvent onFileUploaded = new UnityEvent();
  public UnityEvent onFileUploadExited = new UnityEvent();
  public UnityEvent onInitialized = new UnityEvent();
  public bool initialized;

  private UnityWebRequest uploadWebRequest;

  private UploadHandler uploadHandler;
  public float UploadProgress
  {
    get
    {
      if (uploadWebRequest == null || uploadWebRequest.isDone) return -1;
      return uploadWebRequest.uploadProgress;
    }
  }

  public int uploadFileSizeBytes
  {
    get
    {
      if (uploadWebRequest == null || uploadWebRequest.isDone) return -1;
      return uploadWebRequest.uploadHandler.data.Length;
    }
  }

  public IEnumerator TestConnection()
  {
    string host = development ? host_dev : host_live;
    string url = host + "/api/";
    UnityWebRequest www = UnityWebRequest.Get(url);
    www.SetRequestHeader("Authorization", "Bearer " + API_KEY);
    yield return www.SendWebRequest();
    if (www.result == UnityWebRequest.Result.Success)
    {
      Debug.Log("Connection successful");
    }
    else
    {
      Debug.LogError("Connection failed: " + www.error);
      simulateOffline = true;
    }
  }

  public void Awake()
  {
    // Testing to see if function to upload progress to web server works.
    OnButtonChangeProgress();

    // Test if we can reach the server
    StartCoroutine(TestConnection());
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

  // Function to change progress of the participant
  public IEnumerator ChangeProgress()
  {

    // Create the Web request for the experiment progress
    string host = development ? host_dev : host_live;
    //string progressURL = host + "/api/progress/" + study_UUID + "/" + participant_UUID + "/" + progressParam.ToString();

    string progressURL = host + "/api/progress/" + study_UUID + "/" + participant_UUID + "/" + progressParam.ToString();

    byte[] myData = null;

    using (UnityWebRequest www = UnityWebRequest.Put(progressURL, myData))
    {
      if (!simulateOffline)
      {
        www.SetRequestHeader("Authorization", "Bearer " + API_KEY);
        yield return www.SendWebRequest();
      }

      if (!simulateOffline && www.result != UnityWebRequest.Result.Success)
      {
        Debug.Log(www.error);
      }
      else
      {
        Debug.Log("Upload complete!");
      }
    }

  }

  public void OnButtonChangeProgress()
  {
    Debug.Log("Progress was updated");
    StartCoroutine(ChangeProgress());
  }


  public void SubmitCSV()
  {
    SubmitCSV(filePath);
  }
  public void SubmitCSV(string file)
  {
    Debug.Log(file);
    StartCoroutine(SubmitCSVWrapper(file));
  }

  private IEnumerator SubmitCSVWrapper(string file)
  {
    onBeginFileUpload?.Invoke();
    yield return StartCoroutine(SubmitCSVCoroutine(file));
    onFileUploadExited?.Invoke();
  }

  private IEnumerator SubmitCSVCoroutine(string file)
  {
    string host = development ? host_dev : host_live;
    string file_participant_UDID;
    if (file.Length > 0 && file.Contains("-"))
    {
      Debug.Log(file);
      var basename = Path.GetFileName(file);
      file_participant_UDID = basename.Split('-')[1].Split('.')[0];
      Debug.Log(file_participant_UDID);
    }
    else
    {
      Debug.LogError("Invalid file name");
      yield break;
    }
    string url = host + "/api/logs/" + study_UUID + "/" + file_participant_UDID;
    Debug.Log("Submitting to url" + url);
    byte[] fileData = null;
    bool fileReadSuccess = false;

    // Retry mechanism to handle sharing violation
    for (int i = 0; i < 3; i++)
    {
      try
      {
        Debug.Log("Reading file: " + file);
        fileData = File.ReadAllBytes(file);
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
    form.AddBinaryData("file", fileData, study_UUID + "-" + file_participant_UDID + ".csv", "text/csv");

    uploadWebRequest = UnityWebRequest.Post(url, form);
    uploadWebRequest.SetRequestHeader("Authorization", "Bearer " + API_KEY);
    if (!simulateOffline)
    {
      yield return uploadWebRequest.SendWebRequest();
    }
    if (!simulateOffline && uploadWebRequest.result == UnityWebRequest.Result.Success)
    {
      Debug.Log("Upload complete! Response: " + uploadWebRequest.downloadHandler.text);
      // Append the uploaded file name to the "uploaded.txt" file as a new line
      var uploaded = File.ReadAllLines(Path.Combine(Application.persistentDataPath, "uploaded.txt"));
      if (!Array.Exists(uploaded, element => element == Path.GetFileName(file)))
      {

        File.AppendAllText(Path.Combine(Application.persistentDataPath, "uploaded.txt"), Path.GetFileName(file) + Environment.NewLine);
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
      Debug.LogError("Upload failed: " + uploadWebRequest.error);
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
    if (filePath == "")
    {
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

      writer.WriteLine(string.Join(",", columnNames));
      writer.Flush();
    }
    Debug.Log("CSV File created and saved at " + filePath);
    initialized = true;
    onInitialized?.Invoke();
  }

  public void CreateEntry(int eventId, params object[] values)
  {
    if (!collecting)
    {
      return;
    }
    if (values.Length != columns.Count - 2)
    {
      Debug.LogError("Values count does not match columns count. values" + values.Length + " cols " + columns.Count);
      return;
    }

    List<string> entry = new List<string>();
    // Add timestamp first.
    entry.Add(DateTime.UtcNow.ToString("o"));
    entry.Add(Convert.ToString(eventId));

    for (int i = 0; i < values.Length; i++)
    {
      object value = values[i];
      VERAColumnDefinition.Column column = columns[i + 2];

      string formattedValue = "";
      switch (column.type)
      {
        case VERAColumnDefinition.DataType.Number:
          formattedValue = Convert.ToString(value);
          break;
        case VERAColumnDefinition.DataType.String:
          formattedValue = EscapeForCsv(value.ToString());
          break;
        case VERAColumnDefinition.DataType.JSON:
          var json = JsonConvert.SerializeObject(value);
          formattedValue = EscapeForCsv(json);
          break;
        case VERAColumnDefinition.DataType.Transform:
          var transform = value as Transform;
          if (transform != null)
          {
            var settings = new JsonSerializerSettings
            {
              ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            formattedValue = EscapeForCsv(JsonConvert.SerializeObject(new
            {
              position = new { x = transform.position.x, y = transform.position.y, z = transform.position.z },
              rotation = new { x = transform.rotation.x, y = transform.rotation.y, z = transform.rotation.z, w = transform.rotation.w },
              localScale = new { x = transform.localScale.x, y = transform.localScale.y, z = transform.localScale.z }
            }, settings));
          }
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
    // If it contains a quote, wrap in quotes
    if (value.Contains(",") || value.Contains("\n") || value.Contains("\""))
    {
      return $"\"{value}\"";
    }
    else
    {
      return value;
    }
  }

#if UNITY_EDITOR
  public void CreateColumnDefinition(bool forceCreate = true)
  {
    if (forceCreate || columnDefinition == null)
    {
      columnDefinition = ScriptableObject.CreateInstance<VERAColumnDefinition>();
      // Create the Columns folder if it doesn't exist
      if (!Directory.Exists("Assets/VERA/Columns"))
      {
        Directory.CreateDirectory("Assets/VERA/Columns");
      }
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
