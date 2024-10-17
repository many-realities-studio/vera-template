using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainAreaManager : MonoBehaviour
{

    // MainAreaManager manages the main area of the new UI


    #region VARIABLES


    [SerializeField] private Image bgImg;
    [SerializeField] private CanvasGroup[] areaCanvasGroups;


    #endregion


    #region SWAP MAIN AREA


    // Swaps the main area to a certain UI mode
    //--------------------------------------//
    public void SwapMainAreaToMode(NewMenuNavigation.UiState uiState)
    //--------------------------------------//
    {
        switch(uiState)
        {
            case NewMenuNavigation.UiState.Look:
                SwapCanvasGroups(0);
                bgImg.CrossFadeColor(GlobalColorManager.Instance.lookColorLight, FolderTabsManager.tabMoveTime, false, false);
                break;
            case NewMenuNavigation.UiState.Move:
                SwapCanvasGroups(1);
                bgImg.CrossFadeColor(GlobalColorManager.Instance.moveColorLight, FolderTabsManager.tabMoveTime, false, false);
                break;
            case NewMenuNavigation.UiState.Interact:
                SwapCanvasGroups(2);
                bgImg.CrossFadeColor(GlobalColorManager.Instance.interactColorLight, FolderTabsManager.tabMoveTime, false, false);
                break;
            case NewMenuNavigation.UiState.Settings:
                SwapCanvasGroups(3);
                bgImg.CrossFadeColor(GlobalColorManager.Instance.settingsColorLight, FolderTabsManager.tabMoveTime, false, false);
                break;
        }

    } // END SwapMainAreaToMode


    // Fades out old canvases in leu of new canvas
    //--------------------------------------//
    public void SwapCanvasGroups(int newCanvGroup)
    //--------------------------------------//
    {
        for (int i = 0; i < areaCanvasGroups.Length; i++)
        {
            if (i == newCanvGroup)
            {
                areaCanvasGroups[i].LeanAlpha(1f, FolderTabsManager.tabMoveTime);
            }
            else
            {
                areaCanvasGroups[i].LeanAlpha(0f, FolderTabsManager.tabMoveTime);
            }
        }

    } // END SwapCanvasGroups


    // Hides canvas groups
    //--------------------------------------//
    public void HideCanvGroups()
    //--------------------------------------//
    {
        for (int i = 0; i < areaCanvasGroups.Length; i++)
        {
            areaCanvasGroups[i].LeanAlpha(0f, FolderTabsManager.tabMoveTime);
        }

    } // END HideCanvGroups


    #endregion


} // END MainAreaManager.cs
