using Codice.Client.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using static System.Net.Mime.MediaTypeNames;
using static UnityEngine.GraphicsBuffer;

public class SurveyManager : MonoBehaviour
{

    // SurveyManager manages a single survey

    #region VARIABLES

    // Overall survey
    public SurveyInfo activeSurvey;
    private bool surveyStarted = false;

    // References
    [Header("References")]
    [SerializeField] private Canvas managerCanvas;
    private CanvasGroup managerCanvGroup;
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
    [SerializeField] private GameObject moreOptionsNotif;
    private CanvasGroup moreOptionsCanvGroup;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    // Question
    public enum SurveyQuestionType
    {
        MultipleChoice,
        Selection,
        Rating
    }
    private int currentQuestionIndex;
    private float fullQuestionResponseHeight;
    private float maxQuestionHeight = 300f;
    private bool[] moreOptionsNotifNeeded;

    // Selection and multiple choice
    [SerializeField] private SurveySelectionOption multipleChoiceOptionPrefab;
    [SerializeField] private SurveySelectionOption selectionOptionPrefab;
    [SerializeField] private TMP_Text textAreaPrefab;
    private List<SurveySelectionOption> choiceOptions;
    private List<SurveySelectionOption> selectedOptions; // (Selection)
    private SurveySelectionOption selectedOption; // (Multiple choice)

    // Start and results
    private string[] savedSelections;

    // Animations
    private float swipeTime = 0.25f;

    #endregion


    #region MONOBEHAVIOUR

    // Start, hide the canvas
    private void Start()
    {
        managerCanvGroup = managerCanvas.GetComponent<CanvasGroup>();
        questionTextCanvGroup = questionText.GetComponent<CanvasGroup>();
        countDisplayCanvGroup = countDisplay.GetComponent<CanvasGroup>();
        responseContentCanvGroup = responseContentParent.GetComponent<CanvasGroup>();
        moreOptionsCanvGroup = moreOptionsNotif.GetComponent<CanvasGroup>();

        fullQuestionResponseHeight = responseAreaParent.sizeDelta.y + questionAreaParent.sizeDelta.y;

        moreOptionsNotif.SetActive(false);

        managerCanvGroup.alpha = 0f;
        managerCanvas.gameObject.SetActive(false);

        // TESTING, TODO: delete
        BeginSurvey(activeSurvey);
    }

    #endregion


    #region BEGIN / END

    // Begins a survey based on given info
    public void BeginSurvey(SurveyInfo surveyToBegin)
    {
        surveyStarted = false;
        managerCanvas.gameObject.SetActive(true);

        LeanTween.cancel(managerCanvGroup.gameObject);
        managerCanvGroup.alpha = 0f;
        managerCanvGroup.LeanAlpha(1f, swipeTime);

        activeSurvey = surveyToBegin;

        currentQuestionIndex = -2;
        savedSelections = new string[activeSurvey.surveyQuestions.Count];
        moreOptionsNotifNeeded = new bool[activeSurvey.surveyQuestions.Count];

        for (int i = 0; i < moreOptionsNotifNeeded.Length; i++) 
            moreOptionsNotifNeeded[i] = true;

        selectedOption = null;
        if (selectedOptions == null)
            selectedOptions = new List<SurveySelectionOption>();
        else
            selectedOptions.Clear();

        countDisplayCanvGroup.alpha = 0f;

        NextQuestion();
    }

     // Completes the currently active survey and hide the window
    public void CompleteSurvey()
    {
        // TODO, functionality for pushing results or showing an end screen
        currentQuestionIndex++;
        StartCoroutine(DisplayEndScreen());
    }

    // Hides the window by fading it out, then deactivating it
    private IEnumerator HideWindow()
    {
        LeanTween.cancel(managerCanvGroup.gameObject);
        managerCanvGroup.LeanAlpha(0f, swipeTime);

        yield return new WaitForSeconds(swipeTime);

        managerCanvas.gameObject.SetActive(false);
    }

    #endregion


    #region ANIMATION HELPERS

