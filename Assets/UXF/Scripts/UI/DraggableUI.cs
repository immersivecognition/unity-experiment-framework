using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UXF.UI
{
    /// <summary>
    /// A script that allows any GUI object to be draggable
    /// </summary>
    public class DraggableUI : MonoBehaviour, IDragHandler
    {
        public Transform transformToRaise;
        public RectTransform visibleRect;
        private Vector2 screenSize;

        void Start()
        {
            screenSize = new Vector2(Screen.width, Screen.height);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            visibleRect.transform.position += CalculateNewTransformOffset(eventData.delta);
            transformToRaise.SetAsLastSibling(); // bring to front
        }

        /// <summary>
        /// Clamps a mouse delta from the OnDrag event to ensure the dragged UI stays within the screen
        /// </summary>
        /// <param name="mouseDelta">Mouse delta from the onDrag event</param>
        /// <returns>Corrected delta offset to ensure UI doesn't go off screen</returns>
        private Vector3 CalculateNewTransformOffset(Vector2 mouseDelta)
        {
            // returns an array of corners, from bottom left clockwise.
            Vector3[] corners = new Vector3[4];
            visibleRect.GetWorldCorners(corners);

            float rectLeft = corners[0].x;
            float rectBottom = corners[0].y;
            float rectRight = corners[2].x;
            float rectTop = corners[2].y;

            Vector3 actualDelta = new Vector3(0, 0, 0);

            if (rectLeft + mouseDelta.x < 0)
                actualDelta.x = -rectLeft;
            else if (rectRight + mouseDelta.x > screenSize.x)
                actualDelta.x = screenSize.x - rectRight;
            else
                actualDelta.x = mouseDelta.x;
            
            if (rectBottom + mouseDelta.y < 0)
                actualDelta.y = -rectBottom;
            else if (rectTop + mouseDelta.y > screenSize.y)
                actualDelta.y = screenSize.y - rectTop;
            else
                actualDelta.y = mouseDelta.y;
            
            return actualDelta;
        }
    }
}
