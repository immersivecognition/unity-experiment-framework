using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace UXF
{
    /// <summary>
    /// Creates an EventSystem when a scene has none and configures scene-owned
    /// EventSystems for the active input backend at runtime.
    /// </summary>
    public class EventSystemFallback : MonoBehaviour
    {
        public GameObject eventSystemPrefab;

        void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            SceneManager.sceneLoaded += ConfigureSceneInput;
#endif
        }

        void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            SceneManager.sceneLoaded -= ConfigureSceneInput;
#endif
        }

        void Start()
        {
            CreateEventSystem();
        }

#if ENABLE_INPUT_SYSTEM
        void ConfigureSceneInput(Scene scene, LoadSceneMode mode)
        {
            foreach (var eventSystem in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
                ConfigureInputModule(eventSystem.gameObject);
        }

        static void ConfigureInputModule(GameObject eventSystem)
        {
            var legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (legacyModule == null) return;

            legacyModule.enabled = false;
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
                eventSystem.AddComponent<InputSystemUIInputModule>();
            Destroy(legacyModule);
        }
#endif

        void CreateEventSystem()
        {
            var existingEventSystem = FindFirstObjectByType<EventSystem>();
            if (existingEventSystem != null)
            {
#if ENABLE_INPUT_SYSTEM
                ConfigureInputModule(existingEventSystem.gameObject);
#endif
                return;
            }

            if (eventSystemPrefab == null) return;
            var newEventSystem = Instantiate(eventSystemPrefab);
            newEventSystem.name = "EventSystem";
#if ENABLE_INPUT_SYSTEM
            ConfigureInputModule(newEventSystem);
#endif
        }
    }
}
