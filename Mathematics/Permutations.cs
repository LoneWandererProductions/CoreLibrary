using System;
using System.Collections.Generic;

namespace Mathematics
{
    public static class Permutations
    {
        public static IEnumerable<IEnumerable<T>> GetCombination<T>(List<T> list)
        {
            var count = Math.Pow(2, list.Count);
            var lst = new List<List<T>>();

            for (var i = 1; i <= count - 1; i++)
            {
                var str = Convert.ToString(i, 2).PadLeft(list.Count, '0');
                var cache = new List<T>();

                for (var j = 0; j < str.Length; j++)
                {
                    if (str[j] == '1')
                    {
                        cache.Add(list[j]);
                    }
                }

                lst.Add(cache);
            }

            return lst;
        }

        /// <summary>
        /// Combinations with repetition.
        /// https://stackoverflow.com/questions/25824376/combinations-with-repetitions-c-sharp
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input.</param>
        /// <param name="length">The length.</param>
        /// <returns>All combined SubElements</returns>
        public static IEnumerable<string> CombinationsWithRepetition<T>(this IEnumerable<T> input, int length)
        {
            if (length <= 0)
            {
                yield return string.Empty;
            }
            else
            {
                foreach (var i in input)
                foreach (var c in CombinationsWithRepetition(input, length - 1))
                    yield return i.ToString() + c;
            }
        }
    }
}
