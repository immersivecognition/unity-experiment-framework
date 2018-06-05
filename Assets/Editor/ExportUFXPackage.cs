using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class ExportUFXPackage : MonoBehaviour {

    // Add a menu item named "Do Something" to MyMenu in the menu bar.
    [MenuItem("UXF/Export package")]
    static void ExportPackage()
    {
		string[] assets = new string[]
		{
			"Assets/Plugins",
            "Assets/StreamingAssets",
            "Assets/UXF"
		};



		if (!Directory.Exists("Package"))
			Directory.CreateDirectory("Package");
		string path = "Package/UXF.unitypackage";


        ExportPackageOptions options = ExportPackageOptions.Recurse;

        AssetDatabase.ExportPackage(assets, path, options);
    }
}
