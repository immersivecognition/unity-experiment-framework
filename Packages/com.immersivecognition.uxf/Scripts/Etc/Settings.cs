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

        ISettingsContainer parentSettingsContainer;
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
        /// Creates new empty Settings instance
        /// </summary>
        /// <param name="dict"></param>
        public Settings()
        {
            baseDict = new Dictionary<string, object>();
        }

        /// <summary>
        /// Add all the keys and values from `dict` to the settings.
        /// </summary>
        /// <param name="dict">Dictionary to add.</param>
        public void UpdateWithDict(Dictionary<string, object> dict)
        {
            // add all keys to new dictionary
            dict
                .ToList()
                .ForEach(x => baseDict[x.Key] = x.Value);
        }

        /// <summary>
        /// Sets the parent setting object, which is accessed when a setting is not found in the dictionary.
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(ISettingsContainer parent)
        {
            parentSettingsContainer = parent;
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
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<bool> GetBoolList(string key)
        {
            try
            {
                return GetObjectList(key).Select(v => Convert.ToBoolean(v)).ToList();
            }
            catch (InvalidCastException)
            {
                return (List<bool>) Get(key);
            }
        }

        /// <summary>
        /// Get a integer list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<int> GetIntList(string key)
        {
            try
            {
                return GetObjectList(key).Select(v => Convert.ToInt32(v)).ToList();
            }
            catch (InvalidCastException)
            {
                return (List<int>) Get(key);
            }
        }

        /// <summary>
        /// Get a float list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<float> GetFloatList(string key)
        {
            try
            {
                return GetObjectList(key).Select(v => Convert.ToSingle(v)).ToList();
            }
            catch (InvalidCastException)
            {
                return (List<float>) Get(key);
            }
        }

        /// <summary>
        /// Get a long list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<long> GetLongList(string key)
        {
            try
            {
                return GetObjectList(key).Select(v => Convert.ToInt64(v)).ToList();
            }
            catch (InvalidCastException)
            {
                return (List<long>) Get(key);
            }
        }

        /// <summary>
        /// Get a double list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<double> GetDoubleList(string key)
        {
            try
            {
                return GetObjectList(key).Select(v => Convert.ToDouble(v)).ToList();
            }
            catch (InvalidCastException)
            {
                return (List<double>) Get(key);
            }
        }

        /// <summary>
        /// Get a string list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<string> GetStringList(string key)
        {
            try
            {
                return GetObjectList(key).Select(v => Convert.ToString(v)).ToList();
            }
            catch (InvalidCastException)
            {
                return (List<string>) Get(key);
            }
        }

        /// <summary>
        /// Get a Dictionary<string, object> list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        public List<Dictionary<string, object>> GetDictList(string key)
        {
            try
            {
                return GetObjectList(key).Select(v => (Dictionary<string, object>) v).ToList();
            }
            catch (InvalidCastException)
            {
                return (List<Dictionary<string, object>>) Get(key);
            }
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
        /// Get a boolean setting value. If it is not found, the request will cascade upwards in each parent setting until one is found. Returns valueIfNotFound if the key is not found in the settings.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public bool GetBool(string key, bool valueIfNotFound) { return ContainsKey(key) ? GetBool(key) : valueIfNotFound; }

        /// <summary>
        /// Get a integer setting value. If it is not found, the request will cascade upwards in each parent setting until one is found. Returns valueIfNotFound if the key is not found in the settings.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public int GetInt(string key, int valueIfNotFound) { return ContainsKey(key) ? GetInt(key) : valueIfNotFound; }

        /// <summary>
        /// Get a float setting value. If it is not found, the request will cascade upwards in each parent setting until one is found. Returns valueIfNotFound if the key is not found in the settings.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public float GetFloat(string key, float valueIfNotFound) { return ContainsKey(key) ? GetFloat(key) : valueIfNotFound; }

        /// <summary>
        /// Get a long setting value. If it is not found, the request will cascade upwards in each parent setting until one is found. Returns valueIfNotFound if the key is not found in the settings.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public long GetLong(string key, long valueIfNotFound) { return ContainsKey(key) ? GetLong(key) : valueIfNotFound; }

        /// <summary>
        /// Get a double setting value. If it is not found, the request will cascade upwards in each parent setting until one is found. Returns valueIfNotFound if the key is not found in the settings.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public double GetDouble(string key, double valueIfNotFound) { return ContainsKey(key) ? GetDouble(key) : valueIfNotFound; } 

        /// <summary>
        /// Get a string setting value. If it is not found, the request will cascade upwards in each parent setting until one is found. Returns valueIfNotFound if the key is not found in the settings.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public string GetString(string key, string valueIfNotFound) { return ContainsKey(key) ? GetString(key) : valueIfNotFound; }

        /// <summary>
        /// Get a dictionary setting value. If it is not found, the request will cascade upwards in each parent setting until one is found. Returns valueIfNotFound if the key is not found in the settings.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public Dictionary<string, object> GetDict(string key, Dictionary<string, object> valueIfNotFound) { return ContainsKey(key) ? GetDict(key) : valueIfNotFound;  }

        /// <summary>
        /// Get a object setting value. If it is not found, the request will cascade upwards in each parent setting until one is found. Returns valueIfNotFound if the key is not found in the settings.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public object GetObject(string key, object valueIfNotFound) { return ContainsKey(key) ? GetObject(key) : valueIfNotFound;  }

        /// <summary>
        /// Get a boolean list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found. Returns valueIfNotFound if the key is not found in the settings.
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public List<bool> GetBoolList(string key, List<bool> valueIfNotFound) { return ContainsKey(key) ? GetBoolList(key) : valueIfNotFound; }

        /// <summary>
        /// Get a integer list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public List<int> GetIntList(string key, List<int> valueIfNotFound) { return ContainsKey(key) ? GetIntList(key) : valueIfNotFound; }

        /// <summary>
        /// Get a float list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public List<float> GetFloatList(string key, List<float> valueIfNotFound) { return ContainsKey(key) ? GetFloatList(key) : valueIfNotFound; }

        /// <summary>
        /// Get a long list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public List<long> GetLongList(string key, List<long> valueIfNotFound) { return ContainsKey(key) ? GetLongList(key) : valueIfNotFound; }

        /// <summary>
        /// Get a double list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public List<double> GetDoubleList(string key, List<double> valueIfNotFound) { return ContainsKey(key) ? GetDoubleList(key) : valueIfNotFound; }

        /// <summary>
        /// Get a string list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public List<string> GetStringList(string key, List<string> valueIfNotFound) { return ContainsKey(key) ? GetStringList(key) : valueIfNotFound; }

        /// <summary>
        /// Get a Dictionary<string, object> list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// If the setting references a setting stored in the settings json file, a copy of the list will be returned. If it is a setting created with settings.SetValue(...), the original reference will be returned. 
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public List<Dictionary<string, object>> GetDictList(string key, List<Dictionary<string, object>> valueIfNotFound) { return ContainsKey(key) ? GetDictList(key) : valueIfNotFound; }

        /// <summary>
        /// Get a object list setting value. If it is not found, the request will cascade upwards in each parent setting until one is found.
        /// </summary>
        /// <param name="key">The key (name) of the setting.</param>
        /// <param name="valueIfNotFound">The value returned if the setting does not exist (i.e., a default value).</param>
        public List<object> GetObjectList(string key, List<object> valueIfNotFound) { return ContainsKey(key) ? GetObjectList(key) : valueIfNotFound; }

        
        public bool ContainsKey(string key)
        {
            if (baseDict.ContainsKey(key))
            {
                return true;
            }
            else
            {
                if (parentSettingsContainer != null && parentSettingsContainer.settings != null)
                {
                    return parentSettingsContainer.settings.ContainsKey(key);
                }
                else
                {
                    return false;
                }
            }
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
                if (parentSettingsContainer != null && parentSettingsContainer.settings != null)
                {
                    return parentSettingsContainer.settings.Get(key);
                }
                throw new KeyNotFoundException(
                    string.Format(
                        "The key \"{0}\" was not found in the settings heirarchy. "
                         + "Use UXF Session Debugger (UXF menu at top of unity editor) "
                         + "to check your settings are being applied correctly.",
                         key
                    )
                );
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