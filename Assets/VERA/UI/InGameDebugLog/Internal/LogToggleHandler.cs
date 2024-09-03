using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LogToggleHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    // LogToggleHandler handles when a log toggle button has been hovered or pressed

    [SerializeField] private LogType logType;
    [SerializeField] private InGameDebugLog debugLogger;
    [SerializeField] private Image displayImg;

    private bool isToggleOn = true;
    private bool isHovered = false;

    // On click, initiate toggle
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (logType)
        {
            case LogType.Log:
                debugLogger.ToggleViewNormalLogs();
                break;
            case LogType.Warning:
                debugLogger.ToggleViewWarnings();
                break;
            case LogType.Error:
                debugLogger.ToggleViewErrors();
                break;
        }

        isToggleOn = !isToggleOn;
        UpdateVisuals();
    }

    // On enter, show hovered image
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateVisuals();
    }

    // On exit, show normal image
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisuals();
    }

    // Updates visual display according to hovered or toggled
    private void UpdateVisuals()
    {
        if (isHovered)
        {
            if (isToggleOn)
            {
                float col = 95f / 255f;
                displayImg.color = new Color(col, col, col, 1f);
            }
            else
            {
                float col = 55f / 255f;
                displayImg.color = new Color(col, col, col, 1f);
            }
        }
        else
        {
            if (isToggleOn)
            {
                float col = 64f / 255f;
                displayImg.color = new Color(col, col, col, 1f);
            }
            else
            {
                float col = 45f / 255f;
                displayImg.color = new Color(col, col, col, 1f);
            }
        }
    }
}
