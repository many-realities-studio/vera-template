using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SurveyManager))]
public class SurveyInterfaceIO : MonoBehaviour
{


    #region VARIABLES

    [SerializeField] private SurveyInfo demoSurveyInfo;
    private SurveyManager surveyManager;

    #endregion


    #region MONOBEHAVIOUR

    // Start
    private void Start()
    {
        surveyManager = GetComponent<SurveyManager>();
        surveyManager.Setup();

        // NOTE: Line below, along with associated demoSurveyInfo variable, can be deleted; present for testing purposes.
        StartSurvey(demoSurveyInfo);
    }

    #endregion


    #region INPUT

    // Converts a given survey in database JSON format into SurveyInfo
    public SurveyInfo ConvertSurveyJsonToSurveyInfo(string surveyJson)
    {
        SurveyInfo surveyInfo = ScriptableObject.CreateInstance<SurveyInfo>();

        // TODO: Build the surveyInfo question by question according to data in surveyJson

        return surveyInfo;
    }

    // Begins a survey outlined by given survey info
    public void StartSurvey(SurveyInfo surveyToStart)
    {
        surveyManager.BeginSurvey(surveyToStart);
    }

    #endregion


    #region OUTPUT

    // Outputs given results of given survey back to the database
    public IEnumerator OutputSurveyResults(SurveyInfo surveyToOutput, string[] surveyResults)
    {
        // TODO: Functionality for outputting survey back to database
        // Note: surveyToOutput is the survey itself (e.g., survey name, questions, etc.), while surveyResults are the results.

        // Results are an array of strings, one string per question, formatted as "[question type], [question results]".
        // Examples of the result formatting are given below:
        //     Multiple choice, where the user selected the first option: "MultipleChoice, 0"
        //     Selection, where the user selected the first, third, and eighth option: "Selection, 0, 2, 7"
        //     Slider, where the user selected a value of 0.728: "Slider, 0.728"
        //     Matrix with 3 questions, each with 5 options, where the user selected the third, then first, then last option, respectively: "Matrix, 2, 0, 4"
        // If the user did not give a response to a question, the response will be filled out with "N/A" instead of a result.
        // For example, a multiple choice selection which was left blank would be "MultipleChoice: N/A"

        // This coroutine is called from SurveyManager.cs, upon users beginning the survey upload process.
        // While the coroutine is running, users will be asked to wait for upload to complete.
        // Once coroutine is completed, SurveyManager.cs will notify users the upload was complete, so they can safely exit.

        // For testing purposes...
        //foreach (string s in surveyResults)
        //    Debug.Log(s);

        yield break;
    }

    #endregion

}
