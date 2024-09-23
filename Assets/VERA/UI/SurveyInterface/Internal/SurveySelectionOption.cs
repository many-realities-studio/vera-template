using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SurveySelectionOption : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    // SurveySelectionOption manages a single selectable survey option (for multiple choice or selection questions)


    #region VARIABLES

    [Header("Components")]
    [SerializeField] private Image selectionImg;
    [SerializeField] private Toggle selectionToggle;
    [SerializeField] private TMP_Text selectionText;
    private SurveyManager surveyManager;

    [Header("Customization")]
    [SerializeField] private Color baseBackgroundColor;
    [SerializeField] private Color baseHoveredBackgroundColor;
    [SerializeField] private Color selectedBackgroundColor;
    [SerializeField] private Color selectedHoveredBackgroundColor;

    public bool isSelected { get; private set; } = false;
    public bool isHovered { get; private set; } = false;
    public int sortId { get; private set; }

    #endregion


    #region SETUP

    // Sets up the option
    public void Setup(SurveyManager _surveyManager, int _sortId, string displayText)
    {
        surveyManager = _surveyManager;
        sortId = _sortId;
        selectionText.text = displayText;

        ResetStyling();
    }

    #endregion


    #region POINTER CLICK, ENTER, EXIT

    // On click, select or deselect this option
    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleSelection();
    }

    // On enter, hover this option
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        ResetStyling();
    }

    // On exit, unhover this option
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        ResetStyling();
    }

    #endregion


    #region STYLING

    // Toggles selection on or off
    public void ToggleSelection()
    {
        isSelected = !isSelected;
        surveyManager.OptionWasClicked(this);
        ResetStyling();
    }

    // Resets the styling of this option (color and toggle)
    private void ResetStyling()
    {
        selectionToggle.isOn = isSelected;

        if (isSelected)
        {
            if (isHovered)
                selectionImg.color = selectedHoveredBackgroundColor;
            else
                selectionImg.color = selectedBackgroundColor;
        }
        else
        {
            if (isHovered)
                selectionImg.color = baseHoveredBackgroundColor;
            else
                selectionImg.color = baseBackgroundColor;
        }
    }

    #endregion


}
