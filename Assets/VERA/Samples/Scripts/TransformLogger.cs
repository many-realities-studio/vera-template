using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformLogger : MonoBehaviour
{

    // TransformLogger logs the transform of an object every frame, under given eventId

    [SerializeField] private Transform targetObject;
    [SerializeField] private int eventId;

    // Update, perform log
    void Update()
    {
        if (VERALogger.Instance.initialized && VERALogger.Instance.collecting)
            VERALogger.Instance.CreateEntry(eventId, targetObject.transform);
    }

}
