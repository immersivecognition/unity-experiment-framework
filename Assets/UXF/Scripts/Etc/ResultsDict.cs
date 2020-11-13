using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace UXF
{
    /// <summary>
    /// Specialised class used to store Trial Results.
    /// </summary>
    public class ResultsDictionary
    {
        private Dictionary<string, object> baseDict;
        private bool allowAdHocAdding;

        /// <summary>
        /// Dictionary of results for a trial.
        /// </summary>
        /// <param name="initialKeys">Initial keys (e.g. headers) to add to dictionary</param>
        /// <param name="allowAdHocAdding">Should extra keys be allowed to be added ad-hoc?</param>
        public ResultsDictionary(IEnumerable<string> initialKeys, bool allowAdHocAdding)
        {
            baseDict = new Dictionary<string, object>();
            this.allowAdHocAdding = allowAdHocAdding;
            foreach (var key in initialKeys)
            {
                baseDict.Add(key, string.Empty);
            }
        }

        /// <summary>
        /// Access or set an observation
        /// </summary>
        /// <param name="key">Name (header) of the observation</param>
        /// <returns></returns>
        public object this[string key]
        {
            get { return baseDict[key]; }
            set {
                if (allowAdHocAdding || baseDict.ContainsKey(key))
                {
                    baseDict[key] = value;
                }
                else
                {
                    throw new KeyNotFoundException(string.Format("Custom header \"{0}\" does not exist!", key));
                }
            }
        }

        /// <summary>
        /// Returns the keys in this results dictionary.
        /// </summary>
        /// <value></value>
        public Dictionary<string, object>.KeyCollection Keys
        {
            get
            {
                return baseDict.Keys;
            }
        }

        /// <summary>
        /// Check if the results dictionary contains the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return baseDict.ContainsKey(key);
        }

    }

}