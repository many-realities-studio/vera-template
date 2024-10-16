using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalColorManager : MonoBehaviour
{

    // GlobalColorManager provides a centralized area for obtaining color themes


    #region VARIABLES


    public enum ColorType
    {
        Look,
        Move,
        Interact,
        Settings,
        Highlight
    }

    public Color lookColorLight;
    public Color lookColorDark;
    public Color moveColorLight;
    public Color moveColorDark;
    public Color interactColorLight;
    public Color interactColorDark;
    public Color settingsColorLight;
    public Color settingsColorDark;
    public Color highlightColor;

    public static GlobalColorManager Instance;


    #endregion


    #region MONOBEHAVIOUR


    // Start
    //--------------------------------------//
    void Start()
    //--------------------------------------//
    {
        InitSingleton();

    } // END Start


    // Inits singleton
    //--------------------------------------//
    private void InitSingleton()
    //--------------------------------------//
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;

    } // END InitSingleton


    #endregion


} // END GlobalColorManager.cs
