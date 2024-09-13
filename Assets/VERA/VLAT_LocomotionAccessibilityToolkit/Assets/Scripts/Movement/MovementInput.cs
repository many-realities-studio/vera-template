using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class MovementInput : MonoBehaviour
{

    // MovementInput handles the inputs relating to movement

    
    #region VARIABLES


    public float buttonPress1 = 0;
    public float buttonPress2 = 0;
    public float buttonPress3 = 0;
    public float buttonPress4 = 0;
    public Vector2 stickInput = Vector2.zero;
    public float state = 0;
    // If true, movement will not be done via input actions, but called via the tree
    [SerializeField] private bool movementControlledByTree = false;
    // Variable to change the accessibility level to change controls for movement.
    // [SerializeField] public int accessLevel = 1;

    #endregion


    #region HANDLE PERFORMANCE OF INPUT


    // These functions set the value to 1 to signify that it was pressed
    //--------------------------------------//
    private void SetPressed1(InputAction.CallbackContext ctx)
    //--------------------------------------//
    {
        if (!movementControlledByTree)
            buttonPress1 = ctx.ReadValue<float>();

    } // END SetPressed1


    // 2
    //--------------------------------------//
    private void SetPressed2(InputAction.CallbackContext ctx)
    //--------------------------------------//
    {
        if (!movementControlledByTree)
            buttonPress2 = ctx.ReadValue<float>();
    
    } // END SetPressed2


    // 3
    //--------------------------------------//
    private void SetPressed3(InputAction.CallbackContext ctx)
    //--------------------------------------//
    {
        if (!movementControlledByTree)
            buttonPress3 = ctx.ReadValue<float>();
    
    } // END SetPressed3


    // 4
    //--------------------------------------//
    private void SetPressed4(InputAction.CallbackContext ctx)
    //--------------------------------------//
    {
        if (!movementControlledByTree)
            buttonPress4 = ctx.ReadValue<float>();
    
    } // END SetPressed4


    // private void SetStage2(InputAction.CallbackContext ctx){
    //     state = ctx.ReadValue<float>();
    // }


    #endregion


    #region MOVEMENT ACTIVATION FROM TREE


    // These functions manually activate input functions as if called from input,
    //     but is instead done so via a call from the main tree UI.


    // TreeTurnLeft
    //--------------------------------------//
    public void TreeTurnLeft()
    //--------------------------------------//
    {
        buttonPress1 = 1f;

    } // END TreeTurnLeft


    // TreeMoveForward
    //--------------------------------------//
    public void TreeMoveForward()
    //--------------------------------------//
    {
        buttonPress2 = 1f;

    } // END TreeMoveForward


    // TreeTurnRight
    //--------------------------------------//
    public void TreeTurnRight()
    //--------------------------------------//
    {
        buttonPress3 = 1f;

    } // END TreeTurnRight


    #endregion


    #region TYPE 2


    // SetStickMovement
    //--------------------------------------//
    private void SetStickMovement(InputAction.CallbackContext ctx)
    //--------------------------------------//
    {
        stickInput = ctx.ReadValue<Vector2>();
    
    } // END SetStickMovement


    // private void SetLookUp(InputAction.CallbackContext ctx)
    // {
    //     upPress = ctx.ReadValue<float>();
    // }


    // private void SetLookDown(InputAction.CallbackContext ctx)
    // {
    //     downPress = ctx.ReadValue<float>();
    // }


    // private void SetInteract(InputAction.CallbackContext ctx)
    // {
    //     interactPress = ctx.ReadValue<float>();
    // }


    // private void SetMenu(InputAction.CallbackContext ctx)
    // {
    //     menuPress = ctx.ReadValue<float>();
    // }


    // SetState
    //--------------------------------------//
    private void SetState(InputAction.CallbackContext ctx)
    //--------------------------------------//
    {
        state = ctx.ReadValue<float>();
    
    } // END SetState


    #endregion
    

} // END MovementInput.cs
