/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        FastLinq.cs
 * PURPOSE:     Allocation-free, high-performance LINQ-like helpers for spans.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal

using System;

namespace ExtendedSystemObjects
{
    /// <summary>
    /// Provides allocation-free, high-performance LINQ-like extension methods for <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/>.
    /// Designed for tight loops and performance-critical code where standard LINQ or <see cref="foreach"/> may introduce heap allocations.
    /// </summary>
    public static class FastLinq
    {
        /// <summary>
        /// Executes the specified action for each element in the given read-only span.
        /// Zero allocations, fully inlineable.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The span to iterate over.</param>
        /// <param name="action">The action to execute on each element.</param>
        public static void ForEachFast<T>(this ReadOnlySpan<T> span, Action<T> action)
        {
            foreach (var t in span)
                action(t);
        }

        /// <summary>
        /// Executes the specified action for each element in the given writable span, passing elements by value.
        /// Zero allocations, fully inlineable.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The span to iterate over.</param>
        /// <param name="action">The action to execute on each element.</param>
        public static void ForEachFast<T>(this Span<T> span, Action<T> action)
        {
            foreach (var t in span)
                action(t);
        }

        /// <summary>
        /// Projects each element of a read-only span into a destination span without allocations.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the source span.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the destination span.</typeparam>
        /// <param name="span">The source span to project from.</param>
        /// <param name="destination">The destination span to write projected elements to.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <exception cref="ArgumentException">Thrown if the destination span is smaller than the source span.</exception>
        public static void SelectFast<TSource, TResult>(
            this ReadOnlySpan<TSource> span,
            Span<TResult> destination,
            Func<TSource, TResult> selector)
        {
            if (destination.Length < span.Length)
                throw new ArgumentException("Destination span is too small.");

            for (int i = 0; i < span.Length; i++)
                destination[i] = selector(span[i]);
        }

        /// <summary>
        /// Filters elements in a read-only span into a destination span based on a predicate.
        /// Returns the number of elements written to the destination span.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="span">The source span to filter.</param>
        /// <param name="destination">The destination span to receive filtered elements.</param>
        /// <param name="predicate">A function to test each element for inclusion.</param>
        /// <returns>The number of elements written to the destination span.</returns>
        public static int WhereFast<T>(this ReadOnlySpan<T> span, Span<T> destination, Func<T, bool> predicate)
        {
            int count = 0;
            int destLength = destination.Length; // Cache length to help JIT

            foreach (var t in span)
            {
                if (predicate(t))
                {
                    if ((uint)count < (uint)destLength) // Use uint cast for faster bounds check
                    {
                        destination[count++] = t;
                    }
                    else
                    {
                        throw new ArgumentException("Destination span is too small.");
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Aggregates a value across all elements of a read-only span.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the span.</typeparam>
        /// <typeparam name="TResult">The type of the accumulated result.</typeparam>
        /// <param name="span">The source span to aggregate.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An aggregation function that takes the current accumulator and an element and returns the new accumulator.</param>
        /// <returns>The final accumulated value.</returns>
        public static TResult AggregateFast<T, TResult>(
            this ReadOnlySpan<T> span,
            TResult seed,
            Func<TResult, T, TResult> func)
        {
            var acc = seed;
            foreach (var t in span)
                acc = func(acc, t);

            return acc;
        }

        /// <summary>
        /// Returns true if the collection has at least one element.
        /// Zero allocations and works on any collection type.
        /// </summary>
        /// <typeparam name="T">The element type of the collection.</typeparam>
        /// <param name="span">The source span to check.</param>
        /// <returns>True if the collection contains at least one element; otherwise false.</returns>
        public static bool AnyFast<T>(this ReadOnlySpan<T> span)
        {
            return span.Length != 0;
        }

        /// <summary>
        /// Returns true if all elements in the collection satisfy the given predicate.
        /// Zero allocations and works on any collection type.
        /// </summary>
        /// <typeparam name="T">The element type of the collection.</typeparam>
        /// <param name="span">The source span to check.</param>
        /// <param name="predicate">A function to test each element.</param>
        /// <returns>True if all elements satisfy the predicate; otherwise false.</returns>
        public static bool AllFast<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            foreach (var t in span)
                if (!predicate(t))
                    return false;

            return true;
        }

        /// <summary>
        /// Returns the number of elements in the collection.
        /// Zero allocations and works on any collection type.
        /// </summary>
        /// <typeparam name="T">The element type of the collection.</typeparam>
        /// <param name="span">The source span to count.</param>
        /// <returns>The number of elements in the collection.</returns>
        public static int CountFast<T>(this ReadOnlySpan<T> span)
        {
            return span.Length;
        }
    }
}
