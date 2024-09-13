
using UnityEngine;

public class MovementController : MonoBehaviour
{

    // MovementController is the main controller which moves the player


    #region VARIABLES


    CharacterController _characterController;
    private Transform xrRig;
    private Transform mainCam;
    
    Vector3 _userMoveInput = Vector3.zero;
    Vector3 _userLookInput = Vector3.zero;

    [Header("Movement")]
    MovementInput _input;

    [SerializeField] public float speed = 1f;
    [SerializeField] public float rotationValue = 15f;
    [SerializeField] public float rotationValueVertical = 10f;
    // variables for analog turning (level 2)
    [SerializeField] public float turnCooldown = 1.0f;
    float lastPressTime = 0f;
    // determines which level of accessibity the user is on
    [System.NonSerialized] public int currentLvl = 1;
    private bool useOneTapMove = false;
    private bool continualMoveOn = false;

    private bool movementActive = false;


    #endregion


    #region SETUP


    // Setup
    //--------------------------------------//
    public void Setup(GameObject xrRig, float radius, float height, float moveSpeed)
    //--------------------------------------//
    {
        _input = GetComponent<MovementInput>();
        mainCam = Camera.main.transform;
        //xrRig = FindObjectOfType<XROrigin>().transform;

        this.xrRig = xrRig.transform;
        // Spawn character controller for movement capabilities
        _characterController = xrRig.GetComponent<CharacterController>();
        if (_characterController == null)
        {
            _characterController = xrRig.AddComponent<CharacterController>();
            _characterController.radius = radius;
            _characterController.height = height / 2f;
            _characterController.center = Vector3.up * (height / 4f);
        }

        speed = moveSpeed;

        movementActive = true;

    } // END Setup


    #endregion


    #region FIXED UPDATE


    // FixedUpdate
    //--------------------------------------//
    private void FixedUpdate()
    //--------------------------------------//
    {
        if (!movementActive)
            return;

        // These check if the switch has been pressed
        bool switchDown1 = _input.buttonPress1 > 0.1f;
        bool switchDown2 = _input.buttonPress2 > 0.1f;
        bool switchDown3 = _input.buttonPress3 > 0.1f;
        bool switchDown4 = _input.buttonPress4 > 0.1f;
        bool statePressed = _input.state > 0.1f;

        // If the user is in the air then gravity will bring them down until grounded
        if (_characterController.isGrounded == false)
        {
            _userMoveInput += Physics.gravity;
            _characterController.Move(new Vector3(0f, _userMoveInput.y, 0f) * speed * Time.deltaTime);
        }

        if (continualMoveOn)
        {
            UserMove();
        }

        if (switchDown1)
        {
            //Debug.Log(_input.buttonPress1 + " Turn L");
            _userLookInput = GetTurnLInput();
            UserLook();
            _input.buttonPress1 = 0;
        }

        if (switchDown2)
        {
            //Debug.Log(_input.buttonPress2 + " Forward");
            _userMoveInput = GetMoveInput();
            // If using one tap move, begin continual movement
            if (useOneTapMove)
            {
                if (continualMoveOn)
                {
                    continualMoveOn = false;
                    _input.buttonPress2 = 0;
                }
                else
                {
                    continualMoveOn = true;
                    _input.buttonPress2 = 0;
                }
            }
            else
            {
                UserMove();
                _input.buttonPress2 = 0;
            }
        }

        if (switchDown3)
        {
            //Debug.Log(_input.buttonPress3 + " Turn R");
            _userLookInput = GetTurnRInput();
            UserLook();
            _input.buttonPress3 = 0;
        }
        if (switchDown4)
        {
            //Debug.Log("Back to tree");
            _input.buttonPress4 = 0;
        }
        if (statePressed)
        {
            currentLvl = 2;
            _input.state = 0;
            //Debug.Log("Level: " + currentLvl);
        }


    } // END FixedUpdate


    #endregion


    #region MOVEMENT FUNCTIONS


