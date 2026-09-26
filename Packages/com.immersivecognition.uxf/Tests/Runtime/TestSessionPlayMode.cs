using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UXF.Tests
{
    public class TestSessionPlayMode
    {
        [UnityTest]
        public IEnumerator SessionCanBeginAndEndWithoutUi()
        {
            GameObject gameObject = new GameObject("UXF PlayMode Test Session");
            Session session = gameObject.AddComponent<Session>();
            session.endOnDestroy = false;

            session.Begin("playmode_test", "participant", settings: Settings.empty);
            yield return null;

            Assert.That(session.hasInitialised, Is.True);
            session.End();
            Assert.That(session.hasInitialised, Is.False);

            Object.Destroy(gameObject);
        }
    }
}
