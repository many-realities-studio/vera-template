using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventActionLogger : MonoBehaviour
{

    [SerializeField] private GameObject targetObject;
    [SerializeField] private bool onlyAllowOneLogForEntry = false;
    private bool eventAlreadyLogged = false;

    // Logs an event
    public void LogEvent(int eventId)
    {
        if (onlyAllowOneLogForEntry)
            if (eventAlreadyLogged)
                return;

        if (VERALogger.Instance.initialized && VERALogger.Instance.collecting)
        {
            VERALogger.Instance.CreateEntry(
              // Event ID
              eventId,
              // Target transform
              targetObject.transform
            );

            eventAlreadyLogged = true;
        }
    }

}
