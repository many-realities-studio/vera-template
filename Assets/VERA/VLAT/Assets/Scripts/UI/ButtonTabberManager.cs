using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTabberManager : MonoBehaviour
{

    // FolderTabsManager manages the folder tabs section of the UI


    #region VARIABLES


    [SerializeField] private List<HighlightableTab> highlightableTabs = new List<HighlightableTab>();
    public static float tabMoveTime = .25f;
    private int currentActiveTab;


    #endregion


    #region CONTROL


    // Begins the tabbing process between folder tabs
    //--------------------------------------//
    public void BeginTabbing()
    //--------------------------------------//
    {
        highlightableTabs[currentActiveTab].highlightCanvGroup.alpha = .0f;
        highlightableTabs[currentActiveTab].highlightCanvGroup.LeanAlpha(1f, tabMoveTime).setLoopPingPong();

    } // END BeginTabbing


    // Stops the tabbing process between folder tabs
    //--------------------------------------//
    public void EndTabbing()
    //--------------------------------------//
    {
        LeanTween.cancel(highlightableTabs[currentActiveTab].highlightCanvGroup.gameObject);
        highlightableTabs[currentActiveTab].highlightCanvGroup.LeanAlpha(0f, tabMoveTime);

    } // END EndTabbing


    // Resets tabbing process
    //--------------------------------------//
    public void ResetTabbing()
    //--------------------------------------//
    {
        LeanTween.cancel(highlightableTabs[currentActiveTab].highlightCanvGroup.gameObject);
        highlightableTabs[currentActiveTab].highlightCanvGroup.alpha = 0f;
        currentActiveTab = 0;
        BeginTabbing();

    } // END ResetTabbing


    // Tabs the folders one to the left, returns the new active tab
    //--------------------------------------//
    public int TabLeft()
    //--------------------------------------//
    {
        EndTabbing();

        currentActiveTab--;
        if (currentActiveTab < 0)
        {
            currentActiveTab = highlightableTabs.Count - 1;
        }

        BeginTabbing();

        return currentActiveTab;

    } // END TabLeft


    // Tabs the folders one to the left, returns the new active tab
    //--------------------------------------//
    public int TabRight()
    //--------------------------------------//
    {
        EndTabbing();

        currentActiveTab++;
        if (currentActiveTab > highlightableTabs.Count - 1)
        {
            currentActiveTab = 0;
        }

        BeginTabbing();

        return currentActiveTab;

    } // END TabRight


    #endregion


    #region CLEARING AND ADDING TABS


    // Clears the list of tabs
    //--------------------------------------//
    public void ClearTabs()
    //--------------------------------------//
    {
        for (int i = 0; i < highlightableTabs.Count; i++)
        {
            Destroy(highlightableTabs[i].gameObject);
        }

        highlightableTabs.Clear();
        currentActiveTab = 0;

    } // END ClearTabs


    // Adds to the list of tabs
    //--------------------------------------//
    public void AddTab(HighlightableTab tab)
    //--------------------------------------//
    {
        highlightableTabs.Add(tab);

    } // END AddTab


    // Returns tab at index
    //--------------------------------------//
    public HighlightableTab GetTab(int index)
    //--------------------------------------//
    {
        return highlightableTabs[index];

    } // END GetTab


    #endregion


} // END ButtonTabberManager.cs
