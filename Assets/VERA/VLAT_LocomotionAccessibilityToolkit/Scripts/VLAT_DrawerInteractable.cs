using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VLAT_DrawerInteractable : MonoBehaviour
{

    // VLAT_DrawerInteractable has various built-in functions for using the VLAT with a VR door


    #region VARIABLES


    [Tooltip("The local position of the drawer when it is closed (as shown in the drawer's transform inspector; ensure this position is within the drawer's limits, to prevent undesired behavior)")]
    [SerializeField] private Vector3 closedLocalPosition;
    [Tooltip("The local position of the drawer when it is opened (as shown in the drawer's transform inspector; ensure this position is within the drawer's limits, to prevent undesired behavior)")]
    [SerializeField] private Vector3 openedLocalPosition;

    private bool isOpen = false;
    private float drawerSpeed = 4f;
    private Vector3 targetPosition;


    #endregion


    #region OPEN / CLOSE


    // Opens or closes the drawer
    //--------------------------------------//
    public void OpenAndCloseDrawer()
    //--------------------------------------//
    {
        StopCoroutine(MoveDrawer());

        if (isOpen)
        {
            targetPosition = closedLocalPosition;
            StartCoroutine(MoveDrawer());
        }
        else
        {
            targetPosition = openedLocalPosition;
            StartCoroutine(MoveDrawer());
        }

        // Swap the state after toggling
        isOpen = !isOpen;

    } // END OpenAndCloseDrawer


    // Coroutine to smoothly move the drawer
    //--------------------------------------//
    private IEnumerator MoveDrawer()
    //--------------------------------------//
    {

        Vector3 startPosition = transform.localPosition;

        float timeElapsed = 0f;
        while (timeElapsed < 1f)
        {
            // Interpolate between start and target position over time
            float easedT = Mathf.SmoothStep(0, 1, timeElapsed);
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, easedT);
            timeElapsed += Time.deltaTime * drawerSpeed;

            yield return null;
        }

    } // END MoveDrawer


    #endregion


} // END VLAT_DrawerInteractable.cs
