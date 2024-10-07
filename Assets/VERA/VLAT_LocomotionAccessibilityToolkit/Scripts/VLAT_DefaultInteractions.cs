using UnityEngine;

public class VLAT_DefaultInteractions : MonoBehaviour
{

    // VLAT_DefaultActions provides various simple default interactions, such as pick up, throw, etc.


    #region VARIABLES


    private GrabTracker grabHandler;
    //[SerializeField] 
    private bool swapDoorDirection;
    private bool isGrabbing = false;
    private Renderer rend;
    private HingeJoint joint;
    private ConfigurableJoint gJoint;
    private int doorState = 0;
    private Quaternion doorStartAngle;
    private bool startP;
    private bool gReverse;
    private bool cantMove;
    private float newDistance = 0;

    //[SerializeField][Range(0, 90)] 
    private float throwAngleChange = 10;
    //[SerializeField][Range(0, 90)] 
    private float throwForce = 10;
    private float newVertChange;
    private float verticleAngle;
    private float newHorzChange;
    private float horizontalAngle;


    #endregion


    #region MONOBEHAVIOUR


    // Awake
    //--------------------------------------//
    private void Awake()
    //--------------------------------------//
    {
        joint = GetComponent<HingeJoint>();
        grabHandler = FindObjectOfType<GrabTracker>();
        doorStartAngle = transform.rotation;
        startP = true;
        gReverse = false;
        gJoint = GetComponent<ConfigurableJoint>();
        if (grabHandler == null)
        {
            // Debug.LogError("GrabTracker is not assigned or not found.");
        }
        else
        {
            // Debug.Log("GrabTracker found.");
        }
        rend = GetComponent<Renderer>();

    } // END Awake


    #endregion


    #region DEFAULT ASSIGNABLE FUNCTIONS


    // Grab/Release
    //--------------------------------------//
    public void GrabAndDrop()
    //--------------------------------------//
    {
        //Object Must be grabbable
        //if (GetComponent<XRGrabInteractable>() != null)
        //{
            if (isGrabbing == false)
            {
                //Checks if there is an Object grabbed or not
                //Can probably remove above if statement//Will remove later
                if (grabHandler.GetGrabbedObject() == null)
                {
                    //if not holding anything grab current object
                    GrabObject(gameObject);
                }
                else
                {
                    //if holding something drop grabbed object
                    ReleaseObject();
                }
            }
            else
            {
                ReleaseObject();
            }
        //}
        //else
        //{
        //    Debug.LogError("Not A Grab Interactable");
        //}

    }//END Grab/Release


