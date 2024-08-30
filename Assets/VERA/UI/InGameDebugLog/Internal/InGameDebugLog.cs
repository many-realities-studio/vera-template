using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameDebugLog : MonoBehaviour
{

    // InGameDebugLog duplicates all debug info printed to the console, displaying
    //     it in an in-game text window

    [Header("Customization")]
    [SerializeField] private bool enableLogWindow = true;

    [Header("Components")]
    [SerializeField] private InGameDebugLine debugLinePrefab;
    [SerializeField] private Transform debugLineAreaParent;
    [SerializeField] private ScrollRect debugLineAreaScrollRect;
    [SerializeField] private TMP_Text extendedDetailsText;
    [SerializeField] private GameObject extendedDetailsParent;
    [SerializeField] private Button hideExtendedDetailsButton;

    private InGameDebugLine activeExtendedDebugLine;

    // Awake, registers callback for recieving debug messages
    private void Awake()
    {
        if (enableLogWindow)
        {
            HideExtendedWindow();
            
            ShowWindow();
            // Register the callback for logging messages
            Application.logMessageReceived += HandleNewLog;
            StartCoroutine(Tester());
        }
    }

    // OnDestroy, unregisters callback for recieving debug messages
    private void OnDestroy()
    {
        if (enableLogWindow)
        {
            // Unregister the callback
            Application.logMessageReceived -= HandleNewLog;
        }
    }

    // TEMPORARY, Delete
    private IEnumerator Tester()
    {
        int numMessages = 0;
        while (true)
        {
            numMessages++;
            Debug.Log("Message " + numMessages);
            yield return new WaitForSeconds(2f);
            Debug.LogWarning("Warning " + numMessages);
            yield return new WaitForSeconds(2f);
            Debug.LogError("Error " + numMessages);
            yield return new WaitForSeconds(2f);
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
        HideExtendedWindow();
        transform.GetChild(0).gameObject.SetActive(false);
    }

    // Handles an incoming debug log, outputting it on its own line
    private void HandleNewLog(string logString, string stackTrace, LogType type)
    {
        // Create new line, and pass it desired info
        InGameDebugLine newLine = Instantiate(debugLinePrefab, debugLineAreaParent);
        newLine.SetLineContent(this, logString, stackTrace, type);
        UpdateScrollToNewLog();
    }

    // Updates the scroll view to match new log amount
    private void UpdateScrollToNewLog()
    {
        if (debugLineAreaScrollRect.verticalNormalizedPosition < 0.01f)
        {
            StartCoroutine(WaitThenZeroScroll());
        }        
    }

    // Waits a frame for content to update, then zeroes out scroll
    private IEnumerator WaitThenZeroScroll()
    {
        yield return null;
        debugLineAreaScrollRect.verticalNormalizedPosition = 0f;
    }

    // Clears log content
    public void ClearLog()
    {
        HideExtendedWindow();
        // Delete all active log lines
        foreach (Transform t in debugLineAreaParent)
        {
            Destroy(t.gameObject);
        }
    }

    // Views extended details of a given debug line
    public void ViewExtendedDetails(InGameDebugLine lineToInspect)
    {
        if (activeExtendedDebugLine == null)
        {
            extendedDetailsParent.SetActive(true);
            hideExtendedDetailsButton.gameObject.SetActive(true);
        }
        else
        {
            activeExtendedDebugLine.StopViewExtendedDetails();
        }
        activeExtendedDebugLine = lineToInspect;

        extendedDetailsText.text = activeExtendedDebugLine.logString + "\n" + activeExtendedDebugLine.stackTrace;
    }

    // Hides the extended details window
    public void HideExtendedWindow()
    {
        if (activeExtendedDebugLine != null)
        {
            activeExtendedDebugLine.StopViewExtendedDetails();
            activeExtendedDebugLine = null;
        }
        hideExtendedDetailsButton.gameObject.SetActive(false);
        extendedDetailsParent.SetActive(false);
    }

    // Resumes logging, if it was previously paused
    public void ResumeLogging()
    {
        enableLogWindow = true;
    }

    // Pauses logging, if it was previously playing
    public void PauseLogging()
    {
        enableLogWindow = false;
    }
}
