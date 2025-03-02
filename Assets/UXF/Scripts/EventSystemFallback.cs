using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace UXF
{

    /// <summary>
    /// Simple script to make an event system if one does not exist already.
    /// </summary>
    [ExecuteInEditMode]
    public class EventSystemFallback : MonoBehaviour
    {
        
        public GameObject eventSystemPrefab;

        void Reset()
        {
            CreateEventSystem();
        }

        void Start()
        {
            CreateEventSystem();
        }

        void CreateEventSystem()
        {
#if UNITY_6000
            if (FindFirstObjectByType<EventSystem>() == null)
#else
            if (FindObjectOfType<EventSystem>() == null)
#endif
            {
                var newEventSystem = Instantiate(eventSystemPrefab);
                newEventSystem.name = "EventSystem";
            }
        }
    }
}
