using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VLAT_GrabInteractable : MonoBehaviour
{

    // VLAT_GrabInteractable handles grabbing/dropping an object with VLAT controls


    #region VARIABLES


    private GrabTracker grabHandler;


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


    #region GRAB AND DROP


    // Grabs or drops an object based on current state
    //--------------------------------------//
    public void GrabAndDrop()
    //--------------------------------------//
    {
        // If object not held, grab object
        if (grabHandler.GetGrabbedObject() == null)
            GrabObject(gameObject);
        // If object held, drop object
        else
            ReleaseObject();

    } // END GrabAndDrop


    // Grabs an object
    //--------------------------------------//
    private void GrabObject(GameObject obj)
    //--------------------------------------//
    {
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

        //Adjusting vectors to the correct position relative to the camera
        Quaternion grabRotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);

        //Set Object's parent to camera
        transform.SetParent(Camera.main.transform, true);

        //Setting position to bottom right of pov and setting rotation to the correct orientation
        obj.transform.position = targetPosition;
        obj.transform.rotation = grabRotation;

    } // END GrabObject


    // ReleaseObject
    //--------------------------------------//
    private void ReleaseObject()
    //--------------------------------------//
    {
        //Get Grabbed Object
        GameObject obj = grabHandler.GetGrabbedObject();
        //Set objects parent back to original
        obj.transform.SetParent(grabHandler.GetGrabParent(), true);

        // Adjust the offset as needed// adjust for distance before drop
        Vector3 zero = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 offset = new Vector3(0.5f, 0.5f, 2.0f);
        Vector3 targetPosition = Camera.main.ViewportToWorldPoint(offset);

        // Check for collisions along the path
        if (Physics.Linecast(zero, targetPosition))
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


} // END VLAT_GrabInteractable.cs
