/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/Utility.cs
 * PURPOSE:     Some Methods I seem to use very often. Might add a better way to search the keys!
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects
{
    /// <summary>
    ///     The utility class.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        ///     Get the first available index.
        ///     Only usable for positive int Values
        ///     Thread Safe
        /// </summary>
        /// <param name="lst">The List.</param>
        /// <returns>The first available Index<see cref="int" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFirstAvailableIndex(List<int> lst)
        {
            ArgumentNullException.ThrowIfNull(lst);

            lock (lst)
            {
                if (lst.Count == 0) return 0;

                // Sort a copy to find the gap efficiently
                var sorted = lst.ToList();
                sorted.Sort();

                // If the first element isn't 0, 0 is the first gap
                if (sorted[0] > 0) return 0;

                for (var i = 0; i < sorted.Count - 1; i++)
                {
                    // If there is a gap between two consecutive numbers, return the gap
                    if (sorted[i + 1] > sorted[i] + 1)
                    {
                        return sorted[i] + 1;
                    }
                }

                // No gaps found, return the next number after the max
                return sorted[^1] + 1;
            }
        }

        /// <summary>
        ///     Get the first available index.
        ///     Only usable for positive long Values
        ///     Thread Safe
        /// </summary>
        /// <param name="lst">The List.</param>
        /// <returns>The first available Index<see cref="long" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetFirstAvailableIndex(List<long> lst)
        {
            ArgumentNullException.ThrowIfNull(lst);

            lock (lst)
            {
                if (lst.Count == 0) return 0;

                // Sort a copy to find the gap efficiently
                var sorted = lst.ToList();
                sorted.Sort();

                // If the first element isn't 0, 0 is the first gap
                if (sorted[0] > 0) return 0;

                for (var i = 0; i < sorted.Count - 1; i++)
                {
                    // If there is a gap between two consecutive numbers, return the gap
                    if (sorted[i + 1] > sorted[i] + 1)
                    {
                        return sorted[i] + 1;
                    }
                }

                // No gaps found, return the next number after the max
                return sorted[^1] + 1;
            }
        }
        /// <summary>
        ///     Performs binary search on a sorted span of integers.
        /// </summary>
        /// <param name="sortedKeys">The sorted span of integers.</param>
        /// <param name="target">The value to search for.</param>
        /// <returns>
        ///     Index of the element if found; otherwise, the bitwise complement of the insertion index.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch(ReadOnlySpan<int> sortedKeys, int target)
        {
            return BinarySearch(sortedKeys, sortedKeys.Length, target);
        }



        /// <summary>
        ///     Internal binary search method using a specified count of elements.
        /// </summary>
        /// <param name="sortedKeys">The sorted span of integers.</param>
        /// <param name="count">The number of elements to consider from the start of the span.</param>
        /// <param name="target">The value to search for.</param>
        /// <returns>
        ///     Index of the element if found; otherwise, the bitwise complement of the insertion index.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int BinarySearch(ReadOnlySpan<int> sortedKeys, int count, int target)
        {
            int left = 0, right = count - 1;

            while (left <= right)
            {
                var mid = left + ((right - left) >> 1);
                var midKey = sortedKeys[mid];

                if (midKey == target) return mid;

                if (midKey < target) left = mid + 1;
                else right = mid - 1;
            }

            return ~left;
        }

        /// <summary>
        ///     Gets the next element.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="lst">The LST.</param>
        /// <returns>Next Element</returns>
        public static int GetNextElement(int position, List<int> lst)
        {
            if (lst == null || lst.Count == 0) return -1;

            lock (lst)
            {
                var index = lst.IndexOf(position);
                if (index == -1 || index == lst.Count - 1) return lst[0];
                return lst[index + 1];
            }
        }

        /// <summary>
        ///     Gets the previous element.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="lst">The LST.</param>
        /// <returns>Previous Element</returns>
        public static int GetPreviousElement(int position, List<int> lst)
        {
            if (lst == null || lst.Count == 0) return -1;

            lock (lst)
            {
                var index = lst.IndexOf(position);
                if (index == -1 || index == 0) return lst[^1];
                return lst[index - 1];
            }
        }

        /// <summary>
        ///     Gets the index of the available indexes.
        ///     Only usable for positive int Values
        /// </summary>
        /// <param name="lst">The List.</param>
        /// <param name="count">The count of keys we need.</param>
        /// <returns>A list of keys we can use</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static List<int> GetAvailableIndexes(List<int> lst, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count),
                    SharedResources.ErrorValueNotAllowed);
            }

            var keys = new List<int>();
            for (var i = 0; i < count; i++)
            {
                var key = GetFirstAvailableIndex(lst);
                lst.Add(key);
                keys.Add(key);
            }

            return keys;
        }

        /// <summary>
        ///     Sequencers the specified set.
        /// </summary>
        /// <param name="set">The set.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns>List of Sequences, with start and end index, null if none were found.</returns>
        public static List<KeyValuePair<int, int>>? Sequencer(SortedSet<int> set, int sequence)
        {
            var sequenceGroups = new List<List<int>>();
            var currentSequence = new List<int>();

            var sortedList = new List<int>(set);

            for (var i = 1; i < sortedList.Count; i++)
            {
                var cache = Math.Abs(sortedList[i]);

                if (Math.Abs(sortedList[i - 1] + 1) == cache)
                {
                    //should be only the first case
                    if (!currentSequence.Contains(i - 1))
                    {
                        currentSequence.Add(i - 1);
                    }

                    currentSequence.Add(i);
                }
                else
                {
                    if (currentSequence.Count == 0)
                    {
                        continue;
                    }

                    sequenceGroups.Add(currentSequence);
                    currentSequence = new List<int>();
                }
            }

            return sequenceGroups.Count == 0
                ? null
                : (from stack in sequenceGroups
                    where stack.Count >= sequence
                    let start = stack[0]
                    let end = stack[^1]
                    select new KeyValuePair<int, int>(start, end)).ToList();
        }

        /// <summary>
        ///     Sequencers the specified List.
        /// </summary>
        /// <param name="numbers">The input list.</param>
        /// <param name="sequenceLength">The min count of the sequence.</param>
        /// <returns>List of Sequences, with start and end index, null if none were found.</returns>
        public static List<KeyValuePair<int, int>>? Sequencer(List<int> numbers, int sequenceLength)
        {
            if (numbers == null || numbers.Count < sequenceLength || sequenceLength <= 1)
            {
                return null;
            }

            var results = new List<KeyValuePair<int, int>>();
            int startValue = numbers[0];
            int count = 1;

            // Logic uses a single pass O(N) approach
            for (var i = 1; i < numbers.Count; i++)
            {
                // We use Math.Abs to mirror your original logic for negative handling
                if (Math.Abs(numbers[i - 1] + 1) == Math.Abs(numbers[i]))
                {
                    count++;
                }
                else
                {
                    if (count >= sequenceLength)
                    {
                        results.Add(new KeyValuePair<int, int>(startValue, numbers[i - 1]));
                    }

                    startValue = numbers[i];
                    count = 1;
                }
            }

            // Catch the trailing sequence
            if (count >= sequenceLength)
            {
                results.Add(new KeyValuePair<int, int>(startValue, numbers[^1]));
            }

            return results.Count == 0 ? null : results;
        }

        /// <summary>
        ///     Sequences the specified list.
        /// </summary>
        /// <param name="numbers">The list.</param>
        /// <param name="stepWidth">The step width.</param>
        /// <param name="sequenceLength">The sequence.</param>
        /// <returns>List of Sequences, with start and end index, null if none were found.</returns>
        public static List<KeyValuePair<int, int>>? Sequencer(List<int> numbers, int stepWidth, int sequenceLength)
        {
            if (numbers == null || numbers.Count < sequenceLength || sequenceLength <= 1)
            {
                return null;
            }

            // We work on a sorted copy to ensure linear building
            var sorted = new List<int>(numbers);
            sorted.Sort();

            var numberSet = new HashSet<int>(sorted);
            var result = new List<KeyValuePair<int, int>>();
            var visited = new HashSet<int>();

            foreach (var num in sorted)
            {
                if (visited.Contains(num)) continue;

                var current = num;
                var streak = 1;

                while (numberSet.Contains(current + stepWidth))
                {
                    current += stepWidth;
                    streak++;
                }

                if (streak >= sequenceLength)
                {
                    result.Add(new KeyValuePair<int, int>(num, current));

                    for (var val = num; val <= current; val += stepWidth)
                    {
                        visited.Add(val);
                    }
                }
            }

            return result.Count == 0 ? null : result;
        }

        /// <summary>
        ///     Finds the sequences.
        ///     Looks for consecutive numbers in a sequence.
        /// </summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>Return the start, end, and the repeated value of that streak.</returns>
        public static List<(int start, int end, int value)> FindSequences(List<int> numbers)
        {
            var result = new List<(int start, int end, int value)>();
            if (numbers == null || numbers.Count == 0) return result;
            int start = 0;
            for (int i = 1; i <= numbers.Count; i++)
            {
                if (i == numbers.Count || numbers[i] != numbers[start])
                {
                    result.Add((start, i - 1, numbers[start]));
                    start = i;
                }
            }
            return result;
        }
    }
}
