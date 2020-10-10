using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

namespace UXFExamples
{
    /// <summary>
    /// This script controls the movement of the moving target in the shooting task in the MultiSceneExperiment.
    /// </summary>
    public class Example_ShootTarget : MonoBehaviour
    {

        public float speed = 0.1f;

        public float xAmplitude = 5f;
        public Example_Shooter shooter;

        private Vector3 originalPos;


        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            originalPos = transform.position;
        }


        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            float a = 2f * (Mathf.PerlinNoise(Time.time * speed, 100f) -0.5f);
            transform.position = originalPos + new Vector3(a * xAmplitude, 0, 0);
        }


        public void SetSize(float size)
        {
            transform.localScale = Vector3.one * size;
        }


        /// <summary>
        /// OnTriggerEnter is called when the Collider other enters the trigger.
        /// </summary>
        /// <param name="other">The other Collider involved in this collision.</param>
        void OnTriggerEnter(Collider other)
        {
            if (other.name == "Ball")
            {
                Debug.Log("Goal!");
                shooter.Goal();
            }
        }

    }
}