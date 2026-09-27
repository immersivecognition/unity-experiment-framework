using NUnit.Framework;
using UnityEngine;
using UXF;

public sealed class UXFPackageConsumerSmokeTest
{
    [Test]
    public void PackageRuntimeCanCreateAndEndUiFreeSession()
    {
        GameObject gameObject = new GameObject("UXF package consumer smoke test");
        Session session = gameObject.AddComponent<Session>();
        session.endOnDestroy = false;
        session.CreateBlock(1);

        session.Begin("consumer_smoke", "participant", settings: Settings.empty);
        session.BeginNextTrial();
        session.CurrentTrial.result["answer"] = 42;
        session.CurrentTrial.End();
        session.End();

        Assert.That(session.hasInitialised, Is.False);
        Object.DestroyImmediate(gameObject);
    }
}
