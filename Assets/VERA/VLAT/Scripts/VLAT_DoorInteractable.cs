using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VLAT_DoorInteractable : MonoBehaviour
{

    // VLAT_DoorInteractable has various built-in functions for using the VLAT with a VR door


    #region VARIABLES


    private int currentDoorStateIndex = 0;
    private float[] doorStateTargetAngles;
    private Rigidbody rb;

    private float defaultMotorForce;
    private float motorForce = 100.0f;
    private float defaultMotorSpeed;
    private float motorSpeed = 500f;
    private bool defaultUseMotor;

    [Tooltip("Whether or not the door can open both ways (e.g., when closed, can be both pushed or pulled)")]
    [SerializeField] private bool twoWayDoor;

    // Hinged door
    [Tooltip("A reference to the door's associated hinge joint")]
    [SerializeField] private HingeJoint doorHingeJoint;
    [Tooltip("The hinge's angle when the door is \"pushed\" open")]
    [SerializeField] private float pushedOpenHingeAngle; // Only when two way
    [Tooltip("The hinge's angle when the door is \"pulled\" open")]
    [SerializeField] private float pulledOpenHingeAngle; // Only when two way
    [Tooltip("The hinge's angle when the door is open")]
    [SerializeField] private float openHingeAngle; // Only when not two way
    [Tooltip("The hinge's angle when the door is closed")]
    [SerializeField] private float closedHingeAngle; // Always


    #endregion


    #region MONOBEHAVIOUR


    // Start
    //--------------------------------------//
    private void Start()
    //--------------------------------------//
    {
        rb = doorHingeJoint.GetComponent<Rigidbody>();

        defaultMotorForce = doorHingeJoint.motor.force;
        defaultMotorSpeed = doorHingeJoint.motor.targetVelocity;
        defaultUseMotor = doorHingeJoint.useMotor;

        if (twoWayDoor)
        {
            doorStateTargetAngles = new float[4];
            doorStateTargetAngles[0] = closedHingeAngle;
            doorStateTargetAngles[1] = pushedOpenHingeAngle;
            doorStateTargetAngles[2] = closedHingeAngle;
            doorStateTargetAngles[3] = pulledOpenHingeAngle;
        }
        else
        {
            doorStateTargetAngles = new float[2];
            doorStateTargetAngles[0] = closedHingeAngle;
            doorStateTargetAngles[1] = openHingeAngle;
        }

    } // END Start


    #endregion


    #region OPEN / CLOSE


    // Opens or closes the door based on current state
    //--------------------------------------//
    public void OpenAndClose()
    //--------------------------------------//
    {
        StopCoroutine(RotateToTarget());
        StartCoroutine(RotateToTarget());

    } // END OpenAndClose


    // Coroutine to rotate hinge to target
    //--------------------------------------//
    IEnumerator RotateToTarget()
    //--------------------------------------//
    {
        currentDoorStateIndex = (currentDoorStateIndex + 1) % doorStateTargetAngles.Length;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        float targetAngle = doorStateTargetAngles[currentDoorStateIndex];

        // Set direction based on relative offset
        bool dirNegative = false;
        if (doorHingeJoint.angle < targetAngle)
            dirNegative = true;

        // Setup motor
        JointMotor motor = doorHingeJoint.motor;
        motor.force = motorForce;
        doorHingeJoint.useMotor = true;

        if (dirNegative)
        {
            // Wait until angle reached
            motor.targetVelocity = motorSpeed;
            doorHingeJoint.motor = motor;

            while (doorHingeJoint.angle < targetAngle)
            {
                yield return null;
            }
        }
        else
        {
            // Wait until angle reached
            motor.targetVelocity = -motorSpeed;
            doorHingeJoint.motor = motor;

            while (doorHingeJoint.angle > targetAngle)
            {
                yield return null;
            }
        }

        motor.targetVelocity = defaultMotorSpeed;
        motor.force = defaultMotorForce;
        doorHingeJoint.motor = motor;
        doorHingeJoint.useMotor = defaultUseMotor;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

    } // END RotateToTarget


    #endregion


} // END VLAT_DoorInteractable.cs
