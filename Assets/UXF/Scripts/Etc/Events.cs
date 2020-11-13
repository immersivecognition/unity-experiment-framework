using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace UXF
{

    /// <summary>
    /// Event containing a Session as a parameter 
    /// </summary>
    [Serializable]
    public class SessionEvent : UnityEvent<Session>
    {

    }

    /// <summary>
    /// Event containing a Trial as a parameter
    /// </summary>
    [Serializable]
    public class TrialEvent : UnityEvent<Trial>
    {

    }

}