    // Fades out swipe animations for the question/response area
    private void FadeOutSwipeAnims()
    {
        // Cancel any existing animations and reset params
        LeanTween.cancel(responseContentCanvGroup.gameObject);
        responseContentCanvGroup.alpha = 1f;
        LeanTween.cancel(questionTextCanvGroup.gameObject);
        questionTextCanvGroup.alpha = 1f;
        LeanTween.cancel(countDisplayCanvGroup.gameObject);
        //countDisplayCanvGroup.alpha = 1f;
        LeanTween.cancel(questionAreaParent.gameObject);
        LeanTween.cancel(responseAreaParent.gameObject);

        // Begin fade animation        
        responseContentCanvGroup.LeanAlpha(0f, swipeTime);
        questionTextCanvGroup.LeanAlpha(0f, swipeTime);
        countDisplayCanvGroup.LeanAlpha(0f, swipeTime);
    }

    // Fades in swipe animations for the question/response area
    private void FadeInSwipeAnims()
    {
        countDisplayCanvGroup.LeanAlpha(1f, swipeTime);
        FadeInSwipeAnimsNoCount();
    }

    // Fades in swipe animations, no count
    private void FadeInSwipeAnimsNoCount()
    {
        responseContentCanvGroup.LeanAlpha(1f, swipeTime);
        questionTextCanvGroup.LeanAlpha(1f, swipeTime);
    }

    // Updates the question/response area sizes based on their content
    private void UpdateQuestionResponseSizes()
    {
        // Get desired heights
        LayoutRebuilder.ForceRebuildLayoutImmediate(questionText.rectTransform);
        float desiredQuestionHeight = questionText.rectTransform.sizeDelta.y + 10f;

        if (desiredQuestionHeight > maxQuestionHeight)
            desiredQuestionHeight = maxQuestionHeight;

        float desiredResponseHeight = fullQuestionResponseHeight - desiredQuestionHeight;

        // Animate to desired heights
        LeanTween.value(questionAreaParent.gameObject, questionAreaParent.sizeDelta.y, desiredQuestionHeight, swipeTime).setEaseInOutQuad().setOnUpdate((value) =>
        {
            questionAreaParent.sizeDelta = new Vector2(questionAreaParent.sizeDelta.x, value);
            questionScrollRect.verticalNormalizedPosition = 1f;
        });
        LeanTween.value(responseAreaParent.gameObject, responseAreaParent.sizeDelta.y, desiredResponseHeight, swipeTime).setEaseInOutQuad().setOnUpdate((value) =>
        {
            responseAreaParent.sizeDelta = new Vector2(responseAreaParent.sizeDelta.x, value);
            responseScrollRect.verticalNormalizedPosition = 1f;
        });

        StartCoroutine(CheckForMoreOptionsNotif(desiredResponseHeight));
    }

    // Displays the notification that there are more options if there are
    private IEnumerator CheckForMoreOptionsNotif(float desiredResponseHeight)
    {
        // Wait for content to update
        yield return null;
        yield return null;

        // If check is not needed, we are okay.
        if (currentQuestionIndex >= 0 && currentQuestionIndex < activeSurvey.surveyQuestions.Count && !moreOptionsNotifNeeded[currentQuestionIndex])
        {
            nextButton.interactable = true;
            yield break;
        }

        // If content fits, we are okay.
        if (responseContentParent.GetComponent<RectTransform>().sizeDelta.y - 15f < desiredResponseHeight)
        {
            nextButton.interactable = true;
            yield break;
        }

        // Content does not fit, show notification
        LeanTween.cancel(moreOptionsCanvGroup.gameObject);
        moreOptionsCanvGroup.alpha = 0f;
        moreOptionsNotif.gameObject.SetActive(true);
        moreOptionsCanvGroup.LeanAlpha(1f, swipeTime);

        yield return new WaitForSeconds(swipeTime);

        moreOptionsCanvGroup.LeanAlpha(.7f, swipeTime).setLoopPingPong();

        // Wait until user has scrolled to see all options
        while (responseScrollRect.verticalNormalizedPosition >= 0.01f)
        {
            yield return null;
        }

        // Options seen, hide notif
        LeanTween.cancel(moreOptionsCanvGroup.gameObject);
        moreOptionsCanvGroup.alpha = 0f;
        moreOptionsNotif.gameObject.SetActive(false);
        nextButton.interactable = true;
    }

