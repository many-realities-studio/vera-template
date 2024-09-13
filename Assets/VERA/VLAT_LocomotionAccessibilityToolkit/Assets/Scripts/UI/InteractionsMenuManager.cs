using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionsMenuManager : MonoBehaviour
{

    // InteractionsMenuManager manages the interaction menu in the main UI


    #region VARIABLES


    private SelectionController selectionController;
    [SerializeField] private TMP_Text beginInteractionsText;
    [SerializeField] private GameObject interactSubRow1;
    [SerializeField] private GameObject interactSubRow2;
    [SerializeField] private HighlightableTab interactSubButtonPrefab;
    private int numButtons = 0;


    #endregion


    #region MONOBEHAVIOUR


    // Start
    //--------------------------------------//
    private void Start()
    //--------------------------------------//
    {
        selectionController = FindObjectOfType<SelectionController>();
        beginInteractionsText.text = "No interactable currently selected.";

    } // END Start


    #endregion


    #region INTERACTABLES


    // Called when "highlight all" has been chosen in interact menu
    //--------------------------------------//
    public void HighlightedAll()
    //--------------------------------------//
    {
        beginInteractionsText.text = "No interactable currently selected.";

    } // END HighlightedAll


    // Called when "select next" has been chosen in interact menu
    //--------------------------------------//
    public void SelectedNext()
    //--------------------------------------//
    {
        if (selectionController.currentObj != null)
        {
            beginInteractionsText.text = "View interactions for:\n" + selectionController.currentObj.interactableName;
        }
        else
        {
            ResetText();
        }

    } // END SelectedNext


    // Resets the text to blank
    //--------------------------------------//
    public void ResetText()
    //--------------------------------------//
    {
        beginInteractionsText.text = "No interactable currently selected.";

    } // END ResetText


    #endregion


    #region INTERACT SUB


    // Returns whether we can currently view interactions
    //--------------------------------------//
    public bool CanViewInteractions()
    //--------------------------------------//
    {
        if (selectionController.currentObj != null)
        {
            return true;
        }
        else
        {
            return false;
        }

    } // END CanViewInteractions


    // Sets up the interactable sub menu
    //--------------------------------------//
    public void SetupInteractableSub(ButtonTabberManager interactableTabberManager)
    //--------------------------------------//
    {
        if (numButtons != 0)
        {
            DestroyInteractableSub(interactableTabberManager);
        }

        foreach (string interaction in selectionController.currentObj.GetInteractions())
        {
            numButtons++;

            if (numButtons <= 4)
            {
                HighlightableTab newTab = GameObject.Instantiate(interactSubButtonPrefab, interactSubRow1.transform);
                newTab.buttonText.text = interaction;
                interactableTabberManager.AddTab(newTab);
            }
            else if (numButtons <= 8)
            {
                HighlightableTab newTab = GameObject.Instantiate(interactSubButtonPrefab, interactSubRow2.transform);
                newTab.buttonText.text = interaction;
                interactableTabberManager.AddTab(newTab);
            }
            else
            {
                Debug.LogError("Too many interactions on interactable " + selectionController.currentObj.interactableName);
                return;
            }
            
        }

    } // END SetupInteractableSub


    // Destroys the interactable sub menu
    //--------------------------------------//
    public void DestroyInteractableSub(ButtonTabberManager interactableTabberManager)
    //--------------------------------------//
    {
        numButtons = 0;
        interactableTabberManager.ClearTabs();

    } // END DestroyInteractableSub


    // Triggers interaction at given index
    //--------------------------------------//
    public void TriggerSubInteraction(ButtonTabberManager interactableTabberManager, int interactionIndex)
    //--------------------------------------//
    {
        string targetInteraction = interactableTabberManager.GetTab(interactionIndex).buttonText.text;
        selectionController.currentObj.TriggerInteraction(targetInteraction);

    } // END TriggerSubInteraction


    #endregion


} // END InteractionsMenuManager.cs
