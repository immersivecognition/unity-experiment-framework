using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
    /// <summary>
    /// Class which handles the cascading settings system. Wraps a Dictionary.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Returns a new empty settings object.
        /// </summary>
        public static Settings empty { get { return new Settings(new Dictionary<string, object>()); } }

        Settings parentSettings;
        /// <summary>
        /// The underlying dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> baseDict { get; private set; }

        /// <summary>
        /// The keys for the underlying dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object>.KeyCollection Keys { get { return baseDict.Keys; } }

        /// <summary>
        /// Creates Settings instance from dictionary
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
        /// Get a boolean setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public bool GetBool(string key) { return Convert.ToBoolean(Get(key)); }

        /// <summary>
        /// Get a integer setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public int GetInt(string key) { return Convert.ToInt32(Get(key)); }

        /// <summary>
        /// Get a float setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public float GetFloat(string key) { return Convert.ToSingle(Get(key)); }

        /// <summary>
        /// Get a long setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public long GetLong(string key) { return Convert.ToInt64(Get(key)); }

        /// <summary>
        /// Get a double setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public double GetDouble(string key) { return Convert.ToDouble(Get(key)); }

        /// <summary>
        /// Get a string setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public string GetString(string key) { return Convert.ToString(Get(key)); }

        /// <summary>
        /// Get a dictionary setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public Dictionary<string, object> GetDict(string key) { return (Dictionary<string, object>) Get(key); }

        /// <summary>
        /// Get a object setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public object GetObject(string key) { return Get(key); }

        /// <summary>
        /// Get a boolean list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<bool> GetBoolList(string key)
        {
            return GetObjectList(key).Select(v => Convert.ToBoolean(v)).ToList();
        }

        /// <summary>
        /// Get a integer list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<int> GetIntList(string key)
        {
            return GetObjectList(key).Select(v => Convert.ToInt32(v)).ToList();
        }

        /// <summary>
        /// Get a float list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<float> GetFloatList(string key)
        {
            return GetObjectList(key).Select(v => Convert.ToSingle(v)).ToList();
        }

        /// <summary>
        /// Get a long list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<long> GetLongList(string key)
        {
            return GetObjectList(key).Select(v => Convert.ToInt64(v)).ToList();
        }

        /// <summary>
        /// Get a double list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<double> GetDoubleList(string key)
        {
            return GetObjectList(key).Select(v => Convert.ToDouble(v)).ToList();
        }

        /// <summary>
        /// Get a string list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<string> GetStringList(string key)
        {
            return GetObjectList(key).Select(v => Convert.ToString(v)).ToList();
        }

        /// <summary>
        /// Get a dictionary list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<Dictionary<string, object>> GetDictList(string key)
        {
            return GetObjectList(key).Select(v => (Dictionary<string, object>) v).ToList();
        }

        /// <summary>
        /// Get a object list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<object> GetObjectList(string key)
        {
            return (List<object>) Get(key);
        }

        /// <summary>
        /// Set a setting value.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        public void SetValue(string key, object value) { Set(key, value); }

        protected object Get(string key)
        {
            try
            {
                return baseDict[key];
            }
            catch (KeyNotFoundException)
            {
                if (parentSettings != null)
                {
                    return parentSettings.Get(key);
                }
                throw new KeyNotFoundException(string.Format("The key \"{0}\" was not found in the settings heirarchy. Use UXF Debugger to check your settings are being applied correctly.", key));
            }
        }

        protected void Set(string key, object value)
        {
            baseDict[key] = value;
        }
        
        /// <summary>
        /// Get a setting value. If it is not found, the request will cascade upwards in each parent setting until one is found. If one is never found, it will return null.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Obsolete("Accessing settings with [] is no longer recommended - use new methods GetInt, GetFloat, GetString, GetDict, GetObject, and SetValue instead.")]
        public object this[string key]
        {
            get { return Get(key); }
            set { baseDict[key] = value; }
        }

    }
}