using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoTelemetryLogger : MonoBehaviour
{
    // DemoTelemetryLogger causes an object to move around in a square pattern.
    // Logs telemetry along the way. Used to demonstrate the VERA Logger.
    // Logs data for 1000 frames, then submits the CSV.

    private int logCount = 0;

    private Vector3 moveDirection = Vector3.right;
    private float moveSpeed = 1f;

    // Start; begin the direction changing coroutine (for demonstration purposes)
    private void Start()
    {
        StartCoroutine(DirectionChangeCoroutine());
        Debug.Log("Participant started!");
        VERALogger.Instance.ChangeProgress(VERALogger.StudyParticipantProgressState.IN_EXPERIMENT);
    }

    // Update; log telemetry, and then move the object (for demonstration purposes)
    void Update()
    {
        LogTelemetry();
        MoveObject();
    }

    // Logs an entry in the VERA Logger, logging the object's telemetry
    private void LogTelemetry()
    {
        // Logger expects first two columns to be event ID and a timestamp.
        // Following columns in this example are set to be "Info" and "Transform" (via the VERALogger game object).

        // Create an entry in the CSV with event ID 1, provide the log count for the "Info" column (column 3),
        // and the transform for the "Transform" column (column 4).
        VERALogger.Instance.CreateEntry(1, "Data Entry " + logCount, transform);
        logCount++;
    }

    // Updates the object's position in a cube-like pattern (for demonstration purposes)
    private void MoveObject()
    {
        transform.position = transform.position + (moveDirection * moveSpeed * Time.deltaTime);
        transform.Rotate(moveDirection, moveSpeed * 50f * Time.deltaTime);
    }

    // Periodically changes the object's direction (for demonstration purposes)
    private IEnumerator DirectionChangeCoroutine()
    { 
        while (true)
        {
            moveDirection = Vector3.right;
            yield return new WaitForSeconds(2f);
            moveDirection = Vector3.forward;
            yield return new WaitForSeconds(2f);
            moveDirection = Vector3.left;
            yield return new WaitForSeconds(2f);
            moveDirection = Vector3.back;
            yield return new WaitForSeconds(2f);
        }
    }
}
