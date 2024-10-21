using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.Events;
using System.Linq;  // Add this line

public class VERALogger : MonoBehaviour
{
  public string API_KEY; // Add a public field to set the API key in the Inspector
  public string study_UUID;
  public string directoryPath = "";
  public string filePath = "";
  private List<VERAColumnDefinition.Column> columns = new List<VERAColumnDefinition.Column>();
  public string participant_UUID;
  private string host = "https://sreal.ucf.edu/vera-portal";
  // bool development = false;
  public bool simulateOffline = false;
  public bool collecting = true;

  // Enum for Choosing the progress of the experiment 
  public enum StudyParticipantProgressState { PRE_EXPERIMENT, IN_EXPERIMENT, PROCESSING, COMPLETE };

  public static VERALogger Instance;
  // [HideInInspector]
  public string fileName;
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
    // string host = development ? host_dev : host_live;
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
    // Test if we can reach the server
    StartCoroutine(TestConnection());

    if (Instance == null)
    {
      Instance = this;
    }
    DontDestroyOnLoad(gameObject);
    cache.Clear();
    participant_UUID = Guid.NewGuid().ToString().Replace("-", "");

    if (filePath == "")
    {
#if !UNITY_EDITOR
      directoryPath = Application.persistentDataPath;
#else
      if (!Directory.Exists("Assets/VERA/Data"))
      {
        Directory.CreateDirectory("Assets/VERA/Data");
      }
      directoryPath = Path.Combine(Application.dataPath, "VERA/Data"); 
#endif
    }
      filePath = Path.Combine(directoryPath, fileName + "-" + study_UUID + "-" + participant_UUID + ".csv");
      Debug.Log("CSV File: " + filePath);

    // Get the list of uploaded files from the json file "uploaded.json" on the persistent data path which contains an array
    // of file names that have been uploaded to the server into "loaded"
    if (File.Exists(Path.Combine(directoryPath, "uploaded.txt")))
    {
      Debug.Log("uploaded.txt exists");
      string[] loadedFiles = File.ReadAllLines(Path.Combine(directoryPath, "uploaded.txt"));
      // Loop through the files and upload all that aren't in the list
      foreach (string file in Directory.GetFiles(directoryPath, "*.csv"))
      {
        if (file != "" && !Array.Exists(loadedFiles, element => element == Path.GetFileName(file)))
        {
          Debug.Log("Path exists, submitting..." + file);
          // Check if file has more than 1 line
          if (File.ReadLines(file).Count() > 1)
          {
          StartCoroutine(SubmitCSVCoroutine(file));
          }
          else
          {
            // Delete the empty file
            File.Delete(file);
          }
        }
      }
    }
    else
    {
      Debug.Log("uploaded.txt does not exist, creating it");
      File.Create(Path.Combine(directoryPath, "uploaded.txt"));
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
        Debug.Log("Creating simulated entry for " + column.name + " type " + column.type);
        switch (column.type)
        {
          case VERAColumnDefinition.DataType.String:
            Debug.Log("Adding fakedata " + column.name);
            fakeData.Add("FakeString");
            break;
          case VERAColumnDefinition.DataType.Date:
            fakeData.Add(DateTime.UtcNow.ToString("o"));
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
  public IEnumerator ChangeProgress(StudyParticipantProgressState state)
  {

    // Create the Web request for the experiment progress
    // string host = development ? host_dev : host_live;

    // server expects enums in lower case
    string progressURL = host + "/api/" + study_UUID + "/" + participant_UUID + "/progress/" + state.ToString().ToLower();

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
        Debug.Log("Updated Progress to " + state.ToString());
      }
    }
  }

  public void ChangeParticipantProgressState(StudyParticipantProgressState state)
  {
    Debug.Log("Participant progress state was updated");
    StartCoroutine(ChangeProgress(state));
  }


  public void SubmitCSV()
  {
    SubmitCSV(filePath);
  }
  public void SubmitCSV(string file, bool flushOnSubmit = false)
  {
    Debug.Log(file);
    collecting = false;
    StartCoroutine(SubmitCSVWrapper(file, flushOnSubmit));
  }

