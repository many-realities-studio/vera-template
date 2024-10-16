using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VLAT_InputManager : MonoBehaviour
{

    // VLAT_InputManager handles all inputs, and distributing them to desired sources


    #region VARIABLES


    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private NewMenuNavigation menuNavigation;

    private InputAction button1Action;
    private InputAction button2Action;
    private InputAction button3Action;
    private InputAction button4Action;
    private InputAction joystickAction;


    #endregion


    #region ENABLE / DISABLE


    // OnEnable, activate inputs
    //--------------------------------------//
    private void OnEnable()
    //--------------------------------------//
    {
        inputActionAsset.Enable();

        button1Action = inputActionAsset.FindAction("Switch1");
        button1Action.performed += OnButton1;

        button2Action = inputActionAsset.FindAction("Switch2");
        button2Action.performed += OnButton2;

        button3Action = inputActionAsset.FindAction("Switch3");
        button3Action.performed += OnButton3;

        button4Action = inputActionAsset.FindAction("Switch4");
        button4Action.performed += OnButton4;

    } // END OnEnable


    // OnDisable, deactivate inputs
    //--------------------------------------//
    private void OnDisable()
    //--------------------------------------//
    {
        button1Action.performed -= OnButton1;
        button2Action.performed -= OnButton2;
        button3Action.performed -= OnButton3;
        button4Action.performed -= OnButton4;

        inputActionAsset.Disable();

    } // END OnDisable


    #endregion


    #region INPUT FUNCTIONS


    // Called when button 1 is performed
    //--------------------------------------//
    private void OnButton1(InputAction.CallbackContext context)
    //--------------------------------------//
    {
        menuNavigation.Button1(context);

    } // END OnButton1


    // Called when button 2 is performed
    //--------------------------------------//
    private void OnButton2(InputAction.CallbackContext context)
    //--------------------------------------//
    {
        menuNavigation.Button2(context);

    } // END OnButton2


    // Called when button 3 is performed
    //--------------------------------------//
    private void OnButton3(InputAction.CallbackContext context)
    //--------------------------------------//
    {
        menuNavigation.Button3(context);

    } // END OnButton3


    // Called when button 4 is performed
    //--------------------------------------//
    private void OnButton4(InputAction.CallbackContext context)
    //--------------------------------------//
    {
        menuNavigation.Button4(context);

    } // END OnButton4


    #endregion


} // END VLAT_InputManager.cs
