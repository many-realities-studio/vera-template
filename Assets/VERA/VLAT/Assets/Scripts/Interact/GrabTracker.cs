using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTracker : MonoBehaviour
{

    // GrabTracker tracks which interactable object is currently being held


    #region VARIABLES


    [System.NonSerialized] public GameObject grabbedObject;
    private Transform objParent;
    private bool useGravity;
    private bool freezeRotation;
    private bool isKinematic;
    private RigidbodyInterpolation rigidbodyInterp;
    private GameObject arrow;


    #endregion


    #region MONOBEHAVIOUR


    // Start
    //--------------------------------------//
    void Start()
    //--------------------------------------//
    {
        arrow = GameObject.Find("Arrow");
       // if (arrow == null)
           // Debug.LogError("Arrow UI object not found!");
        //else
            //Debug.Log("Arrow UI object found");

    } //End Start


    #endregion


    #region RIGIDBODY


    // SetRB // Store RigidBody settings //
    //--------------------------------------//
    public void SetRB(Rigidbody rb)
    //--------------------------------------//
    {
        useGravity = rb.useGravity;
        freezeRotation = rb.freezeRotation;
        isKinematic = rb.isKinematic;
        rigidbodyInterp = rb.interpolation;

    }// End SetRB


    // Get Gravity Setting
    //--------------------------------------//
    public bool GetGravity()
    //--------------------------------------//
    {
        return useGravity;

    } //End GetGravity


    // Get Freeze Rotation Setting
    //--------------------------------------//
    public bool GetfreezeRotation()
    //--------------------------------------//
    {
        return freezeRotation;

    } //End GetfreezeRotation


    // Get Kinematic Setting
    //--------------------------------------//
    public bool GetKinematic()
    //--------------------------------------//
    {
        return isKinematic;

    } //End GetKinematic


    // Get Interpolation Setting
    //--------------------------------------//
    public RigidbodyInterpolation GetInterpolation()
    //--------------------------------------//
    {
        return rigidbodyInterp;

    } //End GetInterpolation

    #endregion


    #region PARENT


    // SetGrabParent // Store Grabbed Object's Parent //
    //--------------------------------------//
    public void SetGrabParent(Transform obj)
    //--------------------------------------//
    {
        objParent = obj;

    } //End SetGrabParent


    // Get Grabbed Object Parent
    //--------------------------------------//
    public Transform GetGrabParent()
    //--------------------------------------//
    {
        return objParent;

    } //End GetGrabParent


    #endregion


    #region GRABBED OBJECT


    // SetGrabbedObject // Store Grabbed Object //
    //--------------------------------------//
    public void SetGrabbedObject(GameObject obj)
    //--------------------------------------//
    {
        if (arrow != null)
        {
            if (obj != null)
                arrow.SetActive(false);
            else
                arrow.SetActive(true);
        }

        grabbedObject = obj;
 
    } //End SetGrabbedObject


    // Get Grabbed Object
    //--------------------------------------//
    public GameObject GetGrabbedObject()
    //--------------------------------------//
    {
        return grabbedObject;

    } //End GetGrabbedObject


    #endregion


} // END GrabTracker.cs
