using System;
using UnityEngine;
using System.Collections;

namespace BasteRainGames
{
    public class HidingAttribute : PropertyAttribute { }

    /// <summary>
    /// Hides a field if the bool 'variable' has the state 'state'
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfAttribute : HidingAttribute
    {

        public readonly string variable;
        public readonly bool state;

        public HideIfAttribute(string variable, bool state, int order = 0)
        {
            this.variable = variable;
            this.state = state;
            this.order = order;
        }
    }

    /// <summary>
    /// Hides a field if the Object 'variable' is null
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfNullAttribute : HidingAttribute
    {

        public readonly string variable;

        public HideIfNullAttribute(string variable, int order = 0)
        {
            this.variable = variable;
            this.order = order;
        }
    }

    /// <summary>
    /// Hides a field if the Object 'variable' is not null
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfNotNullAttribute : HidingAttribute
    {

        public readonly string variable;

        public HideIfNotNullAttribute(string variable, int order = 0)
        {
            this.variable = variable;
            this.order = order;
        }
    }

    /// <summary>
    /// Hides a field based on it's enum value.
    /// use hideIf to specify if the variable must be equal to one of the attributes, or if it must be 
    /// unequal to all of the attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfEnumValueAttribute : HidingAttribute
    {

        public readonly string variable;
        public readonly int[] states;
        public readonly bool hideIfEqual;

        public HideIfEnumValueAttribute(string variable, HideIf hideIf, params int[] states)
        {
            this.variable = variable;
            this.hideIfEqual = hideIf == HideIf.Equal;
            this.states = states;
            this.order = -1;
        }
    }

    public enum HideIf
    {
        Equal,
        NotEqual
    }
}