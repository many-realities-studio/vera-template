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
    [SerializeField] private GameObject newLogNotification;

    private List<InGameDebugLine> displayedLogLines = new List<InGameDebugLine>();
    private InGameDebugLine activeExtendedDebugLine;
    private bool newLogHasNotBeenSeen = false;

    // Awake, registers callback for recieving debug messages
    private void Awake()
    {
        if (enableLogWindow)
        {
            HideExtendedWindow();
            newLogNotification.SetActive(false);

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

    // Update, checks if scroll view has reached the bottom, 
    private void Update()
    {
        // Check for "reading" new log
        if (newLogHasNotBeenSeen && debugLineAreaScrollRect.verticalNormalizedPosition < .001f)
        {
            newLogHasNotBeenSeen = false;
            newLogNotification.SetActive(false);
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
        displayedLogLines.Add(newLine);

        if (displayedLogLines.Count <= 5)
            StartCoroutine(WaitThenZeroScroll());
        else
            UpdateScrollToNewLog();
    }

    // Updates the scroll view to match new log amount
    private void UpdateScrollToNewLog()
    {
        // Check if we need to scroll down
        if (debugLineAreaScrollRect.verticalNormalizedPosition < 0.001f)
        {
            StartCoroutine(WaitThenZeroScroll());
        }
        // Else, check if we need to notify user of new unread log
        else if (!newLogHasNotBeenSeen)
        {
            newLogHasNotBeenSeen = true;
            newLogNotification.SetActive(true);
        }
    }

    // Waits a frame for content to update, then zeroes out scroll
    private IEnumerator WaitThenZeroScroll()
    {
        yield return null;
        JumpToNewestLog();
    }

    // Jumps to newest log (resets scroll view to 0)
    public void JumpToNewestLog()
    {
        debugLineAreaScrollRect.verticalNormalizedPosition = 0f;
    }

    // Clears log content
    public void ClearLog()
    {
        HideExtendedWindow();

        // Hide new log notification
        if (newLogHasNotBeenSeen)
        {
            newLogHasNotBeenSeen = false;
            newLogNotification.SetActive(false);
        }

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
