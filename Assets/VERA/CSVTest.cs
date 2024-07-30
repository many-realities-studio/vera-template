using System.Collections;
using UnityEngine;

public class CSVTest : MonoBehaviour
{
    public CSVWriter csvWriter; // Reference to the CSVWriter component

    void Start()
    {
        // Add some entries
        CSVWriter.Instance.CreateEntry(1, "Alice", new { Age = 25, Height = 170 });
        CSVWriter.Instance.CreateEntry(2, "Bob", new { Age = 30, Height = 180 });

        // Upload the file to the server
        csvWriter.Instance.SubmitCSV();
    }
}
