using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatrixOptions : MonoBehaviour
{

    // MatrixOptions handles the overall area of a matrix response question


    #region VARIABLES

    [SerializeField] private Transform headerAreaParent;
    [SerializeField] private TMP_Text columnHeaderTextPrefab;
    [SerializeField] private Transform optionsAreaParent;
    [SerializeField] private MatrixOptionArea matrixOptionAreaPrefab;
    private List<MatrixOptionArea> matrixOptionAreas = new List<MatrixOptionArea>();

    #endregion


    #region SETUP

    // Sets up the header and corresponding row headings
    public void SetupHeader(string[] columnStrings)
    {
        for (int i = 0; i < columnStrings.Length; i++)
        {
            TMP_Text newText = GameObject.Instantiate(columnHeaderTextPrefab, headerAreaParent);
            newText.text = columnStrings[i];
            LayoutRebuilder.ForceRebuildLayoutImmediate(newText.rectTransform);
        }
    }

    // Adds a new option area to the question
    public void AddNewOptionArea(string text, int numToggles, int activeToggleIndex)
    {
        MatrixOptionArea newOptionArea = GameObject.Instantiate(matrixOptionAreaPrefab, optionsAreaParent);
        newOptionArea.Setup(text, numToggles, activeToggleIndex);
        newOptionArea.ResetFitter();
        matrixOptionAreas.Add(newOptionArea);
    }

    // Gets a list of navigatable items in this matrix, for use in VLAT
    public List<GameObject> GetNavigatableItems()
    {
        List<GameObject> ret = new List<GameObject>();

        foreach (MatrixOptionArea optionArea in matrixOptionAreas)
        {
            List<GameObject> optionObjs = optionArea.GetNavigatableItems();
            foreach (GameObject go in optionObjs)
            {
                ret.Add(go);
            }
        }

        return ret;
    }

    #endregion


    #region FINISH

    // Gets all active indexes of all response areas
    public List<int> GetActiveIndexes()
    {
        List<int> ret = new List<int>();
        foreach (MatrixOptionArea area in matrixOptionAreas)
        {
            ret.Add(area.GetActiveToggleIndex());
        }
        return ret;
    }

    #endregion


}
