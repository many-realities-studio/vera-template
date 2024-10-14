using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventActionLogger : MonoBehaviour
{

    [SerializeField] private GameObject targetObject;

    // Update is called once per frame
    public void LogEvent(int eventId)
    {
        if (VERALogger.Instance.initialized && VERALogger.Instance.collecting)
        {
            VERALogger.Instance.CreateEntry(
              // Event ID
              eventId,
              // Target transform
              targetObject.transform
            );

        }
    }

}
