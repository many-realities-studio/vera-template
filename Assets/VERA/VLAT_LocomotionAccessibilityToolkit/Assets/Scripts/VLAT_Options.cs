using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VLAT_Options : MonoBehaviour
{

    // VLAT_Options stores and distributes the various options for VLAT customization.


    #region VARIABLES


    [Header("XR Player")]
    [Tooltip("The XR player's parent game object (e.g., for the XR Interaction Toolkit, the XR Origin game object)")]
    [SerializeField] private GameObject xrPlayerParent;
    [Tooltip("The radius of the player's movement collider")]
    [SerializeField] private float xrPlayerRadius = 0.2f;
    [Tooltip("The height of the player's movement collider")]
    [SerializeField] private float xrPlayerHeight = 2.0f;

    [Header("Movement")]
    [Tooltip("The player's default movement speed, when using VLAT movement")]
    [SerializeField] private float playerMoveSpeed = 2f;
    
    [Header("Interaction")]
    [Tooltip("The maximum distance from which interactable objects may be interacted with")]
    [SerializeField] private float interactionRadius = 10f;

    [Header("Virtual Hands")]
    [Tooltip("Whether the player has virtual hands present while playing")]
    [SerializeField] private bool playerHasVirtualHands = true;
    [Tooltip("A reference to the player's left virtual hand (only required if playerHasVirtualHands = true)")]
    [SerializeField] private GameObject leftVirtualHand;
    [Tooltip("A reference to the player's right virtual hand (only required if playerHasVirtualHands = true)")]
    [SerializeField] private GameObject rightVirtualHand;


    #endregion


    #region SETUP


    // Start, distribute settings
    //--------------------------------------//
    void Start()
    //--------------------------------------//
    {
        SetupMovement();
        SetupInteraction();
        SetupSettings();
        
    } // END Start


    // Sets up the movement / character controller based on options
    //--------------------------------------//
    private void SetupMovement()
    //--------------------------------------//
    {
        if (xrPlayerParent == null)
        {
            Debug.LogError("No XR player parent given for VLAT options. Please provide one for movement to work.");
            return;
        }
            
        MovementController moveControl = FindObjectOfType<MovementController>();

        if (moveControl != null)
            moveControl.Setup(xrPlayerParent, xrPlayerRadius, xrPlayerHeight, playerMoveSpeed);
        else
            Debug.LogError("No VLAT MovementController could be found in scene; VLAT movement will not work.");

    } // END SetupMovement


    // Sets up interaction based on options
    //--------------------------------------//
    private void SetupInteraction()
    //--------------------------------------//
    {
        SelectionController selectControl = FindObjectOfType<SelectionController>();

        if (selectControl != null)
            selectControl.Setup(interactionRadius);
        else
            Debug.LogError("No VLAT SelectionController could be found in scene; VLAT interaction will not work.");

    } // END SetupInteraction


    // Sets up Settings based on options
    //--------------------------------------//
    private void SetupSettings()
    //--------------------------------------//
    {
        SettingsManager settingsControl = FindObjectOfType<SettingsManager>();

        if (settingsControl != null)
            settingsControl.Setup(playerHasVirtualHands, leftVirtualHand, rightVirtualHand);
        else
            Debug.LogError("No VLAT SettingsManager could be found in scene; VLAT settings functionality will not work.");

    } // END SetupSettings


    #endregion


} // END VLAT_Options.cs