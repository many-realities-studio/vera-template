using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatrixOptionArea : MonoBehaviour
{

    // MatrixOptionArea handles a single option within a matrix response


    #region VARIABLES

    [SerializeField] private TMP_Text leftText;
    [SerializeField] private Transform rightAreaParent;
    [SerializeField] private Toggle matrixTogglePrefab;
    [SerializeField] private RectTransform textLayoutSizeFitter;
    private Toggle[] toggles;
    private int activeToggleIndex = -1;

    #endregion


    #region SETUP

    // Sets up the area with relevant info
    public void Setup(string text, int numToggles, int _activeToggleIndex)
    {
        leftText.text = text;

        toggles = new Toggle[numToggles];
        for (int i = 0; i < numToggles; i++)
        {
            toggles[i] = GameObject.Instantiate(matrixTogglePrefab, rightAreaParent);
            int idx = i;
            toggles[i].onValueChanged.AddListener((isOn) => OnToggleChange(idx));
        }

        if (_activeToggleIndex >= 0 && _activeToggleIndex < toggles.Length)
        {
            activeToggleIndex = _activeToggleIndex;
            toggles[activeToggleIndex].isOn = true;
        }
    }

    // Returns a list of the navigatable items in this option, for use in VLAT
    public List<GameObject> GetNavigatableItems()
    {
        List<GameObject> ret = new List<GameObject>();
        foreach (Toggle t in toggles)
        {
            ret.Add(t.gameObject);
        }
        return ret;
    }

    #endregion


    #region TOGGLE

    // Called when a toggle value changes; ensure single-choice behavior, and update current choice
    public void OnToggleChange(int toggleIndex)
    {
        // If the currently active toggle was changed...
        if (activeToggleIndex == toggleIndex)
        {
            // If turned off, set active toggle to nothing
            if (!toggles[toggleIndex].isOn)
            {
                activeToggleIndex = -1;
            }
        }
        // A toggle which was not the active toggle was changed...
        else
        {
            // If new toggle turned on, deactivate old active toggle and set to new one
            if (toggles[toggleIndex].isOn)
            {
                int oldToggleIndex = activeToggleIndex;
                activeToggleIndex = toggleIndex;
                if (oldToggleIndex >= 0)
                    toggles[oldToggleIndex].isOn = false;
            }
        }
    }

    #endregion


    #region ACTIVE VALUE

    // Returns the active toggle index
    public int GetActiveToggleIndex()
    {
        return activeToggleIndex;
    }

    #endregion


    #region LAYOUT

    // Resets the content fitter size of the text
    public void ResetFitter()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(textLayoutSizeFitter);
    }

    #endregion


}
