using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
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
    [SerializeField] private TMP_Text normalLogNumText;
    [SerializeField] private TMP_Text warningLogNumText;
    [SerializeField] private TMP_Text errorLogNumText;

    private List<InGameDebugLine> displayedLogLines = new List<InGameDebugLine>();
    private InGameDebugLine activeExtendedDebugLine;
    private bool newLogHasNotBeenSeen = false;

    private int numNormalLogs = 0;
    private int numWarningLogs = 0;
    private int numErrorLogs = 0;
    private bool showNormalLogs = true;
    private bool showWarningLogs = true;
    private bool showErrorLogs = true;

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
        bool isLineDisplayed = ResetLogDisplay(newLine);

        if (displayedLogLines.Count <= 5)
            StartCoroutine(WaitThenZeroScroll());
        else
            UpdateScrollToNewLog(isLineDisplayed);

        if (type == LogType.Log)
            numNormalLogs++;
        else if (type == LogType.Warning)
            numWarningLogs++;
        else if (type == LogType.Error)
            numErrorLogs++;

        UpdateLogTypeNums();
    }

    // Updates how many logs of each type are present (displaying the numbers to the UI)
    private void UpdateLogTypeNums()
    {
        normalLogNumText.text = numNormalLogs.ToString();
        warningLogNumText.text = numWarningLogs.ToString();
        errorLogNumText.text = numErrorLogs.ToString();
    }

    // Updates the scroll view to match new log amount
    private void UpdateScrollToNewLog(bool checkForUnseenLog)
    {
        // Check if we need to scroll down
        if (debugLineAreaScrollRect.verticalNormalizedPosition < 0.001f)
        {
            StartCoroutine(WaitThenZeroScroll());
        }
        // Else, check if we need to notify user of new unread log
        else if (checkForUnseenLog && !newLogHasNotBeenSeen)
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

        numNormalLogs = 0;
        numWarningLogs = 0;
        numErrorLogs = 0;
        UpdateLogTypeNums();

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

    // Toggles whether "normal" logs should be seen in the logger
    public void ToggleViewNormalLogs()
    {
        showNormalLogs = !showNormalLogs;
        ResetLogDisplayAll();
    }

    // Toggles whether warnings should be seen in the logger
    public void ToggleViewWarnings()
    {
        showWarningLogs = !showWarningLogs;
        ResetLogDisplayAll();
    }

    // Toggles whether errors should be seen in the logger
    public void ToggleViewErrors()
    {
        showErrorLogs = !showErrorLogs;
        ResetLogDisplayAll();
    }

    // Resets log display to only show pertinent logs, based on what types of logs should be shown
    private void ResetLogDisplayAll()
    {
        foreach (InGameDebugLine line in displayedLogLines)
        {
            ResetLogDisplay(line);
        }
    }

    // Resets log display of a single given log line
    // Returns whether the log line is displayed or not, based on current parameters
    private bool ResetLogDisplay(InGameDebugLine line)
    {
        if (line.logType == LogType.Log)
        {
            if (showNormalLogs)
            {
                line.gameObject.SetActive(true);
            }
            else
            {
                line.gameObject.SetActive(false);
            }
        }
        else if (line.logType == LogType.Warning)
        {
            if (showWarningLogs)
            {
                line.gameObject.SetActive(true);
            }
            else
            {
                line.gameObject.SetActive(false);
            }
        }
        else if (line.logType == LogType.Error)
        {
            if (showErrorLogs)
            {
                line.gameObject.SetActive(true);
            }
            else
            {
                line.gameObject.SetActive(false);
            }
        }

        return line.gameObject.activeSelf;
    }
}
