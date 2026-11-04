/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        MapChangeResult.cs
 * PURPOSE:     Record that holds the result of a map change operation.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace Solaris
{
    /// <inheritdoc />
    /// <summary>
    ///     Return type for map manipulation methods.
    /// </summary>
    internal readonly record struct MapChangeResult(bool Changed, Dictionary<int, List<int>>? Map);
}
