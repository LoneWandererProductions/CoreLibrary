/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        TilePatch.cs
 * PURPOSE:     The Tile Path Record.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents;

/// <summary>
/// One tile of a raster patch: pixels before and after an edit (tight rows, clipped at the edges).
/// </summary>
/// <seealso cref="System.IEquatable&lt;Imaging.Objects.Documents.TilePatch&gt;" />
internal readonly record struct TilePatch(int Tx, int Ty, int Width, int Height, byte[] Before, byte[] After);
