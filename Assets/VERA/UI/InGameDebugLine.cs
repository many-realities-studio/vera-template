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
    
    [SerializeField] private RectTransform lineParentTransform;
    [SerializeField] private TMP_Text logTextLine;
    [SerializeField] private Color normalLogColor;
    [SerializeField] private Color warningLogColor;
    [SerializeField] private Color errorLogColor;
    private int debugsThisFrame = 0;

    // Update; reset debugs this frame to 0 (for use in detecting "infinite" loops)
    private void Update()
    {
        debugsThisFrame = 0;
    }

    // Updates the line content to match a given message. Displays newText as the message's text,
    //     colors the text based on lineType, and logs the time based on current time.
    public void SetLineContent(string newText, LogType newLogType)
    {
        // If this frame, there are over 1000 debugs, quit for this frame; there may be an infinite feedback loop
        if (debugsThisFrame > 1000)
        {
            return;
        }
        debugsThisFrame++;

        // Add current time to message
        string currentTime = DateTime.Now.ToString("HH:mm:ss");
        newText = "[" + currentTime + "] " + newText;

        // Set text to the new message and update color
        logTextLine.text = newText;

        switch (newLogType)
        {
            case LogType.Log:
                logTextLine.color = normalLogColor;
                break;
            case LogType.Warning:
                logTextLine.color = warningLogColor;
                break;
            case LogType.Error:
                logTextLine.color = errorLogColor;
                break;
            default:
                logTextLine.color = normalLogColor;
                break;
        }

        // Update log box size to message size (e.g., if spillover to 2nd or 3rd line, update height)
        //     Forces reload of content size fitter, so to get the correct size
        ContentSizeFitter contentSizeFitter = logTextLine.GetComponent<ContentSizeFitter>();
        contentSizeFitter.enabled = false;

        LayoutRebuilder.ForceRebuildLayoutImmediate(logTextLine.GetComponent<RectTransform>());

        contentSizeFitter.enabled = true;

        RectTransform textRectTransform = logTextLine.GetComponent<RectTransform>();
        float preferredHeight = logTextLine.preferredHeight;

        lineParentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
    }
}
