using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoSurveyInitializer : MonoBehaviour
{
    [SerializeField] private DemoSurveyManager surveyManager;
    [SerializeField] private SurveyInfo surveyInfo;
    [SerializeField] private bool beginSurveyOnStart = false;

    void Start()
    {
        if (beginSurveyOnStart)
            StartCoroutine(BeginSurveyCoroutine());
    }

    private IEnumerator BeginSurveyCoroutine()
    {
        yield return new WaitForSeconds(2f);
        StartSurvey();
    }

    public void StartSurvey()
    {
        surveyManager.BeginSurvey(surveyInfo);
    }
}
