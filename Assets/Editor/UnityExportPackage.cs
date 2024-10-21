using UnityEngine;
using UnityEditor;
using System.IO;

public class ExportUnityPackage
{
    [MenuItem("VERA/Export Package")]
    public static void Export()
    {
        string[] foldersToExport = { 
          "Assets/VERA", 
          "Assets/Plugins" 
        }; // Add the paths of the folders you want to include

        string outputPath = Path.Combine(Application.dataPath, "VERA-Unity-plugin-0.1.0.unitypackage");
        
        AssetDatabase.ExportPackage(foldersToExport, outputPath, ExportPackageOptions.Recurse);
        Debug.Log($"Exported package to: {outputPath}");
    }
}
