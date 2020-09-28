using UnityEngine;
using System.Collections;
using System;

namespace UXF.EditorUtils
{
    public class Comment : MonoBehaviour
    {
        public string comment;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            if (!Application.isEditor) Destroy(this);
        }
    }
}