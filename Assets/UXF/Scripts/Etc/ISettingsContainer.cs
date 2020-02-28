using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
    public interface ISettingsContainer
    {
        Settings settings { get; }
    }
}