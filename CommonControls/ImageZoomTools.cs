/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/ImageZoomTools.xaml.cs
 * PURPOSE:     Image View Control Tools, that are exposed to the Outside
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable EventNeverSubscribedTo.Global, only used outside of the dll
// ReSharper disable MemberCanBeInternal, must be visible, if we want to use it outside of the dll
// ReSharper disable UnusedType.Global

namespace CommonControls;

/// <summary>
///     The possible Selection Tools
/// </summary>
/// <summary>
///     The possible Selection Tools
/// </summary>
public enum ImageZoomTools
{
    /// <summary>
    ///     The move tool
    /// </summary>
    Move = 0,

    /// <summary>
    ///     The select rectangle tool
    /// </summary>
    Rectangle = 1,

    /// <summary>
    ///     The select trace tool
    /// </summary>
    Trace = 2,

    /// <summary>
    ///     The select Dot tool
    /// </summary>
    Dot = 3,

    /// <summary>
    ///     The select ellipse tool
    /// </summary>
    Ellipse = 4,

    /// <summary>
    ///     The free form selection tool
    /// </summary>
    FreeForm = 5
}
