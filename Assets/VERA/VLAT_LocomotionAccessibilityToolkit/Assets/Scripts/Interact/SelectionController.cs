using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;


public class SelectionController : MonoBehaviour
{

    // SelectionController controls the selection of nearby interactable objects


    #region VARIABLES


    private List<VLAT_Interactable> interactables = new List<VLAT_Interactable>();
    private int counter = 0;
    private VLAT_Interactable previousObj;
    private Outline outline;
    private GameObject lookTarget;
    [System.NonSerialized] public VLAT_Interactable currentObj;
    private bool manualHighlightCancel = false;
    private GrabTracker grabTracker;

    [SerializeField] float selectRadius;
    private Camera playerCam; // The point where all distance calculations are made (may change later)
    [SerializeField] Color outlineColor;
    [SerializeField] float outlineWidth = 5f;
    [SerializeField] float highlightDuration = 3f;
    private Arrow arrow;
    private TextMeshPro Text;
    [SerializeField] bool useCameraSelect = false;

    private GameObject interactSub;


    #endregion


    #region MONOBEHAVIOUR


    // Setup
    //--------------------------------------//
    public void Setup(float _selectRadius)
    //--------------------------------------//
    {
        grabTracker = FindObjectOfType<GrabTracker>();
        interactSub = GameObject.Find("Interact Sub");
        //Debug.Log(interactSub);
        playerCam = Camera.main;
        arrow = FindObjectOfType<Arrow>();
        selectRadius = _selectRadius;

    } // END Setup

    // Update
    //--------------------------------------//
    private void Update()
    //--------------------------------------//
    {
        if (lookTarget != null && arrow != null)
        {
            arrow.transform.LookAt(lookTarget.transform);
        }
        SelectedOutOfRange();

    } // END Update


    #endregion


    #region SELECTION


    // SelectedOutOfRange
    //--------------------------------------//
    public void SelectedOutOfRange()
    //--------------------------------------//
    {
        // UpdateSelectables();
        if (currentObj != null)
        {
            float outside = Vector3.Distance(playerCam.transform.position, currentObj.transform.position);
            if (outside > selectRadius)
            {
                // Previous current object is out of range, deselect it
                // Debug.Log("Entered Deselect");
                // Debug.Log("preob: " + previousObj);
                currentObj.GetComponent<Outline>().enabled = false;
                currentObj = null;
                lookTarget = null;
                FindObjectOfType<NewMenuNavigation>().InteractOutOfRange();
                //treeBase.back(interactSub, GameObject.Find(currentObj + "1"));
                //currentObj = null;
            }
        }

    } // END SelectedOutOfRange


    // SelectionCycle
    //--------------------------------------//
    public void SelectionCycle()
    //--------------------------------------//
    {
        // Cancel highlighting if all highlighted
        PulseCancelHighlights();

        if (!UpdateSelectables()) return; // If there is nothing to select, skip for now

        if (counter >= interactables.Count) counter = 0;
        currentObj = interactables[counter];

        // De-highlight previous object
        if (previousObj != null)
        {
            previousObj.gameObject.GetComponent<Outline>().enabled = false;
        }

        // Get current object and get / add outline component
        if (interactables[counter].GetComponent<Outline>() != null)
        {
            outline = interactables[counter].GetComponent<Outline>();
        }
        else
        {
            outline = interactables[counter].gameObject.AddComponent<Outline>();
        }

        // Enable outline
        outline.enabled = true;
        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineColor = outlineColor;
        outline.OutlineWidth = outlineWidth;

        // Disable arrow if object is grabbed
        if (arrow != null)
        {
            if (grabTracker.GetGrabbedObject() == interactables[counter])
                arrow.gameObject.SetActive(false);
            else
                arrow.gameObject.SetActive(true);
        }

        // Make sure the camera doesn't snap to the grabbed object
        if (useCameraSelect && (grabTracker.GetGrabbedObject() != interactables[counter]))
            CameraSnap(interactables[counter].gameObject);

        // Logic for determining target's object position relative to player
        ObjectInView();

        previousObj = interactables[counter];
        // Loop back around once end of list
        if (counter == interactables.Count - 1)
            counter = 0;
        else counter++;

    } // END SelectionCycle


