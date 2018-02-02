using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExpMngr
{
    /// <summary>
    /// Class which handles the cascading settings system.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Returns a new empty settings object.
        /// </summary>
        public static Settings empty { get { return new Settings(new Dictionary<string, object>()); } }

        Settings parentSettings;
        public Dictionary<string, object> baseDict;

        /// <summary>
        /// Creates settings from dictionary
        /// </summary>
        /// <param name="dict"></param>
        public Settings(Dictionary<string, object> dict)
        {
            if (dict != null)
            {
                baseDict = dict;
            }
            else
            {
                baseDict = new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Sets the parent setting object, which is accessed when a setting is not found in the dictionary.
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(Settings parent)
        {
            parentSettings = parent;
        }

        /// <summary>
        /// Get a setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                try
                {
                    return baseDict[key];
                }
                catch (KeyNotFoundException)
                {
                    if (parentSettings != null)
                    {
                        return parentSettings[key];
                    }
                    else
                    {
                        Debug.LogError(string.Format("There is no avaiable setting \"{0}\" for this trial, block or experiment. Returning null.", key));
                        return null;
                    }                    
                }
            }

            set { baseDict[key] = value; }
        }

        /// <summary>
        /// Add a new setting to the dictionary
        /// </summary>
        /// <param name="k">Key</param>
        /// <param name="v">Value</param>
        public void Add(string k, object v)
        {
            baseDict.Add(k, v);
        }

    }
}