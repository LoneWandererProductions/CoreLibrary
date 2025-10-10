/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/ImageDuplicate.cs
 * PURPOSE:     Struct to Compare if Images are Duplicates
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using ExtendedSystemObjects;

namespace ImageCompare;

/// <inheritdoc />
/// <summary>
///     Struct for checking duplicate Images
/// </summary>
/// <seealso cref="T:System.IComparable`1" />
[DebuggerDisplay(
    "R = {R}, G = {G}, B = {B}, Id = {Id}")]
internal readonly struct ImageDuplicate : IComparable<ImageDuplicate>
{
    /// <summary>
    ///     Determines whether the specified <see cref="object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
    /// <returns>
    ///     <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
        return obj is ImageDuplicate other && Equals(other);
    }

    /// <summary>
    ///     Gets the image Color Values.
    /// </summary>
    /// <value>
    ///     The image.
    /// </value>
    internal byte[,] Image { get; init; }

    /// <summary>
    ///     Gets the average Red Values.
    /// </summary>
    /// <value>
    ///     The red.
    /// </value>
    internal int R { get; init; }

    /// <summary>
    ///     Gets the average blue Values.
    /// </summary>
    /// <value>
    ///     The blue.
    /// </value>
    internal int B { get; init; }

    /// <summary>
    ///     Gets the average Green Values.
    /// </summary>
    /// <value>
    ///     The green.
    /// </value>
    internal int G { get; init; }

    /// <summary>
    ///     Gets or sets the identifier.
    /// </summary>
    /// <value>
    ///     The identifier.
    /// </value>
    internal int Id { get; init; }

    /// <summary>
    ///     Checks if DupImage is equal to another
    ///     First we check basic outlines and second we check the Color values
    /// </summary>
    /// <param name="other">Compares it to another DupImage Object</param>
    /// <returns>
    ///     True if this Object is equal to <paramref name="other" /> else false"/>.
    /// </returns>
    public bool Equals(ImageDuplicate other)
    {
        if (Image == null || other.Image == null)
        {
            return false;
        }

        for (var y = 0; y < ImageResources.DuplicateSize; y++)
            for (var x = 0; x < ImageResources.DuplicateSize; x++)
            {
                var comparisonResult = Image[x, y].CompareTo(other.Image[x, y]);
                if (comparisonResult != 0)
                {
                    return false;
                }
            }

        return other.R.Interval(R, ImageResources.ColorThreshold) &&
               other.G.Interval(G, ImageResources.ColorThreshold) &&
               other.B.Interval(B, ImageResources.ColorThreshold);
    }

    /// <inheritdoc />
    /// <summary>
    ///     Compares the specified arrays.
    /// </summary>
    /// <param name="other">Object we compare to</param>
    /// <returns>
    ///     Compare result to <paramref name="other" />.
    /// </returns>
    public int CompareTo(ImageDuplicate other)
    {
        if (Image == null)
        {
            return 0;
        }

        for (var i = 0; i < ImageResources.DuplicateSize; i++)
            for (var j = 0; j < ImageResources.DuplicateSize; j++)
            {
                var comparisonResult = Image[i, j].CompareTo(other.Image[i, j]);
                if (comparisonResult != 0)
                {
                    return comparisonResult;
                }
            }

        return 0;
    }

    /// <summary>
    ///     Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Image);
    }

    /// <summary>
    ///     Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    ///     The result of the operator.
    /// </returns>
    public static bool operator ==(ImageDuplicate left, ImageDuplicate right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    ///     The result of the operator.
    /// </returns>
    public static bool operator !=(ImageDuplicate left, ImageDuplicate right)
    {
        return !(left == right);
    }
}