    // Updates highlightable selectables; returns true if there are selectables, false if there are none
    //--------------------------------------//
    bool UpdateSelectables()
    //--------------------------------------//
    {
        // Honestly a suspicious way of doing this: Get all objs in radius > Loop through and find all objs with IInteractable
        interactables.Clear(); // Clear the list first to avoid dupes

        // Get all colliders nearby
        Collider[] colliders = Physics.OverlapSphere(playerCam.transform.position, selectRadius);

        // On each collider, check if there is an interactable component, add to interactables list if yes
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.GetComponent<IInteractable>() != null)
            {
                // RaycastHit hit;
                Vector3 directionToInteractable = collider.gameObject.transform.position - playerCam.transform.position;
                Collider[] cameraColliders = playerCam.GetComponentsInChildren<Collider>();
                Collider[] objChildColliders = collider.gameObject.GetComponentsInChildren<Collider>();
                Collider[] combinedColliders = cameraColliders.Concat(objChildColliders).ToArray();
                RaycastHit[] hits = Physics.RaycastAll(playerCam.transform.position, directionToInteractable, Vector3.Distance(playerCam.transform.position, collider.gameObject.transform.position));

                // If hits is 1, then we have direct line of sight on the object; add it to the list
                if (hits.Length == 1 && hits[0].collider == collider)
                {
                    interactables.Add(collider.gameObject.GetComponent<VLAT_Interactable>());
                }
                else
                {
                    // More than 1 hit between us and object.
                    // Check for blockers hindering collision with the object
                    bool objectBlocked = false;
                    hits = hits.OrderBy(hit => Vector3.Distance(playerCam.transform.position, hit.point)).ToArray();
                    foreach (RaycastHit hitCollider in hits)
                    {
                        // If collider is a child of the XR rig, ignore it (should not be considered as a "blocker"
                        if (hitCollider.collider.transform.IsChildOf(VLAT_Options.Instance.GetXrPlayerParent().transform))
                            continue;
                        // If collider is the interactable in question, we can break (can ignore objects past, since array is sorted)
                        else if (hitCollider.collider.gameObject == collider.gameObject || hitCollider.collider.transform.IsChildOf(collider.transform))
                            break;
                        // If collider is neither of above, it's a "blocker", and the object cannot be seen
                        else
                        {
                            objectBlocked = true;
                            break;
                        }
                    }
                    // If the object is not blocked, add it to the list
                    if (!objectBlocked)
                        interactables.Add(collider.gameObject.GetComponent<VLAT_Interactable>());
                }
            }
        }

        // Return whether there are interactables nearby or not
        // Debug.Log("I see " + interactables.Count);
        return (interactables.Count > 0) ? true : false;

    } // END UpdateSelectables


    // Grabs all the selectables in the scene, is used to create associating buttons in tree UI
    //--------------------------------------//
    public List<GameObject> grabAllSelectables()
    //--------------------------------------//
    {
        // Honestly a suspicious way of doing this, not to mention expensive, do it once please
        List<GameObject> interactables = new List<GameObject>();

        // Get all GameObjects nearby
        //Collider[] colliders = Physics.OverlapSphere(playerCam.transform.position, selectRadius);
        GameObject[] objects = FindObjectsOfType<GameObject>();

        // On each object, check if there is an interactable component, add to interactables list if yes
        foreach (GameObject obj in objects)
        {
            if (obj.GetComponent<IInteractable>() != null)
                interactables.Add(obj);
        }

        return interactables;

    } // END grabAllSelectables


    #endregion


    #region OBJECT IN VIEW


    // ObjectInView
    //--------------------------------------//
    void ObjectInView()
    //--------------------------------------//
    {
        lookTarget = interactables[counter].gameObject;
        Vector3 target = lookTarget.transform.position;

        // Normally this would have the player's position relative to camera but not yet!
        Vector3 playerScreenPos = playerCam.WorldToScreenPoint(playerCam.transform.position);
        Vector3 targetScreenPos = playerCam.WorldToScreenPoint(target);

        // Grab the vector to the target
        Vector3 targetPosition = new Vector3(target.x, target.y, target.z);

        // 10 hardcoded, goofy code
        bool isOffScreen = targetScreenPos.x <= 10 || targetScreenPos.x >= Screen.width || targetScreenPos.y <= 10
                            || targetScreenPos.y >= Screen.height;

        if (arrow != null || Text != null) // REFACTOR
        {
            if (isOffScreen)
            {
                //Text.text = "OFF SCREEN";
                //Text.color = Color.red;

            }
            else
            {
                //Text.text = "ON SCREEN";
                //Text.color = Color.green;
            }
        }

    } // END ObjectInView


    // CameraSnap
    //--------------------------------------//
    void CameraSnap(GameObject target)
    //--------------------------------------//
    {
        playerCam.transform.parent.LookAt(target.transform);

    } // END CameraSnap


    #endregion


    #region HIGHLIGHTING


    // Highlights all nearby selectables
    //--------------------------------------//
    public void HighlightAll()
    //--------------------------------------//
    {
        if (!UpdateSelectables()) return; // If there is nothing to select, skip for now
        StartCoroutine(highlight());

    } // END HighlightAll


    // Coroutine to highlight all interactables
    //--------------------------------------//
    IEnumerator highlight()
    //--------------------------------------//
    {
        manualHighlightCancel = false;

        foreach (VLAT_Interactable inter in interactables)
        {
            GameObject obj = inter.gameObject;
            if (obj.GetComponent<Outline>() != null)
            {
                outline = obj.GetComponent<Outline>();
                outline.enabled = true;
                outline.OutlineMode = Outline.Mode.OutlineVisible;
                outline.OutlineColor = outlineColor;
                outline.OutlineWidth = outlineWidth;
            }
            else
            {
                outline = obj.AddComponent<Outline>();
                outline.enabled = true;
                outline.OutlineMode = Outline.Mode.OutlineVisible;
                outline.OutlineColor = outlineColor;
                outline.OutlineWidth = outlineWidth;
            }
        }
        yield return new WaitForSeconds(highlightDuration);

        // If we manually cancelled highlighting during coroutine, don't cancel highlighting again
        if (!manualHighlightCancel)
        {
            foreach (VLAT_Interactable inter in interactables)
            {
                inter.gameObject.GetComponent<Outline>().enabled = false;
            }
        }

    } // END highlight


    // Pulse cancels all highlighted components
    //--------------------------------------//
    public void PulseCancelHighlights()
    //--------------------------------------//
    {
        manualHighlightCancel = true;

        foreach (VLAT_Interactable inter in interactables)
        {
            if (inter == null)
                continue;

            GameObject obj = inter.gameObject;
            if (obj.GetComponent<Outline>() != null)
            {
                outline = obj.GetComponent<Outline>();
                outline.enabled = false;
            }
        }

    } // END PulseCancelHighlights


    #endregion


    #region OTHER


    // OnDrawGizmosSelected
    //--------------------------------------//
    private void OnDrawGizmosSelected()
    //--------------------------------------//
    {
        // This assumes that the radius is drawn from player's camera, may not be true later!
        Gizmos.color = Color.red;
        if (playerCam != null)
        {
            Gizmos.DrawWireSphere(playerCam.transform.position, selectRadius);
        }

    } // END OnDrawGizmosSelected


    #endregion


} // END SelectionController.cs
