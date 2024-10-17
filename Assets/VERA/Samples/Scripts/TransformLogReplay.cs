using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class TransformLogReplay : MonoBehaviour
{

    // TransformLogReplay "replays" the transform data stored in a csv, matching an object's transform with the csv

    [Tooltip("Filters matched transform to only a single target event ID")]
    [SerializeField] private int targetEventId = 0;

    [Tooltip("The CSV file with desired transform data")]
    [SerializeField] private TextAsset csvFile;

    [Tooltip("Whether to match transform position from CSV")]
    [SerializeField] private bool matchPosition;

    [Tooltip("Whether to match transform rotation from CSV")]
    [SerializeField] private bool matchRotation;

    [Tooltip("Whether to match transform scale from CSV")]
    [SerializeField] private bool matchScale;

    private List<TransformEntry> entries = new List<TransformEntry>();

    // Start
    private void Start()
    {
        // Load and parse the CSV file
        LoadCSV();
        // Start the coroutine to apply transforms
        StartCoroutine(ApplyTransforms());
    }

    // Loads and parses the CSV file into transform datas
    void LoadCSV()
    {
        string[] lines = csvFile.text.Split('\n');

        if (lines.Length <= 1)
        {
            Debug.LogError("CSV file is empty or only contains headers.");
            return;
        }

        // Parse the first timestamp to get the start time
        string[] firstLineParts = lines[1].Split(new char[] { ',' }, 3);

        string firstTimestampString = CleanString(firstLineParts[0]);
        System.DateTime firstTimestamp = System.DateTime.Parse(firstTimestampString, null, DateTimeStyles.RoundtripKind);

        // Loop through each line starting from the second line (excluding headers)
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue; // Skip empty lines

            string[] parts = line.Split(new char[] { ',' }, 3);

            if (parts.Length < 3)
            {
                Debug.LogWarning("Invalid line (less than 3 columns): " + line);
                continue;
            }

            string tsString = CleanString(parts[0]);
            string eventIdString = CleanString(parts[1]);
            string transformString = CleanTransformString(parts[2]);

            int eventId;
            if (!int.TryParse(eventIdString, out eventId))
            {
                Debug.LogWarning("Invalid eventId in line: " + line);
                continue;
            }

            if (eventId != targetEventId)
                continue; // Skip entries with different event IDs

            System.DateTime timestamp = System.DateTime.Parse(tsString, null, DateTimeStyles.RoundtripKind);
            float timeOffset = (float)(timestamp - firstTimestamp).TotalSeconds;

            // Parse the transform JSON
            TransformData transformData = JsonUtility.FromJson<TransformData>(transformString);

            if (transformData == null)
            {
                Debug.LogWarning("Invalid transform data in line: " + line);
                continue;
            }

            // Create and add the transform entry
            TransformEntry entry = new TransformEntry
            {
                timeOffset = timeOffset,
                position = new Vector3(transformData.position.x, transformData.position.y, transformData.position.z),
                rotation = new Quaternion(transformData.rotation.x, transformData.rotation.y, transformData.rotation.z, transformData.rotation.w),
                localScale = new Vector3(transformData.localScale.x, transformData.localScale.y, transformData.localScale.z)
            };

            entries.Add(entry);
        }
    }

    // Coroutine which applies the transforms from CSV at desired times
    IEnumerator ApplyTransforms()
    {
        float startTime = Time.time;
        int currentIndex = 0;
        while (currentIndex < entries.Count)
        {
            TransformEntry entry = entries[currentIndex];
            float targetTime = startTime + entry.timeOffset;
            float waitTime = targetTime - Time.time;
            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
            }

            // Apply the transform to the object
            if (matchPosition)
                transform.position = entry.position;
            if (matchRotation)
                transform.rotation = entry.rotation;
            if (matchScale)
                transform.localScale = entry.localScale;

            currentIndex++;
        }
    }

    // Helper method to clean and unescape strings
    string CleanString(string input)
    {
        input = input.Trim();
        if (input.StartsWith("\"") && input.EndsWith("\""))
        {
            input = input.Substring(1, input.Length - 2);
        }
        return input;
    }

    // Helper method to clean and unescape the transform JSON string
    string CleanTransformString(string input)
    {
        input = input.Trim();
        if (input.StartsWith("\"") && input.EndsWith("\""))
        {
            input = input.Substring(1, input.Length - 2);
        }
        input = input.Replace("\"\"", "\""); // Unescape double quotes
        return input;
    }

    // Class to hold each transform entry
    public class TransformEntry
    {
        public float timeOffset; // Time since start, in seconds
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;
    }

    // Classes to represent the JSON data structure
    [System.Serializable]
    public class TransformData
    {
        public Vector3Data position;
        public QuaternionData rotation;
        public Vector3Data localScale;
    }

    [System.Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;
    }

    [System.Serializable]
    public class QuaternionData
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }

}
