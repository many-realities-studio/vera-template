using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiCameraAttacher : MonoBehaviour
{

    // UiCameraAttacher attaches the UI to the main camera


    #region VARIABLES


    [SerializeField] private Transform parentTrans;


    #endregion


    #region MONOBEHAVIOUR


    // Start
    //--------------------------------------//
    void Start()
    //--------------------------------------//
    {
        parentTrans.SetParent(Camera.main.transform, false);

    } // END Start


    #endregion


} // END UiCameraAttacher.cs
