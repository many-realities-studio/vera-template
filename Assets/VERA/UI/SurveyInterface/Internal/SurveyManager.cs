using Codice.Client.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;

public class SurveyManager : MonoBehaviour
{

    // SurveyManager manages a single survey

    #region VARIABLES

    public SurveyInfo activeSurvey;

    [Header("References")]
    [SerializeField] private RectTransform responseAreaParent;
    [SerializeField] private Transform responseContentParent;
    private CanvasGroup responseContentCanvGroup;
    [SerializeField] private RectTransform questionAreaParent;
    [SerializeField] private TMP_Text questionText;
    private CanvasGroup questionTextCanvGroup;
    [SerializeField] private TMP_Text countDisplay;
    private CanvasGroup countDisplayCanvGroup;
    [SerializeField] private ScrollRect questionScrollRect;
    [SerializeField] private ScrollRect responseScrollRect;

    // Question
    public enum SurveyQuestionType
    {
        MultipleChoice,
        Selection,
        Rating
    }
    private int currentQuestionIndex;

    // Selection and multiple choice
    [SerializeField] private SurveySelectionOption multipleChoiceOptionPrefab;
    [SerializeField] private SurveySelectionOption selectionOptionPrefab;
    private List<SurveySelectionOption> choiceOptions;
    private List<SurveySelectionOption> selectedOptions; // (Selection)
    private SurveySelectionOption selectedOption; // (Multiple choice)

    // Results
    private string[] savedSelections;

    // Animations
    private float swipeTime = 0.25f;

    #endregion


    #region TESTING, TODO DELETE

    private void Start()
    {
        BeginSurvey(activeSurvey);
    }

    #endregion


    #region BEGIN / END

    // Begins a survey based on given info
    public void BeginSurvey(SurveyInfo surveyToBegin)
    {
        activeSurvey = surveyToBegin;

        currentQuestionIndex = -1;
        savedSelections = new string[activeSurvey.surveyQuestions.Count];

        selectedOption = null;
        if (selectedOptions == null)
            selectedOptions = new List<SurveySelectionOption>();
        else
            selectedOptions.Clear();

        if (questionTextCanvGroup == null)
            questionTextCanvGroup = questionText.GetComponent<CanvasGroup>();
        if (countDisplayCanvGroup == null)
            countDisplayCanvGroup = countDisplay.GetComponent<CanvasGroup>();
        if (responseContentCanvGroup == null)
            responseContentCanvGroup = responseContentParent.GetComponent<CanvasGroup>();

        NextQuestion();
    }

     // Completes the currently active survey
    public void CompleteSurvey()
    {
        Debug.Log("Survey complete!");
    }

    #endregion


    #region NEXT / PREVIOUS QUESTION

    // Navigates to the next question
    public void NextQuestion()
    {
        // Save this question's results
        SaveCurrentQuestionResults();

        // Check for last question
        if (currentQuestionIndex >= activeSurvey.surveyQuestions.Count - 1)
        {
            CompleteSurvey();
            return;
        }

        // Incriment to next question
        currentQuestionIndex++;

        // Setup the new question
        StopCoroutine(SetupNewQuestion());
        StartCoroutine(SetupNewQuestion());
    }

    // Navigates to the previous question
    public void PreviousQuestion()
    {
        // Save current question's results
        SaveCurrentQuestionResults();

        // Check if we can go to a previous question
        if (currentQuestionIndex <= 0) return;
                
        // Decrement to previous question
        currentQuestionIndex--;

        // Setup the new question
        StopCoroutine(SetupNewQuestion());
        StartCoroutine(SetupNewQuestion());
    }