  private IEnumerator SubmitCSVWrapper(string file, bool flushOnSubmit = false)
  {
    if (flushOnSubmit)
    {
      Flush();
    }

    onBeginFileUpload?.Invoke();
    yield return StartCoroutine(SubmitCSVCoroutine(file));
    onFileUploadExited?.Invoke();
  }

private IEnumerator SubmitCSVCoroutine(string file)
{
    // Extracting participant UDID from file name
    string file_participant_UDID;
    if (file.Length > 0 && file.Contains("-"))
    {
        Debug.Log(file);
        var basename = Path.GetFileName(file);
        file_participant_UDID = basename.Split('-')[2].Split('.')[0];
        Debug.Log(file_participant_UDID);
    }
    else
    {
        Debug.LogError("Invalid file name");
        yield break;
    }

    // Prepare the upload URL
    string url = host + "/api/logs/" + study_UUID + "/" + file_participant_UDID;
    Debug.Log("Submitting to URL: " + url);

    // Attempt to read the file with a retry mechanism for sharing violation
    byte[] fileData = null;
    bool fileReadSuccess = false;
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
            yield return new WaitForSeconds(0.1f); // Small delay before retrying
        }
    }

    if (!fileReadSuccess)
    {
        Debug.LogError("Failed to read the file after multiple attempts.");
        yield break;
    }

    // Prepare the form for submission
    WWWForm form = new WWWForm();
    form.AddField("study_UUID", study_UUID);
    form.AddField("participant_UUID", file_participant_UDID);
    form.AddBinaryData("file", fileData, study_UUID + "-" + file_participant_UDID + ".csv", "text/csv");

    // Create the UnityWebRequest and configure it
    uploadWebRequest = UnityWebRequest.Post(url, form);
    uploadWebRequest.SetRequestHeader("Authorization", "Bearer " + API_KEY);
    uploadWebRequest.timeout = 300; // Set the timeout to 300 seconds (5 minutes)

    if (!simulateOffline)
    {
        yield return uploadWebRequest.SendWebRequest(); // Wait for the request to complete
    }

    // Check the result
    if (!simulateOffline && uploadWebRequest.result == UnityWebRequest.Result.Success)
    {
        Debug.Log("Upload complete! Response: " + uploadWebRequest.downloadHandler.text);

        // Append to uploaded.txt
        var uploaded = File.ReadAllLines(Path.Combine(directoryPath, "uploaded.txt"));
        if (!Array.Exists(uploaded, element => element == Path.GetFileName(file)))
        {
            File.AppendAllText(Path.Combine(directoryPath, "uploaded.txt"), Path.GetFileName(file) + Environment.NewLine);
            Debug.Log("Updated uploaded.txt");
        }

        // Print the contents of uploaded.txt
        Debug.Log(File.ReadAllText(Path.Combine(directoryPath, "uploaded.txt")));

        // Trigger any on-file-uploaded actions
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
    if (values.Length != columnDefinition.columns.Count - 2)
    {
      Debug.LogError("Values count does not match columns count. values: " + values.Length + ", cols: " + columns.Count 
        + ", columnDefinition.columns.Count: " + columnDefinition.columns.Count);
      return;
    }

    List<string> entry = new List<string>();
    // Add timestamp first.
    entry.Add(DateTime.UtcNow.ToString("o"));
    entry.Add(Convert.ToString(eventId));
    if (columns.Count < 2) {
      VERAColumnDefinition.Column column = columnDefinition.columns[0];
      columns.Add(column);
      column = columnDefinition.columns[1];
      columns.Add(column);

    }
    for (int i = 0; i < values.Length; i++)
    {
      object value = values[i];
      // If column doesn't exist, add it:
      VERAColumnDefinition.Column column;
      if(columns.Count <= i + 2) {
        column = columnDefinition.columns[i+2];
        columns.Add(column);
      } else {
        column = columns[i+2];
      }
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
    }
  }

  private void Flush()
  {
    timeSinceLastFlush = 0f;

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
