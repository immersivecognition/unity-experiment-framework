using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UXF
{
    public class ApplicationHandler : MonoBehaviour
    {
        /// <summary>
        /// Quits the application. This is a handy helper method for use with the onSessionEnd event.
        /// </summary>
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif	
        }

        /// <summary>
        /// Reloads the currently active scene. This is a handy helper method for use with the onSessionEnd event.
        /// </summary>
        public void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);            
        }
    }
}
