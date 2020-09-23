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
        string[] assets = new string[]
        {
            "Assets/StreamingAssets",
            "Assets/UXF"
        };

        DateTime dt = DateTime.Now;
        string date = dt.ToString("yy_MM_dd");

        if (!Directory.Exists("Package"))
            Directory.CreateDirectory("Package");
        string path = string.Format("Package/UXF_{0}.unitypackage", date);

        ExportPackageOptions options = ExportPackageOptions.Recurse;

        AssetDatabase.ExportPackage(assets, path, options);
    }
}
