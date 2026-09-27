using System.IO;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UXF;

public sealed class UXFPackageConsumerEditorSmokeTest
{
    [Test]
    public void PackageAdvertisesSamplesInResolvedPackage()
    {
        PackageInfo package = PackageInfo.FindForAssembly(typeof(Session).Assembly);
        Assert.That(package, Is.Not.Null);
        Assert.That(File.Exists(Path.Combine(package.resolvedPath, "Samples~", "README.md")), Is.True);
        Assert.That(File.Exists(Path.Combine(package.resolvedPath, "Samples~", "WebGLTemplates", "UXF WebGL 2020", "index.html")), Is.True);
    }
}
