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
            if (FindObjectOfType<EventSystem>() == null)
            {
                var newEventSystem = Instantiate(eventSystemPrefab);
                newEventSystem.name = "EventSystem";
            }
        }
    }
}