using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("This is a log.");
        Debug.LogWarning("This is a warning.");
        Debug.LogError("This is an error.");
    }

    void Awake()
    {
        Application.logMessageReceived += HandleNewLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleNewLog;
    }

    void HandleNewLog(string logString, string stackTrace, LogType type)
    {
        Debug.Log("Received log of type: " + type);
    }
}
