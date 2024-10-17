using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FolderTabsManager : MonoBehaviour
{

    // FolderTabsManager manages the folder tabs section of the UI


    #region VARIABLES


    [SerializeField] private HighlightableTab[] folderTabs = new HighlightableTab[4];
    [SerializeField] private int hidHeight;
    [SerializeField] private int activeHeight;
    public static float tabMoveTime = .25f;
    private int currentActiveTab;


    #endregion


    #region CONTROL


    // Initially sets look active
    //--------------------------------------//
    public void InitialStart()
    //--------------------------------------//
    {
        folderTabs[0].rectTrans.LeanMoveLocalY(activeHeight, tabMoveTime).setEaseOutQuad();

    } // END InitialStart


    // Begins the tabbing process between folder tabs
    //--------------------------------------//
    public void BeginTabbing()
    //--------------------------------------//
    {
        folderTabs[currentActiveTab].highlightCanvGroup.alpha = .0f;
        folderTabs[currentActiveTab].highlightCanvGroup.LeanAlpha(1f, tabMoveTime).setLoopPingPong();

    } // END BeginTabbing


    // Stops the tabbing process between folder tabs
    //--------------------------------------//
    public void EndTabbing()
    //--------------------------------------//
    {
        LeanTween.cancel(folderTabs[currentActiveTab].highlightCanvGroup.gameObject);
        folderTabs[currentActiveTab].highlightCanvGroup.LeanAlpha(0f, tabMoveTime);

    } // END EndTabbing


    // Tabs the folders one to the left, returns the new active tab
    //--------------------------------------//
    public int TabLeft()
    //--------------------------------------//
    {
        folderTabs[currentActiveTab].rectTrans.LeanMoveLocalY(hidHeight, tabMoveTime).setEaseOutQuad();
        LeanTween.cancel(folderTabs[currentActiveTab].highlightCanvGroup.gameObject);
        folderTabs[currentActiveTab].highlightCanvGroup.LeanAlpha(0f, tabMoveTime);

        currentActiveTab--;
        if (currentActiveTab < 0)
        {
            currentActiveTab = 3;
        }

        folderTabs[currentActiveTab].rectTrans.LeanMoveLocalY(activeHeight, tabMoveTime).setEaseOutQuad();
        BeginTabbing();

        return currentActiveTab;

    } // END TabLeft


    // Tabs the folders one to the left, returns the new active tab
    //--------------------------------------//
    public int TabRight()
    //--------------------------------------//
    {
        folderTabs[currentActiveTab].rectTrans.LeanMoveLocalY(hidHeight, tabMoveTime).setEaseOutQuad();
        LeanTween.cancel(folderTabs[currentActiveTab].highlightCanvGroup.gameObject);
        folderTabs[currentActiveTab].highlightCanvGroup.LeanAlpha(0f, tabMoveTime);

        currentActiveTab++;
        if (currentActiveTab > 3)
        {
            currentActiveTab = 0;
        }

        folderTabs[currentActiveTab].rectTrans.LeanMoveLocalY(activeHeight, tabMoveTime).setEaseOutQuad();
        BeginTabbing();

        return currentActiveTab;

    } // END TabRight


    #endregion


} // END FolderTabsManager.cs
