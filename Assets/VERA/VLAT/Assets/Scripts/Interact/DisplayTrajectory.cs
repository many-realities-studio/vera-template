using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayTrajectory : MonoBehaviour
{

    // DisplayTrajectory handles the throwing trajectory display


    #region VARIABLES


    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField][Range(20, 100)] private int _lineSegmentCount = 100;
    private List<Vector3> _linePoints = new List<Vector3>();
    public static DisplayTrajectory Instance;
    private bool showLine;
    private float vA;
    private float hA;
    private GrabTracker grabHandler;
    private Vector3 throwDirection;
    private float throwForce;


    #endregion


    #region MONOBEHAVIOUR


    // Awake
    //--------------------------------------//
    private void Awake()
    //--------------------------------------//
    {
        Instance = this;
        showLine = false;
        grabHandler = FindObjectOfType<GrabTracker>();

        if (_lineRenderer != null)
        {
            _lineRenderer.transform.position = Vector3.zero;
        }

    } // END Awake


    // Update
    //--------------------------------------//
    void Update()
    //--------------------------------------//
    {
        if (grabHandler.GetGrabbedObject() != null)
        {
            if (showLine == true)
            {
                Vector3 x = Camera.main.transform.forward;
                x = Quaternion.AngleAxis(-vA, Camera.main.transform.right) * x;
                x = Quaternion.AngleAxis(hA, Camera.main.transform.up) * x;
                setThrowAngle(x);
                Vector3 forceDirection = x * throwForce;
                GameObject obj = grabHandler.GetGrabbedObject();
                calculateLine(forceDirection, obj.transform.position);
            }
        }
        else
        {
            hideLine();
        }

    } // END Update


    #endregion


    #region ANGLE


    // setThrowingAngle
    //--------------------------------------//
    private void setThrowAngle(Vector3 x)
    //--------------------------------------//
    {
        throwDirection = x;

    } // END setThrowingAngle


    // getThrowAngle
    //--------------------------------------//
    public Vector3 getThrowAngle()
    //--------------------------------------//
    {
        return throwDirection;

    } // END getThrowAngle


    // setValues
    //--------------------------------------//
    public void setValues(float vertAng, float horAng, float tForce)
    //--------------------------------------//
    {
        showLine = true;
        vA = vertAng;
        hA = horAng;
        throwForce = tForce;

    } // END setValues


    // calculateLine
    //--------------------------------------//
    public void calculateLine(Vector3 forceVector, Vector3 startingPoint)
    //--------------------------------------//
    {
        //Transform force to velocity vector
        // Vector3 velocity = (forceVector / rigidBody.mass) * Time.fixedDeltaTime;

        // Calculate flight duration
        float flightDuration = (2 * forceVector.magnitude) / Physics.gravity.y;

        // Divide flight duration to step times
        float stepTime = flightDuration / _lineSegmentCount;
        // For each step time passed calculate the position of the object

        _linePoints.Clear();
        _linePoints.Add(startingPoint);
        for (int i = 1; i < _lineSegmentCount; i++)
        {
            float stepTimePassed = stepTime * i;
            Vector3 movementVector = new Vector3(
                forceVector.x * stepTimePassed,
                forceVector.y * stepTimePassed - 0.5f * Physics.gravity.y * stepTimePassed * stepTimePassed,
                forceVector.z * stepTimePassed
            );
            // Debug.Log(movementVector);
            Vector3 newPoint = -movementVector + startingPoint;
            RaycastHit hit;
            if (Physics.Raycast(_linePoints[i - 1], newPoint - _linePoints[i - 1], out hit, (newPoint - _linePoints[i - 1]).magnitude))
            {
                _linePoints.Add(hit.point);
                break;
            }
            _linePoints.Add(newPoint);
        }
        // Compose the line renderer using the positions
        _lineRenderer.positionCount = _linePoints.Count;
        _lineRenderer.SetPositions(_linePoints.ToArray());

    } // END calculateValues


    // hideLine
    //--------------------------------------//
    public void hideLine()
    //--------------------------------------//
    {
        _lineRenderer.positionCount = 0;
        showLine = false;

    } // END hideLine


    // lineCount
    //--------------------------------------//
    public int lineCount()
    //--------------------------------------//
    {
        return _lineRenderer.positionCount;
    
    } // END lineCount


    #endregion


} // END DisplayTrajectory.cs