    //OpenAndClose
    //--------------------------------------//
    public void DoorOpenAndClose()
    //--------------------------------------//
    {
        // If the door has 3 positions (open, close, neutral; has a max and min which are not 0)...
        if (joint.limits.max > 45 && joint.limits.min < -45)
        {
            // If the door's direction is swapped, do in reverse
            if (swapDoorDirection)
            {
                // Swap state between 0, 1, 2, 3; 0 is default closed, 1 is open max, 2 is back to neutral closed, 3 is open min, and then reset to 0.
                if (doorState == 0)
                {
                    // State is 0, next state is 1; transfer from neutral to open min.
                    transform.rotation = doorStartAngle;
                    transform.LeanRotateAround(joint.axis, joint.limits.min, .5f).setEaseOutExpo();
                    doorState++;
                }
                else if (doorState == 1)
                {
                    // State is 1, next state is 2; transfer from open min to neutral.
                    transform.LeanRotateAround(joint.axis, -joint.limits.min, .5f).setEaseOutExpo();
                    doorState++;
                }
                else if (doorState == 2)
                {
                    // State is 2, next state is 3; transfer from neutral to open max.
                    transform.rotation = doorStartAngle;
                    transform.LeanRotateAround(joint.axis, joint.limits.max, .5f).setEaseOutExpo();
                    doorState++;
                }
                else if (doorState == 3)
                {
                    // State is 3, next state is 0; transfer from open max to neutral.
                    transform.LeanRotateAround(joint.axis, -joint.limits.max, .5f).setEaseOutExpo();
                    doorState = 0;
                }
            }
            else
            {
                // Swap state between 0, 1, 2, 3; 0 is default closed, 1 is open max, 2 is back to neutral closed, 3 is open min, and then reset to 0.
                if (doorState == 0)
                {
                    // State is 0, next state is 1; transfer from neutral to open max.
                    transform.rotation = doorStartAngle;
                    transform.LeanRotateAround(joint.axis, joint.limits.max, .5f).setEaseOutExpo();
                    doorState++;
                }
                else if (doorState == 1)
                {
                    // State is 1, next state is 2; transfer from open max to neutral.
                    transform.LeanRotateAround(joint.axis, -joint.limits.max, .5f).setEaseOutExpo();
                    doorState++;
                }
                else if (doorState == 2)
                {
                    // State is 2, next state is 3; transfer from neutral to open min.
                    transform.rotation = doorStartAngle;
                    transform.LeanRotateAround(joint.axis, joint.limits.min, .5f).setEaseOutExpo();
                    doorState++;
                }
                else if (doorState == 3)
                {
                    // State is 3, next state is 0; transfer from open min to neutral.
                    transform.LeanRotateAround(joint.axis, -joint.limits.min, .5f).setEaseOutExpo();
                    doorState = 0;
                }
            }
        }
        // Else, the door has 2 positions (open, close)
        else
        {
            // Swap between states 0 and 1; 0 is neutral, 1 is open min or max.

            // If the door's open state is at the min position (e.g., min is nonzero)...
            if (joint.limits.min < -45)
            {
                if (doorState == 0)
                {
                    // State is 0, next state is 1; transfer from neutral to open min.
                    transform.rotation = doorStartAngle;
                    transform.LeanRotateAround(joint.axis, joint.limits.min, .5f).setEaseOutExpo();
                    doorState++;
                }
                else if (doorState == 1)
                {
                    // State is 1, next state is 0; transfer from open min to neutral.
                    transform.LeanRotateAround(joint.axis, -joint.limits.min, .5f).setEaseOutExpo();
                    doorState = 0;
                }
            }
            // Else, the door's open state is at the max position
            else
            {
                if (doorState == 0)
                {
                    // State is 0, next state is 1; transfer from neutral to open max.
                    transform.rotation = doorStartAngle;
                    transform.LeanRotateAround(joint.axis, joint.limits.max, .5f).setEaseOutExpo();
                    doorState++;
                }
                else if (doorState == 1)
                {
                    // State is 1, next state is 0; transfer from open max to neutral.
                    transform.LeanRotateAround(joint.axis, -joint.limits.max, .5f).setEaseOutExpo();
                    doorState = 0;
                }
            }
        }

        /* OLD OLD OLD
        // Debug.Log("joint.angle:" + joint.angle);
        //collision makes joint angle be offsetted from 0 so if its close to 0 then its closed so open
        if (Mathf.Floor(Mathf.Abs(joint.angle)) == 0)
        {
            //if has two way opening checks greater than 45 to make sure player can move through door
            if (joint.limits.max > 45 && joint.limits.min < -45)
            {
                // Debug.Log("entered Open");
                //depending on reverse state flop between directions
                if (reverse == false)
                {
                    transform.LeanRotateAround(joint.axis, joint.limits.max, .5f).setEaseOutExpo();
                }
                else
                {
                    transform.LeanRotateAround(joint.axis, joint.limits.min, .5f).setEaseOutExpo();
                }
            }
            //if has positive opening
            else if (joint.limits.max > 45)
            {
                transform.LeanRotateAround(joint.axis, joint.limits.max, .5f).setEaseOutExpo();
            }
            //if has negative opening
            else if (joint.limits.min < -45)
            {
                transform.LeanRotateAround(joint.axis, joint.limits.min, .5f).setEaseOutExpo();
            }
        }
        //is open so close
        else
        {
            // Debug.Log("entered Close");
            //reverse reverse
            reverse = !reverse;
            transform.LeanRotateAround(joint.axis, -joint.angle, .5f).setEaseOutExpo();
        }
        //has problems when collisions if kinetatic is disables
        //for better accessability, if it has the option to move outwards from player do that
        //but will require to know which direction the player is opening door from and has to decide based on that to open with min or max
        //might also want to have a smooth animation for opening
        */

    } // END OpenAndClose


    #endregion


    #region THROW / AIM


