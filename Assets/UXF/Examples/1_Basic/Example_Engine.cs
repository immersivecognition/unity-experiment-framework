using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// add the UXF namespace
using UXF;

namespace UXFExamples
{
    /// <summary>
    /// Example script used to test functionality of the Experiment Manager
    /// </summary>
    public class Example_Engine : MonoBehaviour
    {
        public float thrust = 0f;

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            // increment position upwards
            transform.position += Vector3.up * thrust * Time.deltaTime;
        }        
    }
}

