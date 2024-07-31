using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestActionLogger : MonoBehaviour
{
  int logNumber = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

  // Update is called once per frame
  void Update()
  {
    if (logNumber == 100 && VERALogger.Instance.initialized && VERALogger.Instance.collecting) {
      VERALogger.Instance.collecting = false;
      VERALogger.Instance.SubmitCSV();
    } else {
    VERALogger.Instance.CreateEntry(1, 
      // Third column
      "Data Entry",
      // Fourth column
      logNumber++
    );

    }
  }

}
