using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestActionLogger : MonoBehaviour
{
  int logNumber = 0;
  int logFrames = 1000;

  // Update is called once per frame
  void Update()
  {
    if (logNumber == logFrames && VERALogger.Instance.initialized && VERALogger.Instance.collecting) {
      VERALogger.Instance.collecting = false;
      VERALogger.Instance.SubmitCSV();
    } else {
    VERALogger.Instance.CreateEntry(
      // Event ID
      1, 
      // Fifth column
      transform,
      // Third column
      "Data Entry" + logNumber++
    );

    }
  }

}
