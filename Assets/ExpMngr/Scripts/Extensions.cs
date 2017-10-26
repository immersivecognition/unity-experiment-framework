using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

static class Extensions
{
    public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
    {
        return listToClone.Select(item => (T)item.Clone()).ToList();
    }

    public static string GetSafeFilename(string filename)
    {
        return string.Join("", filename.Split(Path.GetInvalidFileNameChars()));
    }
}