    // Sets up a new question
    private IEnumerator SetupNewQuestion()
    {
        // Cancel any existing animations and reset params
        LeanTween.cancel(responseContentCanvGroup.gameObject);
        responseContentCanvGroup.alpha = 1f;
        LeanTween.cancel(questionTextCanvGroup.gameObject);
        questionTextCanvGroup.alpha = 1f;
        LeanTween.cancel(countDisplayCanvGroup.gameObject);
        countDisplayCanvGroup.alpha = 1f;
        LeanTween.cancel(questionAreaParent.gameObject);
        LeanTween.cancel(responseAreaParent.gameObject);

        // Begin fade animation        
        responseContentCanvGroup.LeanAlpha(0f, swipeTime);
        questionTextCanvGroup.LeanAlpha(0f, swipeTime);
        countDisplayCanvGroup.LeanAlpha(0f, swipeTime);

        // Wait for swipe animation to complete, then remove existing options
        yield return new WaitForSeconds(swipeTime);

        RemoveDisplayedOptions();

        // Set the question count
        countDisplay.text = "" + (currentQuestionIndex + 1) + " / " + activeSurvey.surveyQuestions.Count;

        // Setup new question based on question type
        switch (activeSurvey.surveyQuestions[currentQuestionIndex].questionType)
        {
            case SurveyQuestionType.MultipleChoice:
                SetupChoiceOrSelection(multipleChoiceOptionPrefab);
                break;
            case SurveyQuestionType.Selection:
                SetupChoiceOrSelection(selectionOptionPrefab);
                break;
            case SurveyQuestionType.Rating:
                break;
        }

        // Set the question text and update desired size
        questionText.text = activeSurvey.surveyQuestions[currentQuestionIndex].questionText;
        LayoutRebuilder.ForceRebuildLayoutImmediate(questionText.rectTransform);
        float desiredQuestionHeight = questionText.rectTransform.sizeDelta.y + 10f;

        if (desiredQuestionHeight > 100f)
            desiredQuestionHeight = 100f;

        LeanTween.value(questionAreaParent.gameObject, questionAreaParent.sizeDelta.y, desiredQuestionHeight, swipeTime).setEaseInOutQuad().setOnUpdate((value) =>
        {
            questionAreaParent.sizeDelta = new Vector2(questionAreaParent.sizeDelta.x, value);
            questionScrollRect.verticalNormalizedPosition = 1f;
        });
        LeanTween.value(responseAreaParent.gameObject, responseAreaParent.sizeDelta.y, 336f - desiredQuestionHeight, swipeTime).setEaseInOutQuad().setOnUpdate((value) =>
        {
            responseAreaParent.sizeDelta = new Vector2(responseAreaParent.sizeDelta.x, value);
            responseScrollRect.verticalNormalizedPosition = 1f;
        });

        // Begin fade animation
        responseContentCanvGroup.LeanAlpha(1f, swipeTime);
        questionTextCanvGroup.LeanAlpha(1f, swipeTime);
        countDisplayCanvGroup.LeanAlpha(1f, swipeTime);
    }

    #endregion


    #region SETUP NEW OPTIONS

    // Sets up multiple choice or selection question based on current question index
    private void SetupChoiceOrSelection(SurveySelectionOption optionPrefab)
    {
        // Clear choice options
        if (choiceOptions == null)
            choiceOptions = new List<SurveySelectionOption>();
        else
            choiceOptions.Clear();

        List<int> optionsWhichNeedToBeOn = new List<int>();
        // If data exists from previous response entry, load which options were selected
        if (savedSelections[currentQuestionIndex] != null && savedSelections[currentQuestionIndex] != "")
        {
            // Split the string by commas and trim spaces from each part
            string[] parts = savedSelections[currentQuestionIndex].Split(',')
                             .Select(part => part.Trim())
                             .ToArray();

            if (parts.Length > 1 && parts[1] != "N/A")
            {
                // Skip the first part (the signifier) and parse the remaining parts as integers
                optionsWhichNeedToBeOn = parts.Skip(1)
                                   .Select(int.Parse)
                                   .ToList();
            }
        }

        // Set up new choice options
        int i;
        for (i = 0; i < activeSurvey.surveyQuestions[currentQuestionIndex].optionsDisplayStrings.Length; i++)
        {
            // Spawn and setup new response option
            SurveySelectionOption newOption = GameObject.Instantiate(optionPrefab, responseContentParent);
            newOption.Setup(this, i, activeSurvey.surveyQuestions[currentQuestionIndex].optionsDisplayStrings[i]);
            choiceOptions.Add(newOption);

            if (optionsWhichNeedToBeOn.Contains(i))
            {
                newOption.ToggleSelection();
            }
        }
    }

