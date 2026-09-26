using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UXF
{
    /// <summary>
    /// Generic interface for a session builder. Possibly used in the future to build up sessions in a generic way.
    /// </summary>
    public interface IExperimentBuilder
    {
        void BuildExperiment(Session session);
    }
}