    #endregion


    #region NEXT / PREVIOUS QUESTION

    // Navigates to the next question
    public void NextQuestion()
    {
        // Check for start screen
        if (currentQuestionIndex == -2)
        {
            StopCoroutine(DisplayStartScreen());
            StartCoroutine(DisplayStartScreen());
            currentQuestionIndex++;
            return;
        }

        if (currentQuestionIndex == -1)
            nextButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Next";

        // Check for end screen
        if (currentQuestionIndex >= activeSurvey.surveyQuestions.Count)
        {
            StartCoroutine(HideWindow());
            return;
        }

        previousButton.interactable = true;

        // Save this question's results
        SaveCurrentQuestionResults();

        // Check for last question
        if (currentQuestionIndex >= activeSurvey.surveyQuestions.Count - 1)
        {
            CompleteSurvey();
            return;
        }

        // Save that we visited this question and don't need the notification of more options
        if (currentQuestionIndex >= 0)
            moreOptionsNotifNeeded[currentQuestionIndex] = false;

        // Incriment to next question
        currentQuestionIndex++;

        // Setup the new question
        StopCoroutine(SetupNewQuestion());
        StartCoroutine(SetupNewQuestion());
    }

    // Navigates to the previous question
    public void PreviousQuestion()
    {
        // Check if we are on the final screen
        if (currentQuestionIndex == activeSurvey.surveyQuestions.Count)
            nextButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Next";
        else
            SaveCurrentQuestionResults();

        // Check if we can go to a previous question
        if (currentQuestionIndex <= -1) return;

        // Disable more options notification of current question
        LeanTween.cancel(moreOptionsCanvGroup.gameObject);
        moreOptionsCanvGroup.alpha = 0f;

        // Check if we need to go to the start screen
        if (currentQuestionIndex == 0)
        {
            StopCoroutine(DisplayStartScreen());
            StartCoroutine(DisplayStartScreen());
            currentQuestionIndex--;
            return;
        }
                
        // Decrement to previous question
        currentQuestionIndex--;

        // Setup the new question
        StopCoroutine(SetupNewQuestion());
        StartCoroutine(SetupNewQuestion());
    }

    // Sets up a new question
    private IEnumerator SetupNewQuestion()
    {
        nextButton.interactable = false;
        previousButton.interactable = false;

        FadeOutSwipeAnims();

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

        UpdateQuestionResponseSizes();

        previousButton.interactable = true;

        FadeInSwipeAnims();
    }

    #endregion


    #region START / END SCREEN

    // Displays the survey's start screen
    private IEnumerator DisplayStartScreen()
    {
        nextButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Start";

        previousButton.interactable = false;

        // If the survey has already started (e.g., we are coming from question 1), remove old components
        if (surveyStarted)
        {
            FadeOutSwipeAnims();

            // Wait for swipe animation to complete, then remove existing options
            yield return new WaitForSeconds(swipeTime);

            RemoveDisplayedOptions();
        }

        // Set the description
        TMP_Text newBlock = GameObject.Instantiate(textAreaPrefab, responseContentParent);
        newBlock.text = activeSurvey.surveyDescription;

        // Set the question text and update desired size
        questionText.text = activeSurvey.surveyName;

        UpdateQuestionResponseSizes();

        FadeInSwipeAnimsNoCount();

        surveyStarted = true;
    }

    // Displays the survey's end screen
    private IEnumerator DisplayEndScreen()
    {
        nextButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Finish";

        FadeOutSwipeAnims();

        // Wait for swipe animation to complete, then remove existing options
        yield return new WaitForSeconds(swipeTime);

        RemoveDisplayedOptions();

        // Set the description
        TMP_Text newBlock = GameObject.Instantiate(textAreaPrefab, responseContentParent);
        newBlock.text = activeSurvey.surveyEndStatement;

        // Set the question text and update desired size
        questionText.text = "Survey complete";

        UpdateQuestionResponseSizes();

        FadeInSwipeAnimsNoCount();

        previousButton.interactable = true;
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
    [TextArea] public string surveyDescription;
    [TextArea] public string surveyEndStatement;
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
