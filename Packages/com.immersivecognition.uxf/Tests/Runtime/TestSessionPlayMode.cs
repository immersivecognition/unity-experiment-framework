using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace UXF.Tests
{
    public class TestSessionPlayMode
    {
#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator FallbackEventSystemUsesTheActiveInputBackend()
        {
            Assume.That(Object.FindFirstObjectByType<EventSystem>(), Is.Null,
                "The test scene already has an EventSystem.");

            const string prefabPath = "Packages/com.immersivecognition.uxf/Prefabs/Etc/EventSystem.prefab";
            GameObject eventSystemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(eventSystemPrefab, Is.Not.Null);

            var bootstrap = new GameObject("UXF input fallback test");
            bootstrap.AddComponent<EventSystemFallback>().eventSystemPrefab = eventSystemPrefab;
            yield return null;

            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            Assert.That(eventSystem, Is.Not.Null);
#if ENABLE_INPUT_SYSTEM
            Assert.That(eventSystem.GetComponent<InputSystemUIInputModule>(), Is.Not.Null);
            Assert.That(eventSystem.GetComponent<StandaloneInputModule>()?.enabled, Is.Not.True);
#else
            Assert.That(eventSystem.GetComponent<StandaloneInputModule>(), Is.Not.Null);
#endif
            Object.Destroy(eventSystem.gameObject);
            Object.Destroy(bootstrap);
        }

#if ENABLE_INPUT_SYSTEM
        [UnityTest]
        public IEnumerator FallbackConvertsAnExistingSceneEventSystem()
        {
            Assume.That(Object.FindFirstObjectByType<EventSystem>(), Is.Null,
                "The test scene already has an EventSystem.");

            var sceneEventSystem = new GameObject("Existing EventSystem");
            sceneEventSystem.AddComponent<EventSystem>();
            sceneEventSystem.AddComponent<StandaloneInputModule>();
            var bootstrap = new GameObject("UXF input fallback test");
            bootstrap.AddComponent<EventSystemFallback>();
            yield return null;

            Assert.That(sceneEventSystem.GetComponent<InputSystemUIInputModule>(), Is.Not.Null);
            Assert.That(sceneEventSystem.GetComponent<StandaloneInputModule>(), Is.Null);
            Object.Destroy(sceneEventSystem);
            Object.Destroy(bootstrap);
        }
#endif
#endif

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
