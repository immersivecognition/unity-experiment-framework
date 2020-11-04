using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;

public class ExportUFXPackage : MonoBehaviour
{
    [MenuItem("UXF/Export package")]
    static void ExportPackage()
    {
        string version;
        if (File.Exists("VERSION.txt"))
        {
            version = File.ReadAllText("VERSION.txt");
        }
        else
        {
            version = "unknown";
        }
         
        string outName = string.Format("UXF.v{0}.unitypackage", version);
        if (EditorUtility.DisplayDialog("Export Package", string.Format("Export package '{0}'?", outName), "Yes", "Cancel"))
        {
            string[] assets = new string[]
            {
                "Assets/StreamingAssets",
                "Assets/UXF",
                "Assets/WebGLTemplates"
            };

            if (!Directory.Exists("Package"))
                Directory.CreateDirectory("Package");

            string path = Path.Combine("Package", outName);

            ExportPackageOptions options = ExportPackageOptions.Recurse;

            AssetDatabase.ExportPackage(assets, path, options);
        }

    }
}
