using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXFExamples
{
    public class Example_CursorFollower : MonoBehaviour
    {

        public Camera cam;

        // Update is called once per frame
        void Update()
        {
            Vector3 mousePos = Input.mousePosition;
			Vector3 worldPos = cam.ScreenToWorldPoint(
				new Vector3(mousePos.x, mousePos.y, 1f)
			);
			transform.position = worldPos;
        }
    }
}