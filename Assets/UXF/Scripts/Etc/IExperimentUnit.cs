using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
    /// <summary>
    /// Represents a unit of an experiment.
    /// </summary>
    public interface IExperimentUnit : ISettingsContainer
    {
        /// <summary>
        /// Sets wether data should be saved for this experiment unit.
        /// </summary>
        bool saveData { get; }
    }
}