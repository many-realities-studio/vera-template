using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Looking : MonoBehaviour
{

    // Looking handles the user's view rotation adjustment
    // NOTE: CURRENTLY BROKEN (see full documentation for details)


    #region VARIABLES


    public float speed;
    public float smooth;
    public float minAngle = 90f;
    public float maxAngle = 90f;
    private Transform mainCam;


    #endregion


    #region MONOBEHAVIOUR


    // Start
    //--------------------------------------//
    void Start()
    //--------------------------------------//
    {
        mainCam = Camera.main.transform;

    } // END Start


    #endregion


    #region LOOKING


    // LookUp
    //--------------------------------------//
    public void LookUp()
    //--------------------------------------//
    {
        if (Camera.main.transform.parent.parent.rotation.eulerAngles.x % 360 <= 90 || Camera.main.transform.parent.parent.rotation.eulerAngles.x % 360 > 270)
        {
            Vector3 oldPos = mainCam.parent.position;
            Camera.main.transform.parent.parent.Rotate(-speed, 0f, 0f, Space.Self);
            mainCam.parent.position = oldPos;
        }

    } // END LookUp


    // LookDown
    //--------------------------------------//
    public void LookDown()
    //--------------------------------------//
    {
        if (Camera.main.transform.parent.parent.rotation.eulerAngles.x % 360 < 90 || Camera.main.transform.parent.parent.rotation.eulerAngles.x % 360 >= 270)
        {
            Vector3 oldPos = mainCam.parent.position;
            Camera.main.transform.parent.parent.Rotate(speed, 0f, 0f, Space.Self);
            mainCam.parent.position = oldPos;
        }

    } // END lookDown


    // ResetAngle
    //--------------------------------------//
    public void ResetAngle()
    //--------------------------------------//
    {
        Vector3 oldPos = mainCam.parent.position;
        Camera.main.transform.parent.parent.rotation = Quaternion.Euler(0, mainCam.parent.rotation.eulerAngles.y, mainCam.parent.rotation.eulerAngles.z);
        mainCam.parent.position = oldPos;
    
    } // END ResetAngle


    #endregion


} // END Looking.cs
