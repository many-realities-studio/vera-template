using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InGameDebugLine : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    // InGameDebugLine handles a single line of text which is logged in the in-game 
    //     debug logger (e.g., InGameDebugLog.cs)

    [Header("Components")]
    [SerializeField] private RectTransform lineParentTransform;
    [SerializeField] private TMP_Text logTextLine1;
    [SerializeField] private TMP_Text logTextLine2;
    [SerializeField] private Image logSymbolImg;
    [SerializeField] private Image backgroundImg;

    [Header("Customization")]
    [SerializeField] private Color baseBackgroundColor;
    [SerializeField] private Color hoveredBackgroundColor;
    [SerializeField] private Color selectedBackgroundColor;

    [SerializeField] private Color normalLogColor;
    [SerializeField] private Sprite normalLogSprite;
    [SerializeField] private Color warningLogColor;
    [SerializeField] private Sprite warningLogSprite;
    [SerializeField] private Color errorLogColor;
    [SerializeField] private Sprite errorLogSprite;

    private int debugsThisFrame = 0;
    private InGameDebugLog parentLogger;
    private bool currentlyViewingExtended = false;

    public string logString { get; private set; }
    public string stackTrace { get; private set; }
    public LogType logType { get; private set; }

    // Update; reset debugs this frame to 0 (for use in detecting "infinite" loops)
    private void Update()
    {
        debugsThisFrame = 0;
    }

    // Updates the line content to match a given message. Displays newText as the message's text,
    //     colors the text based on lineType, and logs the time based on current time.
    public void SetLineContent(InGameDebugLog _parentLogger, string _logString, string _stackTrace, LogType _logType)
    {
        parentLogger = _parentLogger;
        logString = _logString;
        stackTrace = _stackTrace;
        logType = _logType;

        // If this frame, there are over 1000 debugs, quit for this frame; there may be an infinite feedback loop
        if (debugsThisFrame > 1000)
        {
            return;
        }
        debugsThisFrame++;

        // Add current time to message
        string currentTime = DateTime.Now.ToString("HH:mm:ss");
        string logStringWithDate = "[" + currentTime + "] " + logString;

        // Reset stylizing (color, log image)
        ResetStylizing();

        // Set text to the new message
        logTextLine1.text = logStringWithDate;
        logTextLine2.text = stackTrace;
    }

    // Resets stylizing (including text color and display image)
    // Based on stored log type (log, warning, error)
    private void ResetStylizing()
    {
        switch (logType)
        {
            case LogType.Log:
                logTextLine1.color = normalLogColor;
                logTextLine2.color = normalLogColor;
                logSymbolImg.sprite = normalLogSprite;
                break;
            case LogType.Warning:
                logTextLine1.color = warningLogColor;
                logTextLine2.color = warningLogColor;
                logSymbolImg.sprite = warningLogSprite;
                break;
            case LogType.Error:
                logTextLine1.color = errorLogColor;
                logTextLine2.color = errorLogColor;
                logSymbolImg.sprite = errorLogSprite;
                break;
            default:
                logTextLine1.color = normalLogColor;
                logTextLine2.color = normalLogColor;
                logSymbolImg.sprite = normalLogSprite;
                break;
        }
    }

    // Stops viewing extended details (update background color)
    public void StopViewExtendedDetails()
    {
        currentlyViewingExtended = false;
        backgroundImg.color = baseBackgroundColor;
    }

    // When clicked, open full message in debug log viewer
    public void OnPointerClick(PointerEventData eventData)
    {
        if (parentLogger == null) 
        { 
            Debug.LogError("Debug line has no parent InGameDebugLog script; cannot print to debug log."); 
            return; 
        }

        backgroundImg.color = selectedBackgroundColor;
        parentLogger.ViewExtendedDetails(this);
        currentlyViewingExtended = true;
    }

    // When hovered, show hover color (unless already viewing extended details)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!currentlyViewingExtended)
        {
            backgroundImg.color = hoveredBackgroundColor;
        }
    }

    // When exit hover, show normal color (unless already viewing extended details)
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!currentlyViewingExtended)
        {
            backgroundImg.color = baseBackgroundColor;
        }
    }
}
