using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementControllerT2 : MonoBehaviour
{
    // MovementControllerT2 handles type 2 movement
    // NOTE: CURRENTLY UNUSED


    #region VARIABLES

    Rigidbody _rigidbody = null;
    [SerializeField] Transform Rig;
    [SerializeField] Transform Camera;
    Vector3 _userMoveInput = Vector3.zero;
    Vector3 _userLookInput = Vector3.zero;
    float lastPressTime = 0f;
    [Header("Movement")]
    [SerializeField] MovementInput _input;

    [SerializeField] public float speed = 10f;
    [SerializeField] public float rotationValue = 15f;
    [SerializeField] public float rotationValueVertical = 10f;
    [SerializeField] public float turnCooldown = 1.0f;
    #endregion


    #region MONOBEHAVIOUR


    // Awake
    //--------------------------------------//
    private void Awake()
    //--------------------------------------//
    {
        _rigidbody = GetComponent<Rigidbody>();
    
    } // END Awake


    // private void FixedUpdate(){
    //     // Switch input checks
    //     bool lookUpPressed = _input.upPress > 0.1f;
    //     bool lookDownPressed = _input.downPress > 0.1f;
    //     bool interactPressed = _input.interactPress > 0.1f;
    //     bool menuPressed = _input.menuPress > 0.1f;
    //     bool statePressed = _input.state > 0.1f;

    //     // Movement
    //     _userMoveInput = GetMoveInput();
    //     UserMove();

    //     // if(_userMoveInput != Vector3.zero)
    //     // {
    //     //     Debug.Log(_userMoveInput);
    //     // }
    //     // Switch Inputs 
    //     if(lookUpPressed)
    //     {
    //         Debug.Log(_input.upPress + " Turn U");
    //         _userLookInput = GetLookUpInput();
    //         UserLook();
    //         _input.upPress = 0;
    //     }

    //     if(lookDownPressed)
    //     {
    //         Debug.Log(_input.downPress + " Turn D");
    //         _userLookInput = GetLookDownInput();
    //         UserLook();
    //         _input.downPress = 0;
    //     }

    //     if(interactPressed)
    //     {
    //         Debug.Log("Interact change goes here");
    //         _input.interactPress = 0;
    //     }

    //     if(menuPressed)
    //     {
    //         Debug.Log("Menu change goes here");
    //         _input.menuPress = 0;
    //     }

    //     if(statePressed)
    //     {
    //         Debug.Log("State changed to 1");
    //         _input.state = 0;

    //     }
    // }


    #endregion


    #region MOVEMENT FUNCTIONS


    // GetMoveInput
    //--------------------------------------//
    private Vector3 GetMoveInput()
    //--------------------------------------//
    {
        return new Vector3(_input.stickInput.x, 0.0f, _input.stickInput.y);
    
    } // END GetMoveInput


    // UserMove
    //--------------------------------------//
    private void UserMove()
    //--------------------------------------//
    {
        // Checks input for the movement while limiting left and right movement
        bool movementCheck = (_userMoveInput.z > 0.9f && _userMoveInput.x < 0.2f && _userMoveInput.x > -0.2f) || 
                             (_userMoveInput.z < -0.9f && _userMoveInput.x < 0.2f && _userMoveInput.x > -0.2f);
        bool turnCheck = (_userMoveInput.x > 0.8f || _userMoveInput.x < -0.8f) && _userMoveInput.z < 0.9f && _userMoveInput.z > -0.9f;
        // Turning
        if(turnCheck)
        {   
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            float currentTime = Time.time;
            float diffSecs = currentTime - lastPressTime;
            if(diffSecs >= turnCooldown)
            {
                lastPressTime = currentTime;
                _userLookInput = new Vector3(0, rotationValue * Mathf.Round(_userMoveInput.x),0);
                UserLookHorizontal();
            }
            else
            {
                Debug.Log("Turning on cooldown");
            }
        }
        else if(movementCheck)
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _userMoveInput = new Vector3(_userMoveInput.x,
                                        _userMoveInput.y,
                                        _userMoveInput.z * speed * _rigidbody.mass);
            // Continous forward movement
            Debug.Log(_userMoveInput);
            _rigidbody.AddRelativeForce(_userMoveInput, ForceMode.Force);
            
            // Debug.Log("Moving");
        } 
        // else {
        //     _userMoveInput = new Vector3(0,
        //                                 0,
        //                                 0);
        //     // Debug.Log("Not Moving");
        // }

    } // END UserMove


    // GetLookUpInput
    //--------------------------------------//
    private Vector3 GetLookUpInput()
    //--------------------------------------//
    {
        return new Vector3 (-rotationValueVertical, 0, 0);
    
    } // END GetLookUpInput


    // GetLookDownInput
    //--------------------------------------//
    private Vector3 GetLookDownInput()
    //--------------------------------------//
    {
        return new Vector3 (rotationValueVertical, 0, 0);
    
    } // END GetLookDownInput


    // References the camera instead of the rig itself since vertical adjustsments caused the rig to 
    //      rotate instead of just the camera.
    //--------------------------------------//
    private void UserLook()
    //--------------------------------------//
    {
        Camera.transform.eulerAngles = Camera.transform.eulerAngles + _userLookInput;
    
    } // END UserLook


    // UserLookHorizontal
    //--------------------------------------//
    private void UserLookHorizontal()
    //--------------------------------------//
    {
        Rig.transform.eulerAngles = Rig.transform.eulerAngles + _userLookInput;
    
    } // END UserLookHorizontal


    #endregion

} // END MovementControllerT2
