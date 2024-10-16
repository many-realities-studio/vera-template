using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SurveySliderOption : MonoBehaviour
{

    // SurveySliderOption handles a slider response option for surveys


    #region VARIABLES

    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text leftText;
    [SerializeField] private TMP_Text rightText;

    #endregion


    #region TEXT

    // Sets the left text to a certain value
    public void SetLeftText(string text)
    {
        leftText.text = text;
    }
    
    // Sets the right text to a certain value
    public void SetRightText(string text)
    {
        rightText.text = text;
    }

    #endregion


    #region SLIDER

    // Returns the value of the slider
    public float GetSliderValue()
    {
        return slider.value;
    }

    // Sets the value of the slider
    public void SetSliderValue(float newValue)
    {
        slider.value = newValue;
    }

    // Gets navigatable item, for use in VLAT
    public GameObject GetNavigatableItem()
    {
        return slider.gameObject;
    }

    #endregion

}
