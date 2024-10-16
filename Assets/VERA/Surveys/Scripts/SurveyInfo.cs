using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "SurveyInfo", menuName = "VERA/Survey Info")]
public class SurveyInfo : ScriptableObject
{

    // SurveyInfo is a data container for defining a single survey


    #region VARIABLES

    public string surveyName;
    [TextArea] public string surveyDescription;
    [TextArea] public string surveyEndStatement;
    public List<SurveyQuestionInfo> surveyQuestions;

    #endregion

}

[System.Serializable]
public class SurveyQuestionInfo
{

    // SurveyQuestionInfo is a data container for defining what a single survey question consists of


    #region OVERVIEW

    public enum SurveyQuestionType
    {
        MultipleChoice,
        Selection,
        Slider,
        Matrix
    }

    [Header("Overview")]
    [Tooltip("The type of question; Multiple Choice - presented with a series of choices, choose only one; " +
        "Selection - presented with a series of choices, choose as many as you wish; " +
        "Slider - presented with a slider which may be adjusted between a minimum and maximum value; " +
        "Matrix - presented with a matrix table, of which may be populated with options and scale choices")]
    public SurveyQuestionType questionType;
    [Tooltip("The question's displayed text, written as should be displayed to the survey taker")]
    [TextArea] public string questionText;

    #endregion


    #region MULTIPLE CHOICE / SELECTION

    // Multiple choice / selection
    [Header("Multiple Choice / Selection")]
    [Tooltip("The options the survey taker may choose from, written as should be displayed to the survey taker")]
    public string[] selectionOptions;

    #endregion


    #region SLIDER

    // Slider
    [Header("Slider")]
    [Tooltip("The text which will display to the left of the slider, indicating what the slider indicates when slid to the left")]
    public string leftSliderText;
    [Tooltip("The text which will display to the right of the slider, indicating what the slider indicates when slid to the right")]
    public string rightSliderText;

    #endregion


    #region MATRIX

    // Matrix
    [Header("Matrix")]
    [Tooltip("The text that will be displayed on the columns of the matrix (commonly, could be Likert scale values, or numerical ratings)")]
    public string[] matrixColumnTexts;
    [Tooltip("The text that will be displayed on the rows of the matrix (commonly, could be individual questions)")]
    public string[] matrixRowTexts;

    #endregion

}