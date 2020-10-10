using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

namespace UXFExamples
{
    public class Example_Shooter : MonoBehaviour
    {

        public Rigidbody ball;

        public Vector3 launchVelocity = new Vector3(0, 0, 10f);

        public float timeOutTime = 2f;

        private bool canShoot = false;

        public void Ready()
        {
            canShoot = true;
            ball.gameObject.SetActive(true);
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            if (canShoot)
            {
                // imaginary plane the mouse is moving on
                Plane plane = new Plane(Vector3.forward, transform.position);

                // where is my mouse on the plane?
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float distance;
                if (plane.Raycast(ray, out distance))
                {
                    // location of mouse in 3D
                    Vector3 newPos = ray.GetPoint(distance);
                    
                    // locked in y, z
                    newPos.y = transform.position.y;
                    newPos.z = transform.position.z;
                    
                    // move ball
                    ball.transform.position = newPos;
                    ball.angularVelocity = Vector3.zero;

                    // launch when clicked
                    if (Input.GetMouseButtonDown(0))
                    {
                        ball.velocity = launchVelocity;
                        canShoot = false;

                        // only have a certain time to score, or its classified as a miss!
                        Invoke("Timeout", timeOutTime);
                    }
                }


            }

        }

        // we call this when we score.
        public void Goal()
        {
            // make sure we also don't timeout
            CancelInvoke("Timeout");

            Session.instance.CurrentTrial.result["outcome"] = "goal";
            Session.instance.CurrentTrial.End();
        }

        // we call this when the time has run out (e.g. we missed and didnt hit the goal)
        public void Timeout()
        {
            Session.instance.CurrentTrial.result["outcome"] = "missed";
            Session.instance.CurrentTrial.End();
        }

    }
}