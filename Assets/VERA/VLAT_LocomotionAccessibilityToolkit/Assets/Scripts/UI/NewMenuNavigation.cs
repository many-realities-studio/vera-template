using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewMenuNavigation : MonoBehaviour
{

    // NewMenuNavigation handles navigation of the new UI menu


    #region VARIABLES


    public enum UiState
    {
        Header,
        Look,
        Move,
        Interact,
        InteractSub,
        Settings
    }

    private UiState pendingStateAfterHeader = UiState.Look;
    public UiState currentUiState { get; private set; } = UiState.Header;
    private int currentButtonHighlight = 0;

    private FolderTabsManager folderTabsManager;
    private MainAreaManager mainAreaManager;

    [SerializeField] private ButtonTabberManager lookTabberManager;
    [SerializeField] private ButtonTabberManager moveTabberManager;
    [SerializeField] private ButtonTabberManager interactTabberManager;
    [SerializeField] private ButtonTabberManager interactSubTabberManager;
    [SerializeField] private VLAT_MenuNavigator settingsNavigator;
    [SerializeField] private CanvasGroup settingsCanvasGroup;
    private CanvasGroup menuParentCanvasGroup;

    private UiInputDistributor uiInputDistributor;
    private SelectionController selectionController;
    private InteractionsMenuManager interactionsMenuManager;
    private SettingsManager settingsManager;

    public HighlightableTab toggleHighlightPrefab;
    public HighlightableTab sliderHighlightPrefab;
    public HighlightableTab buttonHighlightPrefab;

    private bool navigatingExternalMenu = false;
    private VLAT_MenuNavigator externalMenuNavigator;
    private float settingsStartPosY;
    private float settingsOldSize = 100f;
    private float settingsNewSize = 300f;
    private float currentTurnAmt = 0f;
    public float defaultLocalY;


    #endregion


    #region SETUP


    // Setup
    //--------------------------------------//
    public void Setup()
    //--------------------------------------//
    {
        folderTabsManager = FindObjectOfType<FolderTabsManager>();
        folderTabsManager.BeginTabbing();
        folderTabsManager.InitialStart();
        mainAreaManager = FindObjectOfType<MainAreaManager>();
        mainAreaManager.SwapMainAreaToMode(UiState.Look);
        menuParentCanvasGroup = GetComponent<CanvasGroup>();
        menuParentCanvasGroup.alpha = 1f;

        uiInputDistributor = FindObjectOfType<UiInputDistributor>();
        selectionController = FindObjectOfType<SelectionController>();
        interactionsMenuManager = FindObjectOfType<InteractionsMenuManager>();
        settingsManager = FindObjectOfType<SettingsManager>();

        settingsStartPosY = transform.localPosition.y;
        defaultLocalY = transform.GetChild(0).localPosition.y;

    } // END Setup


    #endregion


    #region BUTTONS


    // Button 1 (left)
    //--------------------------------------//
    public void Button1(InputAction.CallbackContext ctx)
    //--------------------------------------//
    {
        if (navigatingExternalMenu)
        {
            externalMenuNavigator.TabPrevious();
            return;
        }

        switch(currentUiState)
        {
            // If on header, tab one folder to the left
            case UiState.Header:
                switch (folderTabsManager.TabLeft())
                {
                    case 0: 
                        pendingStateAfterHeader = UiState.Look;
                        break;
                    case 1: 
                        pendingStateAfterHeader = UiState.Move; 
                        break;
                    case 2: 
                        pendingStateAfterHeader = UiState.Interact; 
                        break;
                    case 3: 
                        pendingStateAfterHeader = UiState.Settings; 
                        break;
                }
                mainAreaManager.SwapMainAreaToMode(pendingStateAfterHeader);
                break;

            case UiState.Look:
                currentButtonHighlight = lookTabberManager.TabLeft();
                break;

            case UiState.Move:
                currentButtonHighlight = moveTabberManager.TabLeft();
                break;

            case UiState.Interact:
                currentButtonHighlight = interactTabberManager.TabLeft();
                break;

            case UiState.InteractSub:
                currentButtonHighlight = interactSubTabberManager.TabLeft();
                break;

            case UiState.Settings:
                // N/A, handled externally
                break;
        }

    } // END Button1


    // Button 2 (right)
    //--------------------------------------//
    public void Button2(InputAction.CallbackContext ctx)
    //--------------------------------------//
    {
        if (navigatingExternalMenu)
        {
            externalMenuNavigator.TabNext();
            return;
        }

        switch (currentUiState)
        {
            // If on header, tab one folder to the right
            case UiState.Header:
                switch (folderTabsManager.TabRight())
                {
                    case 0:
                        pendingStateAfterHeader = UiState.Look;
                        break;
                    case 1:
                        pendingStateAfterHeader = UiState.Move;
                        break;
                    case 2:
                        pendingStateAfterHeader = UiState.Interact;
                        break;
                    case 3:
                        pendingStateAfterHeader = UiState.Settings;
                        break;
                }
                mainAreaManager.SwapMainAreaToMode(pendingStateAfterHeader);
                break;

            case UiState.Look:
                currentButtonHighlight = lookTabberManager.TabRight();
                break;

            case UiState.Move:
                currentButtonHighlight = moveTabberManager.TabRight();
                break;

            case UiState.Interact:
                currentButtonHighlight = interactTabberManager.TabRight();
                break;

            case UiState.InteractSub:
                currentButtonHighlight = interactSubTabberManager.TabRight();
                break;

            case UiState.Settings:
                // N/A, handled externally
                break;
        }

    } // END Button2


    // Button 3 (select)
    //--------------------------------------//
    public void Button3(InputAction.CallbackContext ctx)
    //--------------------------------------//
    {
        if (navigatingExternalMenu)
        {
            externalMenuNavigator.SelectItem();
            return;
        }

        switch (currentUiState)
        {
            // If on header, begin controlling main area
            case UiState.Header:
                folderTabsManager.EndTabbing();
                switch(pendingStateAfterHeader)
                {
                    case UiState.Look:
                        lookTabberManager.ResetTabbing();
                        break;
                    case UiState.Move:
                        moveTabberManager.ResetTabbing();
                        break;
                    case UiState.Interact:
                        interactTabberManager.ResetTabbing();
                        break;
                    case UiState.Settings:
                        OpenSettings();
                        break;
                }
                currentUiState = pendingStateAfterHeader;
                currentButtonHighlight = 0;
                break;

            // Activate corresponding look function
            case UiState.Look:
                switch (currentButtonHighlight)
                {
                    case 0:
                        uiInputDistributor.LookUp();
                        break;
                    case 1:
                        uiInputDistributor.LookDown();
                        break;
                    case 2:
                        uiInputDistributor.LookReset();
                        break;
                }
                break;

            // Activate corresponding move function
            case UiState.Move:
                switch (currentButtonHighlight)
                {
                    case 0:
                        uiInputDistributor.MoveForward();
                        break;
                    case 1:
                        uiInputDistributor.TurnLeft();
                        break;
                    case 2:
                        uiInputDistributor.TurnRight();
                        break;
                }
                break;

            // Activate corresponding interact function
            case UiState.Interact:
                switch (currentButtonHighlight)
                {
                    case 0:
                        uiInputDistributor.HighlightAll();
                        interactionsMenuManager.HighlightedAll();
                        break;
                    case 1:
                        uiInputDistributor.SelectNext();
                        interactionsMenuManager.SelectedNext();
                        break;
                    case 2:
                        if (interactionsMenuManager.CanViewInteractions())
                        {
                            currentButtonHighlight = 0;
                            interactTabberManager.EndTabbing();
                            interactionsMenuManager.SetupInteractableSub(interactSubTabberManager);
                            interactSubTabberManager.BeginTabbing();
                            currentUiState = UiState.InteractSub;
                        }
                        break;
                }
                break;

            case UiState.InteractSub:
                interactionsMenuManager.TriggerSubInteraction(interactSubTabberManager, currentButtonHighlight);
                break;

            case UiState.Settings:
                // N/A, handled externally
                break;
        }

    } // END Button3


    // Button 4 (back)
    //--------------------------------------//
    public void Button4(InputAction.CallbackContext ctx)
    //--------------------------------------//
    {
        if (navigatingExternalMenu)
        {
            externalMenuNavigator.BackButton();
            return;
        }

        switch (currentUiState)
        {
            case UiState.Header:
                // At head of UI system, no "back" available
                break;

            case UiState.Look:
                lookTabberManager.EndTabbing();
                folderTabsManager.BeginTabbing();
                currentUiState = UiState.Header;
                break;

            case UiState.Move:
                moveTabberManager.EndTabbing();
                folderTabsManager.BeginTabbing();
                currentUiState = UiState.Header;
                break;

            case UiState.Interact:
                interactTabberManager.EndTabbing();
                folderTabsManager.BeginTabbing();
                currentUiState = UiState.Header;
                break;

            case UiState.InteractSub:
                interactSubTabberManager.EndTabbing();
                interactionsMenuManager.DestroyInteractableSub(interactSubTabberManager);
                interactTabberManager.ResetTabbing();
                currentButtonHighlight = 0;
                interactTabberManager.BeginTabbing();
                currentUiState = UiState.Interact;
                break;

            case UiState.Settings:
                // N/A, handled externally                
                break;
        }

    } // END Button4


    #endregion


    #region EXTERNAL MENU


    // Sets whether we are navigating an external menu
    //--------------------------------------//
    public void StartNavigateExternalMenu(VLAT_MenuNavigator menuToNavigate, bool hideMenu)
    //--------------------------------------//
    {
        navigatingExternalMenu = true;
        externalMenuNavigator = menuToNavigate;
        if (hideMenu)
            HideVlatMenu();

    } // END SetNavigatingExternalMenu


    // Stops navigating an external menu
    //--------------------------------------//
    public void StopNavigateExternalMenu()
    //--------------------------------------//
    {
        navigatingExternalMenu = false;

        if (currentUiState == UiState.Settings)
        {
            CloseSettings();
        }

        ShowVlatMenu();

    } // END StopNavigateExternalMenu


    // Hides the VLAT menu
    //--------------------------------------//
    public void HideVlatMenu()
    //--------------------------------------//
    {
        menuParentCanvasGroup.alpha = 0f;

    } // END HideVlatMenu


    // Shows the VLAT menu
    //--------------------------------------//
    public void ShowVlatMenu()
    //--------------------------------------//
    {
        menuParentCanvasGroup.alpha = settingsManager.GetMenuOpacity();

    } // END ShowVlatMenu


    #endregion


    #region SETTINGS


    // Opens settings menu
    //--------------------------------------//
    private void OpenSettings()
    //--------------------------------------//
    {
        settingsCanvasGroup.LeanAlpha(1f, .25f);

        transform.LeanMoveLocalY(settingsStartPosY + (settingsNewSize - settingsStartPosY) / 2f, .25f).setEaseOutQuad();
        
        RectTransform rectTrans = mainAreaManager.GetComponent<RectTransform>();
        LeanTween.value(mainAreaManager.gameObject, rectTrans.sizeDelta.y, settingsNewSize, .25f).setEaseOutQuad().setOnUpdate((value) =>
        {
            rectTrans.sizeDelta = new Vector2(rectTrans.sizeDelta.x, value);
        });

        mainAreaManager.HideCanvGroups();

        settingsNavigator.StartMenuNavigation(true);        

    } // END OpenSettings


    // Closes settings
    //--------------------------------------//
    private void CloseSettings()
    //--------------------------------------//
    {
        settingsCanvasGroup.LeanAlpha(0f, .25f);

        transform.LeanMoveLocalY(settingsStartPosY, .25f).setEaseOutQuad();

        RectTransform rectTrans = mainAreaManager.GetComponent<RectTransform>();
        LeanTween.value(mainAreaManager.gameObject, rectTrans.sizeDelta.y, settingsOldSize, .25f).setEaseOutQuad().setOnUpdate((value) =>
        {
            rectTrans.sizeDelta = new Vector2(rectTrans.sizeDelta.x, value);
        });

        mainAreaManager.SwapCanvasGroups(3);
        folderTabsManager.BeginTabbing();
        currentUiState = UiState.Header;

        settingsNavigator.StopMenuNavigation();

    } // END CloseSettings


    // Rotates menu to specified amt
    //--------------------------------------//
    public void RotTo(float turnAmt)
    //--------------------------------------//
    {
        currentTurnAmt = turnAmt;
        transform.localRotation = Quaternion.Euler(turnAmt, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
    
    } // END RotTo


    // Changes height of menu to given percent val
    //--------------------------------------//
    public void HeightTo(float heightMultiplier)
    //--------------------------------------//
    {
        heightMultiplier -= 1f;
        transform.GetChild(0).localPosition = new Vector3(transform.localPosition.x, defaultLocalY + 50f * heightMultiplier, transform.localPosition.z);

    } // END HeightTo


    #endregion


    #region OTHER


    // Called when interact is out of range
    //--------------------------------------//
    public void InteractOutOfRange()
    //--------------------------------------//
    {
        if (currentUiState == UiState.InteractSub)
        {
            interactSubTabberManager.EndTabbing();
            interactionsMenuManager.DestroyInteractableSub(interactSubTabberManager);
            interactionsMenuManager.ResetText();
            interactTabberManager.BeginTabbing();
            currentUiState = UiState.Interact;
        }
        else
        {
            interactionsMenuManager.ResetText();
        }

    } // END InteractOutOfRange


    #endregion


} // END NewMenuNavigation.cs
