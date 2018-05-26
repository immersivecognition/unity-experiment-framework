using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

/// <summary>
/// Class used to store trial results. Is a dictionary that stores its entries in a given order.
/// </summary>
public class OrderedResultDict : OrderedDictionary
{
    /// <summary>
    /// Access or set an observation
    /// </summary>
    /// <param name="key">Name (header) of the observation</param>
    /// <returns></returns>
    public object this[string key]
    {
        get { return base[key]; }
        set {
            if (base.Contains(key))
            {
                base[key] = value;
            }
            else
            {
                throw new KeyNotFoundException(string.Format("Custom header \"{0}\" does not exist!", key));
            }
        }
    }
}

