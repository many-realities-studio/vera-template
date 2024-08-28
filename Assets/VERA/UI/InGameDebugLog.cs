using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameDebugLog : MonoBehaviour
{

    // InGameDebugLog duplicates all debug info printed to the console, displaying
    //     it in an in-game text window

    [SerializeField] private InGameDebugLine debugLinePrefab;
    [SerializeField] private Transform debugLineAreaParent;

    // Awake, registers callback for recieving debug messages
    void Awake()
    {
        // Register the callback for logging messages
        Application.logMessageReceived += HandleNewLog;
    }

    // OnDestroy, unregisters callback for recieving debug messages
    void OnDestroy()
    {
        // Unregister the callback
        Application.logMessageReceived -= HandleNewLog;
    }

    // Handles an incoming debug log, outputting it on its own line
    void HandleNewLog(string logString, string stackTrace, LogType type)
    {
        // Create new line, and pass it desired info
        InGameDebugLine newLine = Instantiate(debugLinePrefab, debugLineAreaParent);
        newLine.SetLineContent(logString, stackTrace, type);
    }

    // Clears log content
    public void ClearLog()
    {
        // Delete all active log lines
        foreach (Transform t in debugLineAreaParent)
        {
            Destroy(t.gameObject);
        }
    }

    // Shows the debug log window
    public void ShowWindow()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }

    // Hides the debug log window
    public void HideWindow()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
