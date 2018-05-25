using System;
using UnityEngine;
using UnityEngine.Events;

namespace UXF
{

    [Serializable]
    public class SessionEvent : UnityEvent<Session>
    {

    }

    [Serializable]
    public class TrialEvent : UnityEvent<Trial>
    {

    }


}