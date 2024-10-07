using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{

    // SettingsManager manages the settings


    #region VARIABLES


    [Header("UI Elements")]
    [SerializeField] private Toggle joystickToggle;
    [SerializeField] private Toggle tapActivationToggle;
    [SerializeField] private Toggle disableVirtualHandsToggle;
    [SerializeField] private Slider moveSpeedSlider;
    [SerializeField] private TMP_Text moveSpeedText;
    [SerializeField] private Slider turnAmtSlider;
    [SerializeField] private TMP_Text turnAmtText;
    [SerializeField] private Slider menuTiltSlider;
    [SerializeField] private TMP_Text menuTiltText;
    [SerializeField] private Slider menuHeightSlider;
    [SerializeField] private TMP_Text menuHeightText;
    [SerializeField] private Slider menuOpacitySlider;
    [SerializeField] private TMP_Text menuOpacityText;

    private bool playerHasVirtualHands;
    private GameObject leftVirtualHand, rightVirtualHand;
    private MovementController movementController;
    private float defaultMoveSpeed;
    private float defaultTurnAmt;
    private NewMenuNavigation menuNav;
    private float defaultMenuRotX;
    private float defaultMenuPosY;


    #endregion


    #region SETUP


    // Setup
    //--------------------------------------//
    public void Setup(bool _playerHasVirtualHands, GameObject _leftVirtualHand, GameObject _rightVirtualHand)
    //--------------------------------------//
    {
        movementController = FindObjectOfType<MovementController>();
        defaultMoveSpeed = movementController.speed;
        defaultTurnAmt = movementController.rotationValue;

        menuNav = FindObjectOfType<NewMenuNavigation>();
        defaultMenuRotX = menuNav.transform.localRotation.x;
        defaultMenuPosY = menuNav.transform.localPosition.y;

        playerHasVirtualHands = _playerHasVirtualHands;
        leftVirtualHand = _leftVirtualHand;
        rightVirtualHand = _rightVirtualHand;

    } // END Setup


    #endregion


    #region ON TOGGLE CHANGE


    // Called on change
    //--------------------------------------//
    public void OnJoystickToggleChange()
    //--------------------------------------//
    {
        if (movementController == null)
        {
            movementController = FindObjectOfType<MovementController>();
        }

        if (movementController != null)
        {
            if (joystickToggle != null)
            {
                if (joystickToggle.isOn)
                {
                    movementController.currentLvl = 2;
                }
                else
                {
                    movementController.currentLvl = 1;
                }
            }
        }

    } // END OnChange


    // Called on change
    //--------------------------------------//
    public void OnTapActivationToggleChange()
    //--------------------------------------//
    {
        if (movementController == null)
        {
            movementController = FindObjectOfType<MovementController>();
        }

        if (movementController != null)
        {
            if (tapActivationToggle != null)
            {
                movementController.SetOneTapMove(tapActivationToggle.isOn);
            }
        }

    } // END OnChange


    // Called on change
    //--------------------------------------//
    public void OnDisableVirtualHandsToggleChange()
    //--------------------------------------//
    {
        if (!playerHasVirtualHands) return;

        if (disableVirtualHandsToggle.isOn)
        {
            if (leftVirtualHand != null)
                leftVirtualHand.gameObject.SetActive(false);
            if (rightVirtualHand != null)
                rightVirtualHand.gameObject.SetActive(false);
        }
        else
        {
            if (leftVirtualHand != null)
                leftVirtualHand.gameObject.SetActive(false);
            if (rightVirtualHand != null)
                rightVirtualHand.gameObject.SetActive(false);
        }

    } // END OnChange


    #endregion


    #region MOVE SLIDER ON CHANGE


    // Called on change
    //--------------------------------------//
    public void OnMoveSpeedSliderToggleChange()
    //--------------------------------------//
    {
        float multiplier = 1f;

        switch (moveSpeedSlider.value)
        {
            case 0: multiplier = .1f;
                break;
            case 1: multiplier = .35f;
                break;
            case 2: multiplier = .7f;
                break;
            case 3: multiplier = 1f;
                break;
            case 4: multiplier = 1.5f;
                break;
            case 5: multiplier = 2f;
                break;
            case 6: multiplier = 3f;
                break;
        }

        if (movementController == null)
        {
            movementController = FindObjectOfType<MovementController>();
        }

        if (movementController != null)
        {
            movementController.speed = defaultMoveSpeed * multiplier;
            moveSpeedText.text = "" + Mathf.FloorToInt(multiplier * 100f) + "%";
        }

    } // END OnChange


    // Called on change
    //--------------------------------------//
    public void OnTurnAmtToggleChange()
    //--------------------------------------//
    {
        float multiplier = 1f;

        switch (moveSpeedSlider.value)
        {
            case 0:
                multiplier = .1f;
                break;
            case 1:
                multiplier = .35f;
                break;
            case 2:
                multiplier = .7f;
                break;
            case 3:
                multiplier = 1f;
                break;
            case 4:
                multiplier = 1.5f;
                break;
            case 5:
                multiplier = 2f;
                break;
            case 6:
                multiplier = 3f;
                break;
        }

        if (movementController == null)
        {
            movementController = FindObjectOfType<MovementController>();
        }

        if (movementController != null)
        {
            movementController.rotationValue = defaultTurnAmt * multiplier;
            turnAmtText.text = "" + Mathf.FloorToInt(multiplier * 100f) + "%";
        }

    } // END OnChange


    #endregion


    #region MENU SLIDER ON CHANGE


    // Called on change
    //--------------------------------------//
    public void OnMenuTiltToggleChange()
    //--------------------------------------//
    {
        float turnAmt = 30f;

        switch (menuTiltSlider.value)
        {
            case 0:
                turnAmt = 0f;
                break;
            case 1:
                turnAmt = 10f;
                break;
            case 2:
                turnAmt = 20f;
                break;
            case 3:
                turnAmt = 30f;
                break;
            case 4:
                turnAmt = 50f;
                break;
            case 5:
                turnAmt = 60f;
                break;
            case 6:
                turnAmt = 70f;
                break;
        }

        if (menuNav == null)
        {
            menuNav = FindObjectOfType<NewMenuNavigation>();
        }

        if (menuNav != null)
        {
            menuNav.RotTo(turnAmt);
            menuTiltText.text = "" + Mathf.FloorToInt(turnAmt) + "°";
        }

    } // END OnChange


    // Called on change
    //--------------------------------------//
    public void OnMenuHeightToggleChange()
    //--------------------------------------//
    {
        float multiplier = 1f;

        switch (menuHeightSlider.value)
        {
            case 0:
                multiplier = 0f;
                break;
            case 1:
                multiplier = .35f;
                break;
            case 2:
                multiplier = .7f;
                break;
            case 3:
                multiplier = 1f;
                break;
            case 4:
                multiplier = 1.3f;
                break;
            case 5:
                multiplier = 1.65f;
                break;
            case 6:
                multiplier = 2f;
                break;
        }

        if (menuNav == null)
        {
            menuNav = FindObjectOfType<NewMenuNavigation>();
        }

        if (menuNav != null)
        {
            menuNav.HeightTo(multiplier);
            menuHeightText.text = "" + Mathf.FloorToInt(multiplier * 100f) + "%";
        }

    } // END OnChange


    // Called on change
    //--------------------------------------//
    public void OnMenuOpacityToggleChange()
    //--------------------------------------//
    {
        float op = GetMenuOpacity();

        if (menuNav == null)
        {
            menuNav = FindObjectOfType<NewMenuNavigation>();
        }

        if (menuNav != null)
        {
            menuNav.GetComponent<CanvasGroup>().alpha = op;
            menuOpacityText.text = "" + Mathf.FloorToInt(op * 100f) + "%";
        }

    } // END OnChange


    // Gets menu opacity
    //--------------------------------------//
    public float GetMenuOpacity()
    //--------------------------------------//
    {
        float op = 1f;

        switch (menuOpacitySlider.value)
        {
            case 0:
                op = .4f;
                break;
            case 1:
                op = .5f;
                break;
            case 2:
                op = .6f;
                break;
            case 3:
                op = .7f;
                break;
            case 4:
                op = .8f;
                break;
            case 5:
                op = .9f;
                break;
            case 6:
                op = 1f;
                break;
        }

        return op;

    } // END GetMenuOpacity


    #endregion


} // END SettingsManager.cs
