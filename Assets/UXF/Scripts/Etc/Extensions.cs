using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UXF
{
    /// <summary>
    /// Useful methods
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// Random number generator with seed based on current time.
        /// </summary>
        /// <returns></returns>
        private static System.Random rng = new System.Random();

        /// <summary>
        /// Clones a list and all items inside
        /// </summary>
        /// <param name="listToClone"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        /// <summary>
        /// Modify a string to remove any unsafe characters
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
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
        /// Swaps the order of the elements at indeces `indexA` and `indexB` within `list`
        /// </summary>
        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
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

        public static string ToLower(this UXFDataType dataType)
        {
            return dataType.ToString().ToLower();
        }

        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(
                  this IEnumerable<TSource> source, int size)
        {
            TSource[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new TSource[size];

                bucket[count++] = item;
                if (count != size)
                    continue;

                yield return bucket;

                bucket = null;
                count = 0;
            }

            if (bucket != null && count > 0)
                yield return bucket.Take(count).ToArray();
        }

    }

    [System.Serializable]
    public class StringEvent : UnityEngine.Events.UnityEvent<string> 
    {

    }

}