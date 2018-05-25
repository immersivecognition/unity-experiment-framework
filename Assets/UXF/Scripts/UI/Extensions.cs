﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UXF
{

    static class Extensions
    {

        /// <summary>
        /// Random number generator with seed based on current time.
        /// </summary>
        /// <returns></returns>
        private static System.Random rng = new System.Random();

        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static float ToFloat(this object obj)
        {
            return System.Convert.ToSingle(obj);
        }

        public static int ToInt(this object obj)
        {
            return System.Convert.ToInt32(obj);
        }


        public static string GetSafeFilename(string filename)
        {
            return string.Join("", filename.Split(Path.GetInvalidFileNameChars()));
        }

        /// <summary>
        /// Shuffles a list in-place with a given random number generator.
        /// </summary>
        /// <param name="list">List to shuffle</param>
        /// <param name="rng">Random number generator via which the shuffling occurs</param>
        public static void Shuffle<T>(this IList<T> list, System.Random rng)
        {

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Shuffles a list in-place with the current time based random number generator. 
        /// </summary>
        /// <param name="list">List to shuffle</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            list.Shuffle(rng);
        }


        /// <summary>
        /// Combine many path parts into a single path.
        /// </summary>
        /// <param name="path1">Base path</param>
        /// <param name="paths">Array of subsequent paths</param>
        /// <returns></returns>
        public static string CombinePaths(string path1, params string[] paths)
        {
            if (path1 == null)
            {
                throw new ArgumentNullException("path1");
            }
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            return paths.Aggregate(path1, (acc, p) => Path.Combine(acc, p));
        }

    }

}