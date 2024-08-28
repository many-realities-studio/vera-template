using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class InGameDebugLine : MonoBehaviour
{

    // InGameDebugLine handles a single line of text which is logged in the in-game 
    //     debug logger (e.g., InGameDebugLog.cs)

    [Header("Components")]
    [SerializeField] private RectTransform lineParentTransform;
    [SerializeField] private TMP_Text logTextLine1;
    [SerializeField] private TMP_Text logTextLine2;
    [SerializeField] private Image logSymbolImg;

    [Header("Customization")]
    [SerializeField] private bool useClassicStyle = true;
    [SerializeField] private Color normalLogColor;
    [SerializeField] private Sprite normalLogSprite;
    [SerializeField] private Color warningLogColor;
    [SerializeField] private Sprite warningLogSprite;
    [SerializeField] private Color errorLogColor;
    [SerializeField] private Sprite errorLogSprite;

    private int debugsThisFrame = 0;

    // Update; reset debugs this frame to 0 (for use in detecting "infinite" loops)
    private void Update()
    {
        debugsThisFrame = 0;
    }

    // Updates the line content to match a given message. Displays newText as the message's text,
    //     colors the text based on lineType, and logs the time based on current time.
    public void SetLineContent(string logString, string stackTrace, LogType logType)
    {
        // If this frame, there are over 1000 debugs, quit for this frame; there may be an infinite feedback loop
        if (debugsThisFrame > 1000)
        {
            return;
        }
        debugsThisFrame++;

        // Add current time to message
        string currentTime = DateTime.Now.ToString("HH:mm:ss");
        logString = "[" + currentTime + "] " + logString;

        // Reset stylizing (color, log image)
        ResetStylizing(logType);

        // Set text to the new message
        logTextLine1.text = logString;
        logTextLine2.text = stackTrace;
    }

    // Resets stylizing (including text color and display image)
    // Based on given log type (log, warning, error)
    private void ResetStylizing(LogType newLogType)
    {
        switch (newLogType)
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
}