    // Removes all displayed response options
    private void RemoveDisplayedOptions()
    {
        selectedOption = null;

        if (selectedOptions != null)
            selectedOptions.Clear();

        foreach (Transform t in responseContentParent)
        {
            GameObject.Destroy(t.gameObject);
        }
    }

    #endregion


    #region SAVE

    // Saves current question results to array
    private void SaveCurrentQuestionResults()
    {
        if (currentQuestionIndex < 0 || currentQuestionIndex >= activeSurvey.surveyQuestions.Count) return;

        // Build a save string with the necessary info inside it
        string saveString = "";

        // Save string based on question type
        switch (activeSurvey.surveyQuestions[currentQuestionIndex].questionType)
        {
            // Multiple choice, save selected answer (e.g., "MultipleChoice: A")
            case SurveyQuestionType.MultipleChoice:
                saveString += "MultipleChoice, ";
                if (selectedOption == null)
                {
                    saveString += "N/A";
                }
                else
                {
                    saveString += selectedOption.sortId;
                }
                break;
            // Selection, save selected answers (e.g., "Selection: A, B, D")
            case SurveyQuestionType.Selection:
                saveString += "Selection, ";
                if (selectedOptions.Count == 0)
                {
                    saveString += "N/A";
                }
                else
                {
                    saveString += selectedOptions[0].sortId;
                    for (int i = 1; i < selectedOptions.Count; i++)
                    {
                        saveString += ", " + selectedOptions[i].sortId; 
                    }
                }
                break;
            // Rating, TODO
            case SurveyQuestionType.Rating:
                break;
        }
        savedSelections[currentQuestionIndex] = saveString;

        // TODO, DELETE
        Debug.Log(saveString);
    }

    #endregion


    #region SELECTION

    // Called when an option is clicked; update selected options
    public void OptionWasClicked(SurveySelectionOption option)
    {
        // Multiple choice, only one option can be active at a time
        if (activeSurvey.surveyQuestions[currentQuestionIndex].questionType == SurveyQuestionType.MultipleChoice)
        {
            // Selected new option (off -> on)
            if (option.isSelected)
            {
                // If we are selecting a new option while a different one is already selected, deactivate old option
                if (selectedOption != null && selectedOption != option)
                {
                    selectedOption.ToggleSelection();
                }
                // Set newly selected option to be the actively selected option
                selectedOption = option;
            }
            // Deselected new option (on -> off)
            else
            {
                // If we are deselecting the currently selected option, set selected option to null
                if (selectedOption != null && selectedOption == option)
                {
                    selectedOption = null;
                }
            }
        }
        // Selection, multiple options can be active at a time
        else if (activeSurvey.surveyQuestions[currentQuestionIndex].questionType == SurveyQuestionType.Selection)
        {
            // If we are activating an option, add it to the selected options list if needed
            if (option.isSelected)
            {
                if (!selectedOptions.Contains(option))
                {
                    selectedOptions.Add(option);
                }
            }
            // If we are deactivating an option, remove it from the selected options list if needed
            else
            {
                if (selectedOptions.Contains(option))
                {
                    selectedOptions.Remove(option);
                }
            }
        }
    }

    #endregion


}

[System.Serializable]
public class SurveyInfo
{

    // SurveyInfo is a data container for defining a single survey

    public string surveyName;
    public List<SurveyQuestionInfo> surveyQuestions;

}

[System.Serializable]
public class SurveyQuestionInfo
{

    // SurveyQuestionInfo is a data container for defining what a single survey question consists of

    public SurveyManager.SurveyQuestionType questionType;
    [TextArea] public string questionText;

    public string[] optionsDisplayStrings;

}
