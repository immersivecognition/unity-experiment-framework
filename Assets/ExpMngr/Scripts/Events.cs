using System;
using UnityEngine;
using UnityEngine.Events;

namespace ExpMngr
{

    [Serializable]
    public class SessionEvent : UnityEvent<ExperimentSession>
    {

    }

    [Serializable]
    public class TrialEvent : UnityEvent<Trial>
    {

    }


}