    // Returns a value to be used to calculate the distance traveled for level 1 movement
    //--------------------------------------//
    private Vector3 GetMoveInput()
    //--------------------------------------//
    {
        return xrRig.transform.forward * speed;

    } // END GetMoveInput


    // Returns a value to be used to turn the rig a set amount of degrees
    //--------------------------------------//
    private Vector3 GetTurnLInput()
    //--------------------------------------//
    {
        return new Vector3 (0, -rotationValue, 0);
    
    } // END GetTurnLInput


    // GetTurnRInput
    //--------------------------------------//
    private Vector3 GetTurnRInput()
    //--------------------------------------//
    {
        return new Vector3 (0, rotationValue, 0);
    
    } // END GetTurnRInput


    // Sets whether we are using one-tap movement
    //--------------------------------------//
    public void SetOneTapMove(bool newVal)
    //--------------------------------------//
    {
        useOneTapMove = newVal;
    
    } // END SetOneTapMove


    // Controls forward movement for level 1
    //--------------------------------------//
    private void UserMove()
    //--------------------------------------//
    {
        _userMoveInput = new Vector3(_userMoveInput.x, _userMoveInput.y, _userMoveInput.z);
        _userMoveInput = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);

        if (useOneTapMove)
        {
            _characterController.Move(_userMoveInput * speed * Time.deltaTime * .15f);
        }
        else
        {
            _characterController.Move(_userMoveInput * speed * .02f);
        }
    
    } // END UserMove


    // Rotates the rig based on if the rotation is left or right.
    //--------------------------------------//
    private void UserLook()
    //--------------------------------------//
    {
        xrRig.transform.eulerAngles = xrRig.transform.eulerAngles + _userLookInput;
    
    } // END UserLook


    // References the camera instead of the rig itself since vertical adjustsments caused the rig to 
    //      rotate instead of just the camera.
    //--------------------------------------//
    private void UserLookVertical()
    //--------------------------------------//
    {
        mainCam.eulerAngles = mainCam.eulerAngles + _userLookInput;
    
    } // END UserLookVertical


    // Translates user stick input into a vector value for movement
    //--------------------------------------//
    private Vector3 GetMoveInputAnalog()
    //--------------------------------------//
    {
        return new Vector3(_input.stickInput.x, 0.0f, _input.stickInput.y);
    
    } // END GetMoveInputAnalog


    // Controls movement through the stick
    //      Direct forward and back on the stick does locomotion in the respective directions
    //      Left and right causes the rig to turn left and right which gets put on a cooldown
    //--------------------------------------//
    private void UserMoveAnalog()
    //--------------------------------------//
    {
        // Checks input for the movement while limiting left and right movement
        bool movementCheck = (_userMoveInput.z > 0.9f && _userMoveInput.x < 0.2f && _userMoveInput.x > -0.2f) || 
                             (_userMoveInput.z < -0.9f && _userMoveInput.x < 0.2f && _userMoveInput.x > -0.2f);
        bool turnCheck = (_userMoveInput.x > 0.8f || _userMoveInput.x < -0.8f) && _userMoveInput.z < 0.9f && _userMoveInput.z > -0.9f;
        // Turning
        if(turnCheck)
        {   
            float currentTime = Time.time;
            float diffSecs = currentTime - lastPressTime;
            if(diffSecs >= turnCooldown)
            {
                lastPressTime = currentTime;
                _userLookInput = new Vector3(0, rotationValue * Mathf.Round(_userMoveInput.x),0);
                UserLook();
            }
            else
            {
                Debug.Log("Turning on cooldown");
            }
        }
        else if(movementCheck)
        {
            _userMoveInput = Camera.main.transform.right * _userMoveInput.x + Camera.main.transform.forward * _userMoveInput.z;
            // Continous forward movement
            if(_characterController.isGrounded == false)
            {
                _userMoveInput += Physics.gravity;
            }
            Debug.Log(_userMoveInput);
            _characterController.Move(_userMoveInput * speed * Time.deltaTime);
        } 

    } // END UserMoveAnalog


    // Generates a rotation value to be used later
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


    #endregion
    

} // END MovementController.cs

