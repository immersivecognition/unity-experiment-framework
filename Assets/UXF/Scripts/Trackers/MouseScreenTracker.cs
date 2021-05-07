using UnityEngine;

namespace UXF
{
    /// <summary>
    /// Attach this component to any gameobject and assign it in the trackedObjects field in an ExperimentSession to automatically record the position of the mouse in screen coordinates. 
    /// Note (0, 0) is the bottom-left of the window displaying the game.
    /// </summary>
    public class MouseScreenTracker : Tracker
    {
        /// <summary>
        /// Sets measurementDescriptor and customHeader to appropriate values
        /// </summary>
        protected override void SetupDescriptorAndHeader()
        {
            measurementDescriptor = "mouse_screen";
            
            customHeader = new string[]
            {
                "pix_x",
                "pix_y"
            };
        }

        /// <summary>
        /// Returns current mouse position in screen coordinates
        /// </summary>
        /// <returns></returns>
        protected override UXFDataRow GetCurrentValues()
        {
            // get position and rotation
            Vector2 p = new Vector2(Input.mousePosition.x, Input.mousePosition.y);


            // return position, rotation (x, y, z) as an array
            var values = new UXFDataRow()
            {
                ("pix_x", p.x),
                ("pix_y", p.y)
            };

            return values;
        }
    }
}