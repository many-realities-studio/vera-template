using UnityEngine;

public class VLAT_ThrowInteractable : MonoBehaviour
{

    // VLAT_ThrowInteractable provides various throw-based interactions


    #region VARIABLES


    private GrabTracker grabHandler;

    //[SerializeField][Range(0, 90)] 
    private float throwAngleChange = 10;
    //[SerializeField][Range(0, 90)] 
    private float throwForce = 10;
    private float newVertChange;
    private float verticleAngle;
    private float newHorzChange;
    private float horizontalAngle;
    private float newDistance = 0f;


    #endregion


    #region MONOBEHAVIOUR


    // Start
    //--------------------------------------//
    private void Start()
    //--------------------------------------//
    {
        grabHandler = FindObjectOfType<GrabTracker>();

    } // END Start


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
            //isGrabbing = false;
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


    #endregion


} // END VLAT_DefaultActions.cs