    //Throw
    //--------------------------------------//
    public void Throw()
    //--------------------------------------//
    {
        //Get Grabbed object
        GameObject obj = grabHandler.GetGrabbedObject();

        //Checks if you are grabing because if you are not holding an object you cant throw 
        //maybe change to allow to grab if not holding object similar to grab/release
        if (obj != null)
        {
            isGrabbing = false;
            //Setting object parent back to original
            obj.transform.SetParent(grabHandler.GetGrabParent(), true);

            //if object had a different orientation set using Attach Transform in XRGrabInteractable then restore original parent for it
            //XRGrabInteractable grabInteractable = obj.GetComponent<XRGrabInteractable>();
            //if (grabInteractable != null && grabInteractable.attachTransform != null)
            //{
            //    grabInteractable.attachTransform.SetParent(obj.transform, true);
            //}

            // Adjust the offset as needed//
            Vector3 offset = new Vector3(0.75f, 0.25f, 0.5f);
            Vector3 zero = Camera.main.ViewportToWorldPoint(Vector3.zero);
            Vector3 targetPosition = Camera.main.ViewportToWorldPoint(offset);
            //Checks if there is a collision between player and held object //to account for teleporting past a wall or something
            // if (CheckCollisions(zero, targetPosition))
            // {
            //     // If there is a collision, move to the point just before the collision
            //     Vector3 adjustedPosition = FindAdjustedPosition(zero, targetPosition, obj);
            //     obj.transform.position = adjustedPosition;
            // }
            // else
            // {
            // If no collision, move to the target position
            obj.transform.position = targetPosition;
            // }


            //Setting RigidBody back to normal
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                //Gravity must be true else object wont fall to the ground
                rb.useGravity = true;
                rb.freezeRotation = grabHandler.GetfreezeRotation();
                //Kinematic must be false or else force cannot be acted on it
                rb.isKinematic = false;
                rb.interpolation = grabHandler.GetInterpolation();
                //Force acted in the direction the user is looking
                Vector3 throwDirection;
                if (DisplayTrajectory.Instance.lineCount() == 0)
                {
                    throwDirection = Camera.main.transform.forward;
                }
                else
                {
                    throwDirection = DisplayTrajectory.Instance.getThrowAngle();
                }
                rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
                DisplayTrajectory.Instance.hideLine();
                verticleAngle = 0;
                newVertChange = throwAngleChange;
                horizontalAngle = 0;
                newHorzChange = throwAngleChange;
            }
            grabHandler.SetGrabbedObject(null);

        }
    }//End Throw

    #region TRAJECTORY

    public void ThrowAimActivate()
    {
        //show tragectory line with regular force
        GameObject obj = grabHandler.GetGrabbedObject();

        //Checks if you are grabing because if you are not holding an object you cant throw 
        //maybe change to allow to grab if not holding object similar to grab/release
        if (obj != null)
        {
            verticleAngle = 0;
            newVertChange = throwAngleChange;
            horizontalAngle = 0;
            newHorzChange = throwAngleChange;
            DisplayTrajectory.Instance.setValues(verticleAngle, horizontalAngle, throwForce);
        }
    }
    public void ThrowAimUp()
    {
        //move tragectory line up n units
        GameObject obj = grabHandler.GetGrabbedObject();

        //Checks if you are grabing because if you are not holding an object you cant throw 
        //maybe change to allow to grab if not holding object similar to grab/release
        if (obj != null)
        {
            if (verticleAngle < 90)
            {
                if (verticleAngle == -90)
                {
                    verticleAngle += newVertChange;
                }
                else
                {
                    if (verticleAngle + throwAngleChange > 90)
                    {
                        newVertChange = 90 - verticleAngle;
                        verticleAngle = 90;
                    }
                    else
                    {
                        verticleAngle += throwAngleChange;
                        newVertChange = throwAngleChange;
                    }
                }
            }
            DisplayTrajectory.Instance.setValues(verticleAngle, horizontalAngle, throwForce);
            //update Trajectory line
        }
    }
    public void ThrowAimDown()
    {
        //move tragectory line down n units
        //IF CURRENTDIRECTION == UP DIRECTION ROTATE NEGATIVE FROM RADIANS
        GameObject obj = grabHandler.GetGrabbedObject();

        //Checks if you are grabing because if you are not holding an object you cant throw 
        //maybe change to allow to grab if not holding object similar to grab/release
        if (obj != null)
        {
            if (verticleAngle > -90)
            {
                if (verticleAngle == 90)
                {
                    verticleAngle -= newVertChange;
                }
                else
                {
                    if (verticleAngle - throwAngleChange < -90)
                    {
                        newVertChange = 90 + verticleAngle;
                        verticleAngle = -90;
                    }
                    else
                    {
                        verticleAngle -= throwAngleChange;
                        newVertChange = throwAngleChange;
                    }
                }

            }
            DisplayTrajectory.Instance.setValues(verticleAngle, horizontalAngle, throwForce);
        }
        //update Trajectory line
    }
    public void ThrowAimRight()
    {
        //move tragectory line right n units
        GameObject obj = grabHandler.GetGrabbedObject();

        //Checks if you are grabing because if you are not holding an object you cant throw 
        //maybe change to allow to grab if not holding object similar to grab/release
        if (obj != null)
        {
            if (horizontalAngle < 90)
            {
                if (horizontalAngle == -90)
                {
                    horizontalAngle += newHorzChange;
                }
                else
                {
                    if (horizontalAngle + throwAngleChange > 90)
                    {
                        newHorzChange = 90 - horizontalAngle;
                        horizontalAngle = 90;
                    }
                    else
                    {
                        horizontalAngle += throwAngleChange;
                        newHorzChange = throwAngleChange;
                    }
                }
            }
            DisplayTrajectory.Instance.setValues(verticleAngle, horizontalAngle, throwForce);
        }
        //update Trajectory line
    }
    public void ThrowAimLeft()
    {
        //move tragectory line left n units
        GameObject obj = grabHandler.GetGrabbedObject();

        //Checks if you are grabing because if you are not holding an object you cant throw 
        //maybe change to allow to grab if not holding object similar to grab/release
        if (obj != null)
        {
            if (horizontalAngle > -90)
            {
                if (horizontalAngle == 90)
                {
                    horizontalAngle -= newHorzChange;
                }
                else
                {
                    if (horizontalAngle - throwAngleChange < -90)
                    {
                        newHorzChange = 90 + horizontalAngle;
                        horizontalAngle = -90;
                    }
                    else
                    {
                        horizontalAngle -= throwAngleChange;
                        newHorzChange = throwAngleChange;
                    }
                }

            }
            DisplayTrajectory.Instance.setValues(verticleAngle, horizontalAngle, throwForce);
        }
        //update Trajectory line
    }
    //back and throw will unshow tragectory line
    #endregion
    //end of Trajectory

    public void DrawerOpenAndClose()
    {
        Vector3 movementDirection = Vector3.zero;

        if (gJoint.xMotion != ConfigurableJointMotion.Locked)
        {
            movementDirection += transform.TransformDirection(gJoint.axis);
        }
        if (gJoint.yMotion != ConfigurableJointMotion.Locked)
        {
            movementDirection += transform.TransformDirection(gJoint.secondaryAxis);
        }
        if (gJoint.zMotion != ConfigurableJointMotion.Locked)
        {
            Vector3 crossAxis = Vector3.Cross(gJoint.axis, gJoint.secondaryAxis);
            movementDirection += transform.TransformDirection(crossAxis);
        }

        if (startP)
        {
            if (testFunction(movementDirection, gJoint.connectedAnchor, gJoint.linearLimit.limit) && testFunction(-movementDirection, gJoint.connectedAnchor, gJoint.linearLimit.limit))
            {
                if (!gReverse)
                {
                    LeanTween.move(gameObject, transform.position + movementDirection * gJoint.linearLimit.limit, .5f).setEase(LeanTweenType.easeInOutQuad);
                }
                else
                {
                    LeanTween.move(gameObject, transform.position - movementDirection * gJoint.linearLimit.limit, .5f).setEase(LeanTweenType.easeInOutQuad);
                }
                startP = false;
            }
            else
            {
                if (testFunction(movementDirection, gJoint.connectedAnchor, gJoint.linearLimit.limit))
                {
                    gReverse = false;
                    LeanTween.move(gameObject, transform.position + movementDirection * gJoint.linearLimit.limit, .5f).setEase(LeanTweenType.easeInOutQuad);
                    startP = false;
                }
                else if (testFunction(-movementDirection, gJoint.connectedAnchor, gJoint.linearLimit.limit))
                {
                    gReverse = true;
                    LeanTween.move(gameObject, transform.position - movementDirection * gJoint.linearLimit.limit, .5f).setEase(LeanTweenType.easeInOutQuad);
                    startP = false;
                }
                else
                {
                    if (!gReverse)
                    {
                        testFunction(movementDirection, gJoint.connectedAnchor, gJoint.linearLimit.limit);
                        LeanTween.move(gameObject, transform.position + movementDirection * newDistance, .5f).setEase(LeanTweenType.easeInOutQuad);
                    }
                    else
                    {
                        testFunction(-movementDirection, gJoint.connectedAnchor, gJoint.linearLimit.limit);
                        LeanTween.move(gameObject, transform.position - movementDirection * newDistance, .5f).setEase(LeanTweenType.easeInOutQuad);
                    }
                    cantMove = true;
                    startP = false;
                }
            }
        }
        else
        {
            if (!gReverse)
            {
                if (cantMove)
                {
                    LeanTween.move(gameObject, transform.position - movementDirection * newDistance, .5f).setEase(LeanTweenType.easeInOutQuad);
                }
                else
                {
                    LeanTween.move(gameObject, transform.position - movementDirection * gJoint.linearLimit.limit, .5f).setEase(LeanTweenType.easeInOutQuad);
                }
            }
            else
            {
                if (cantMove)
                {
                    LeanTween.move(gameObject, transform.position + movementDirection * newDistance, .5f).setEase(LeanTweenType.easeInOutQuad);
                }
                else
                {
                    LeanTween.move(gameObject, transform.position + movementDirection * gJoint.linearLimit.limit, .5f).setEase(LeanTweenType.easeInOutQuad);
                }
            }
            cantMove = false;
            startP = true;
            gReverse = !gReverse;
        }

    }


    #endregion


    #region HELPER FUNCTIONS


    // testFunction
    //--------------------------------------//
    bool testFunction(Vector3 direction, Vector3 pos, float limit)
    //--------------------------------------//
    {
        Physics.queriesHitBackfaces = true;
        Collider[] relaventColliders = this.GetComponentsInChildren<Collider>();
        Ray ray = new Ray(pos, direction);
        RaycastHit hit;
        if (Physics.Raycast(pos, direction, out hit, Mathf.Infinity))
        {
            // Physics.queriesHitBackfaces = false;
            // Debug.Log("hitcollider: " + hit.collider);
            // Debug.Log("hitpoint: " + hit.point);
            // Debug.Log("hitdistnce: " + hit.distance);
            foreach (Collider c in relaventColliders)
            {
                // Debug.Log("childCollider: " + c);
                if (hit.collider == c)
                {
                    return testFunction(direction, hit.point, limit);
                }
            }
            RaycastHit reverseHit;
            if (Physics.Raycast(hit.point, -direction, out reverseHit, Mathf.Infinity))
            {
                if (reverseHit.distance >= limit)
                {
                    return true;
                }
                else
                {
                    newDistance = reverseHit.distance;
                    return false;
                }
            }
            else
            {
                //i dont think ever enters here// idk cant remember
                return false;
            }
        }
        else
        {
            return true;
        }

    } // END testFunction
    
    
    // GrabObject
    //--------------------------------------//
    void GrabObject(GameObject obj)
    //--------------------------------------//
    {
        isGrabbing = true;

        //Saving Values of Grabbed Object
        grabHandler.SetGrabbedObject(obj);
        grabHandler.SetGrabParent(transform.parent);
        grabHandler.SetRB(obj.GetComponent<Rigidbody>());

        //Change RigidBody Values
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.freezeRotation = true;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
        }

        // Adjust the offset as needed
        Vector3 offset = new Vector3(0.75f, 0.25f, 0.5f);
        Vector3 targetPosition = Camera.main.ViewportToWorldPoint(offset);
        //if Attach Transform in XRGrabInteractable is not null adjust orientation accordingly
        /*XRGrabInteractable grabInteractable = obj.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null && grabInteractable.attachTransform != null)
        {
            //Adjusting vectors to the correct position relative to the camera
            Quaternion grabRotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);

            //Set AttachTransform object's parent to the camera
            grabInteractable.attachTransform.SetParent(Camera.main.transform, true);
            //Set Object's parent to the Attach Transform object
            transform.SetParent(grabInteractable.attachTransform, true);

            //Setting position to bottom right of pov and setting rotation to the correct orientation
            grabInteractable.attachTransform.position = targetPosition;
            grabInteractable.attachTransform.rotation = grabRotation;
        }
        //using default orientation
        else
        {*/
            //Adjusting vectors to the correct position relative to the camera
            Quaternion grabRotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);
            //Set Object's parent to camera
            transform.SetParent(Camera.main.transform, true);
            //Setting position to bottom right of pov and setting rotation to the correct orientation
            obj.transform.position = targetPosition;
            obj.transform.rotation = grabRotation;

        //}

    } // END GrabObject


    // ReleaseObject
    //--------------------------------------//
    void ReleaseObject()
    //--------------------------------------//
    {
        isGrabbing = false;
        //Get Grabbed Object
        GameObject obj = grabHandler.GetGrabbedObject();
        //Set objects parent back to original
        obj.transform.SetParent(grabHandler.GetGrabParent(), true);
        //If Attach Transform had an object connected revert to original parent/grabbedobject
        //XRGrabInteractable grabInteractable = obj.GetComponent<XRGrabInteractable>();
        //if (grabInteractable != null && grabInteractable.attachTransform != null)
        //{
        //    grabInteractable.attachTransform.SetParent(obj.transform, true);
        //}

        // Placing the grabbed object in front of the player

        // Adjust the offset as needed// adjust for distance before drop
        Vector3 zero = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 offset = new Vector3(0.5f, 0.5f, 2.0f);
        Vector3 targetPosition = Camera.main.ViewportToWorldPoint(offset);
        // Check for collisions along the path
        if (CheckCollisions(zero, targetPosition))
        {
            // If there is a collision, move to the point just before the collision
            Vector3 adjustedPosition = FindAdjustedPosition(zero, targetPosition, obj);
            obj.transform.position = adjustedPosition;
        }
        else
        {
            // If no collision, move to the target position
            obj.transform.position = targetPosition;
        }
        //Setting RigidBody values back to original
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = grabHandler.GetGravity();
            rb.freezeRotation = grabHandler.GetfreezeRotation();
            rb.isKinematic = grabHandler.GetKinematic();
            rb.interpolation = grabHandler.GetInterpolation();
        }
        //Set Grabbed Object to none
        grabHandler.SetGrabbedObject(null);

    } // END ReleaseObject


    // CheckCollisions
    //--------------------------------------//
    bool CheckCollisions(Vector3 start, Vector3 end)
    //--------------------------------------//
    {
        // Check for collisions using Physics.Linecast
        // This function returns true if there is a collision along the line
        return Physics.Linecast(start, end);
    } // END CheckCollisions


    // FindAdjustedPosition
    //--------------------------------------//
    Vector3 FindAdjustedPosition(Vector3 start, Vector3 end, GameObject obj)
    //--------------------------------------//
    {
        //initiating value to the target position
        Vector3 adjustedPosition = end;

        // Perform a Raycast to find the first collision along the path

        //Returns a list of all objects with colliders that are children to the main object
        Collider[] relaventColliders = obj.GetComponentsInChildren<Collider>();

        //Returns an unordered list of objects hit by the raycast from the player to the target position
        RaycastHit[] hits = Physics.RaycastAll(start, (end - start), Vector3.Distance(start, end));
        //Sort the Raycast List by distance from player
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        //for each object hit by the raycast check if it is one of the children colliders of the main object
        foreach (RaycastHit hitC in hits)
        {
            //initializing value to false each iteration
            bool isIn = false;
            foreach (Collider c in relaventColliders)
            {
                //if the raycast object collider is one of the children colliders break loop no need to check anymore
                if (hitC.collider == c)
                {
                    isIn = true;
                    break;
                }
            }
            //if raycast object is not in collider array of the main object children then set position to that objects position
            if (isIn == false)
            {
                adjustedPosition = hitC.point - (end - start).normalized * 0.1f;
            }
            //At any point of the loop if adjustedPosition is not at the targetposition that means we have first contact
            if (adjustedPosition != end)
            {
                break;
            }
        }
        return adjustedPosition;

    } // END FindAdjustedPosition


    #endregion


} // END VLAT_DefaultActions.cs
