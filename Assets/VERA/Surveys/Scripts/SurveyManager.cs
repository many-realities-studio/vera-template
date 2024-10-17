using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class SurveyManager : MonoBehaviour
{

    // SurveyManager manages a single survey

    #region VARIABLES

    // Overall survey
    private SurveyInfo activeSurvey;
    private bool surveyStarted = false;
    private SurveyInterfaceIO surveyInterfaceIo;

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

    // Slider
    [SerializeField] private SurveySliderOption sliderOptionPrefab;
    private SurveySliderOption activeSliderOption;

    // Matrix
    [SerializeField] private MatrixOptions matrixOptionsPrefab;
    [SerializeField] private GameObject matrixSeparatorLine;
    private MatrixOptions matrixOptions;

    // Start and results
    private string[] surveyResults;

    // Animations
    private float swipeTime = 0.25f;

    // Accessibility
    private VLAT_MenuNavigator vlatMenuNav;
    private int infLoopDetect = 0;

    #endregion


    #region SETUP

    // Sets up the manager and its various components
    public void Setup()
    {
        surveyInterfaceIo = GetComponent<SurveyInterfaceIO>();
        managerCanvGroup = managerCanvas.GetComponent<CanvasGroup>();
        questionTextCanvGroup = questionText.GetComponent<CanvasGroup>();
        countDisplayCanvGroup = countDisplay.GetComponent<CanvasGroup>();
        responseContentCanvGroup = responseContentParent.GetComponent<CanvasGroup>();
        moreOptionsCanvGroup = moreOptionsNotif.GetComponent<CanvasGroup>();

        fullQuestionResponseHeight = responseAreaParent.sizeDelta.y + questionAreaParent.sizeDelta.y;

        moreOptionsNotif.SetActive(false);

        vlatMenuNav = gameObject.AddComponent<VLAT_MenuNavigator>();
        vlatMenuNav.ManualSetup();
        vlatMenuNav.onChangeHighlightedItem.AddListener(OnHighlightedItemChanged);

        managerCanvGroup.alpha = 0f;
        managerCanvas.gameObject.SetActive(false);
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
        surveyResults = new string[activeSurvey.surveyQuestions.Count];
        moreOptionsNotifNeeded = new bool[activeSurvey.surveyQuestions.Count];

        for (int i = 0; i < moreOptionsNotifNeeded.Length; i++) 
            moreOptionsNotifNeeded[i] = true;

        selectedOption = null;
        if (selectedOptions == null)
            selectedOptions = new List<SurveySelectionOption>();
        else
            selectedOptions.Clear();

        countDisplayCanvGroup.alpha = 0f;

        vlatMenuNav.StartMenuNavigation();

        NextQuestion();
    }

     // Completes the currently active survey and hide the window
    public void CompleteSurvey()
    {
        currentQuestionIndex++;
        StartCoroutine(DisplayEndScreen());
    }

    // Hides the window by fading it out, then deactivating it
    private IEnumerator HideWindow()
    {
        vlatMenuNav.StopMenuNavigation();

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
    private void UpdateQuestionResponseSizes(bool overrideNextButtonActivation)
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

        StartCoroutine(CheckForMoreOptionsNotif(desiredResponseHeight, overrideNextButtonActivation));
    }

    // Override of above
    private void UpdateQuestionResponseSizes()
    {
        UpdateQuestionResponseSizes(false);
    }

    // Displays the notification that there are more options if there are
    private IEnumerator CheckForMoreOptionsNotif(float desiredResponseHeight, bool overrideNextButtonActivation)
    {
        // Wait for content to update
        yield return null;
        yield return null;
        yield return null;

        // If check is not needed, we are okay.
        if (currentQuestionIndex >= 0 && currentQuestionIndex < activeSurvey.surveyQuestions.Count && !moreOptionsNotifNeeded[currentQuestionIndex])
        {
            if (!overrideNextButtonActivation)
                nextButton.interactable = true;
            yield break;
        }

        // If content fits, we are okay.
        if (responseContentParent.GetComponent<RectTransform>().sizeDelta.y - 15f < desiredResponseHeight)
        {
            if (!overrideNextButtonActivation)
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

        if (!overrideNextButtonActivation)
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
        {
            TMP_Text nextText = nextButton.transform.GetComponentInChildren<TMP_Text>();
            nextText.text = "Next";
        }
            

        // Check for end screen
        if (currentQuestionIndex == activeSurvey.surveyQuestions.Count)
        {
            currentQuestionIndex++;
            StartCoroutine(UploadSurvey());
            return;
        }

        // Check for upload complete screen
        if (currentQuestionIndex >= activeSurvey.surveyQuestions.Count + 1)
        {
            StartCoroutine(HideWindow());
            return;
        }

        previousButton.interactable = true;

        // Save this question's results
        SaveCurrentQuestionResults();

        // Save that we visited this question and don't need the notification of more options
        if (currentQuestionIndex >= 0)
            moreOptionsNotifNeeded[currentQuestionIndex] = false;

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
        // Check if we are on the final screen
        if (currentQuestionIndex == activeSurvey.surveyQuestions.Count)
        {
            TMP_Text nextText = nextButton.transform.GetComponentInChildren<TMP_Text>();
            nextText.text = "Next";
        }
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

        vlatMenuNav.ClearNavigatableItems();

        // Setup new question based on question type
        switch (activeSurvey.surveyQuestions[currentQuestionIndex].questionType)
        {
            case SurveyQuestionInfo.SurveyQuestionType.MultipleChoice:
                SetupChoiceOrSelection(multipleChoiceOptionPrefab);
                break;
            case SurveyQuestionInfo.SurveyQuestionType.Selection:
                SetupChoiceOrSelection(selectionOptionPrefab);
                break;
            case SurveyQuestionInfo.SurveyQuestionType.Slider:
                SetupSlider();
                break;
            case SurveyQuestionInfo.SurveyQuestionType.Matrix:
                matrixSeparatorLine.SetActive(true);
                SetupMatrix();
                break;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(responseContentParent.GetComponent<RectTransform>());

        // Set the question text and update desired size
        questionText.text = activeSurvey.surveyQuestions[currentQuestionIndex].questionText;

        UpdateQuestionResponseSizes();

        previousButton.interactable = true;

        FadeInSwipeAnims();

        vlatMenuNav.AddNavigatableItem(previousButton.gameObject);
        vlatMenuNav.AddNavigatableItem(nextButton.gameObject);
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

        matrixSeparatorLine.SetActive(false);
    }

    #endregion


    #region START / END SCREEN

    // Displays the survey's start screen
    private IEnumerator DisplayStartScreen()
    {
        vlatMenuNav.ClearNavigatableItems();

        TMP_Text nextText = nextButton.transform.GetComponentInChildren<TMP_Text>();
        nextText.text = "Start";

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

        vlatMenuNav.AddNavigatableItem(nextButton.gameObject);
    }

    // Displays the survey's end screen
    private IEnumerator DisplayEndScreen()
    {
        vlatMenuNav.ClearNavigatableItems();

        TMP_Text nextText = nextButton.transform.GetComponentInChildren<TMP_Text>();
        nextText.text = "Finish";

        FadeOutSwipeAnims();

        // Wait for swipe animation to complete, then remove existing options
        yield return new WaitForSeconds(swipeTime);

        RemoveDisplayedOptions();

        // Set the description
        TMP_Text newBlock = GameObject.Instantiate(textAreaPrefab, responseContentParent);
        newBlock.text = activeSurvey.surveyEndStatement + "\n\nOnce you continue, your results will be uploaded, " +
            "and you will not be able to change your survey response.";

        // Set the question text and update desired size
        questionText.text = "Survey complete";

        UpdateQuestionResponseSizes();

        FadeInSwipeAnimsNoCount();

        previousButton.interactable = true;

        vlatMenuNav.AddNavigatableItem(nextButton.gameObject);
        vlatMenuNav.AddNavigatableItem(previousButton.gameObject);
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
            case SurveyQuestionInfo.SurveyQuestionType.MultipleChoice:
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
            case SurveyQuestionInfo.SurveyQuestionType.Selection:
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
            // Slider, save slider value (as float)
            case SurveyQuestionInfo.SurveyQuestionType.Slider:
                saveString += "Slider, ";
                saveString += activeSliderOption.GetSliderValue().ToString();
                break;
            // Matrix, save row selection options
            case SurveyQuestionInfo.SurveyQuestionType.Matrix:
                saveString += "Matrix, ";
                List<int> activeIndexes = matrixOptions.GetActiveIndexes();
                if (activeIndexes.Count == 0)
                {
                    saveString += "N/A";
                }
                else
                {
                    if (activeIndexes[0] < 0)
                        saveString += "N/A";
                    else
                        saveString += activeIndexes[0].ToString();

                    for (int i = 1; i < activeIndexes.Count; i++)
                    {
                        if (activeIndexes[i] < 0)
                            saveString += ", N/A";
                        else
                            saveString += ", " + activeIndexes[i].ToString();
                    }
                }
                break;
        }
        surveyResults[currentQuestionIndex] = saveString;
    }

    #endregion


    #region UPLOAD

    // Calls IO to upload survey, and updates screen to notify user
    private IEnumerator UploadSurvey()
    {
        vlatMenuNav.ClearNavigatableItems();

        previousButton.interactable = false;
        nextButton.interactable = false;

        FadeOutSwipeAnims();

        // Wait for swipe animation to complete, then remove existing options
        yield return new WaitForSeconds(swipeTime);

        RemoveDisplayedOptions();

        // Set the description
        TMP_Text newBlock = GameObject.Instantiate(textAreaPrefab, responseContentParent);
        newBlock.text = "Please wait for survey results to finish uploading...";

        // Set the question text and update desired size
        questionText.text = "Survey results are being uploaded";

        UpdateQuestionResponseSizes(true);

        FadeInSwipeAnimsNoCount();

        // Wait for IO to finish uploading (and ensure we have waited at least 1.5 seconds overall)
        float currentTime = Time.time;
        yield return StartCoroutine(surveyInterfaceIo.OutputSurveyResults(activeSurvey, surveyResults));
        if (currentTime + 1.5f > Time.time)
        {
            yield return new WaitForSeconds((currentTime + 1.5f) - Time.time);
        }

        // Display that upload has completed
        FadeOutSwipeAnims();
        yield return new WaitForSeconds(swipeTime);
        RemoveDisplayedOptions();

        newBlock = GameObject.Instantiate(textAreaPrefab, responseContentParent);
        newBlock.text = "Survey results have finished uploading. You may now safely exit the survey.";
        questionText.text = "Survey results uploaded";

        UpdateQuestionResponseSizes(true);
        FadeInSwipeAnimsNoCount();

        // Allow exit
        TMP_Text nextText = nextButton.transform.GetComponentInChildren<TMP_Text>();
        nextText.text = "Exit";
        nextButton.interactable = true;

        vlatMenuNav.AddNavigatableItem(nextButton.gameObject);
    }

    #endregion


    #region SELECTION / MULTIPLE CHOICE

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
        if (surveyResults[currentQuestionIndex] != null && surveyResults[currentQuestionIndex] != "")
        {
            // Split the string by commas and trim spaces from each part
            string[] parts = surveyResults[currentQuestionIndex].Split(',')
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
        for (i = 0; i < activeSurvey.surveyQuestions[currentQuestionIndex].selectionOptions.Length; i++)
        {
            // Spawn and setup new response option
            SurveySelectionOption newOption = GameObject.Instantiate(optionPrefab, responseContentParent);
            newOption.Setup(this, i, activeSurvey.surveyQuestions[currentQuestionIndex].selectionOptions[i]);
            choiceOptions.Add(newOption);
            vlatMenuNav.AddNavigatableItem(newOption.GetNavigatableItem());

            if (optionsWhichNeedToBeOn.Contains(i))
            {
                newOption.ToggleSelection();
            }
        }
    }

    // Called when an option is clicked; update selected options
    public void OptionWasClicked(SurveySelectionOption option)
    {
        // Multiple choice, only one option can be active at a time
        if (activeSurvey.surveyQuestions[currentQuestionIndex].questionType == SurveyQuestionInfo.SurveyQuestionType.MultipleChoice)
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
        else if (activeSurvey.surveyQuestions[currentQuestionIndex].questionType == SurveyQuestionInfo.SurveyQuestionType.Selection)
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


    #region SLIDER

    // Sets up a slider response question
    private void SetupSlider()
    {
        float savedSliderVal = 0f;
        // If data exists from previous response entry, load slider value
        if (surveyResults[currentQuestionIndex] != null && surveyResults[currentQuestionIndex] != "")
        {
            // Split the string by commas and trim spaces from each part
            string[] parts = surveyResults[currentQuestionIndex].Split(',')
                             .Select(part => part.Trim())
                             .ToArray();

            if (parts.Length > 1 && parts[1] != "N/A")
            {
                bool parseSuccess = float.TryParse(parts[1], out savedSliderVal);
                if (!parseSuccess)
                    Debug.LogWarning("Unable to parse saved slider value; setting value to 0 and continuing.");
            }
        }

        // Spawn and setup new response option
        SurveySliderOption newSlider = GameObject.Instantiate(sliderOptionPrefab, responseContentParent);
        newSlider.SetSliderValue(savedSliderVal);
        newSlider.SetLeftText(activeSurvey.surveyQuestions[currentQuestionIndex].leftSliderText);
        newSlider.SetRightText(activeSurvey.surveyQuestions[currentQuestionIndex].rightSliderText);
        activeSliderOption = newSlider;

        vlatMenuNav.AddNavigatableItem(newSlider.GetNavigatableItem());
    }

    #endregion


    #region MATRIX


    // Sets up a matrix question
    private void SetupMatrix()
    {
        int[] selectedMatrixOptions = null;
        // If data exists from previous response entry, load which options were selected
        if (surveyResults[currentQuestionIndex] != null && surveyResults[currentQuestionIndex] != "")
        {
            // Split the string by commas and trim spaces from each part
            string[] parts = surveyResults[currentQuestionIndex].Split(',')
                             .Select(part => part.Trim())
                             .ToArray();

            if (parts.Length > 1)
            {
                // Convert each value to an integer or -1 if parsing fails
                selectedMatrixOptions = parts.Select(part =>
                {
                    int number;
                    bool success = int.TryParse(part, out number);
                    return success ? number : -1;
                }).Skip(1).ToArray();
            }
        }

        // Spawn overall matrix and setup columns
        matrixOptions = GameObject.Instantiate(matrixOptionsPrefab, responseContentParent);
        matrixOptions.SetupHeader(activeSurvey.surveyQuestions[currentQuestionIndex].matrixColumnTexts);

        // Spawn and setup new response options
        for (int i = 0; i < activeSurvey.surveyQuestions[currentQuestionIndex].matrixRowTexts.Length; i++)
        {
            string rowText = activeSurvey.surveyQuestions[currentQuestionIndex].matrixRowTexts[i];
            int numCols = activeSurvey.surveyQuestions[currentQuestionIndex].matrixColumnTexts.Length;
            int activeIdx = -1;
            if (selectedMatrixOptions != null && selectedMatrixOptions.Length > i)
            {
                activeIdx = selectedMatrixOptions[i];
            }

            matrixOptions.AddNewOptionArea(rowText, numCols, activeIdx);
        }

        List<GameObject> navItems = matrixOptions.GetNavigatableItems();

        foreach (GameObject go in navItems)
        {
            vlatMenuNav.AddNavigatableItem(go);
        }
    }


    #endregion


    #region ACCESSIBILITY

    public void OnHighlightedItemChanged()
    {
        infLoopDetect++;
        if (infLoopDetect > 1000)
        {
            return;
        }

        GameObject highlightedItem = vlatMenuNav.GetHighlightedItem();
        // If highlighted item exists and is within the scroll view...
        if (highlightedItem != null && highlightedItem.transform.IsChildOf(responseContentParent))
        {
            // Get whether the highlighted item is "visible" within the scroll view
            RectTransform viewport = responseScrollRect.viewport;
            RectTransform content = responseScrollRect.content;    // The scrollable content area
            RectTransform targetRect = highlightedItem.GetComponent<RectTransform>();

            // Get the world position of the target's corners
            Vector3[] targetCorners = new Vector3[4];
            targetRect.GetWorldCorners(targetCorners);

            // Get the world position of the viewport's corners
            Vector3[] viewportCorners = new Vector3[4];
            viewport.GetWorldCorners(viewportCorners);

            // Check if any of the target's corners are inside the viewport
            bool isAbove = (targetCorners[0].y) > viewportCorners[1].y;
            bool isBelow = (targetCorners[1].y) < viewportCorners[0].y;

            if (isAbove)
            {
                if (responseContentParent.childCount > 0 && highlightedItem.transform.IsChildOf(responseContentParent.GetChild(0)))
                {
                    responseScrollRect.verticalNormalizedPosition = 1f;
                }
                else
                {
                    responseScrollRect.verticalNormalizedPosition += .2f;
                    OnHighlightedItemChanged();
                }
            }
            else if (isBelow)
            {
                if (responseContentParent.childCount > 0 && highlightedItem.transform.IsChildOf(responseContentParent.GetChild(responseContentParent.childCount - 1)))
                {
                    responseScrollRect.verticalNormalizedPosition = 0f;
                }
                else
                {
                    responseScrollRect.verticalNormalizedPosition -= .2f;
                    OnHighlightedItemChanged();
                }
            }
            else
            {
                infLoopDetect = 0;
            }
        }
    }

    #endregion

}
