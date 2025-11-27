/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver;
 * FILE:        CommandSignature.cs
 * PURPOSE:     Record for all my command signatures.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver;

/// <summary>
/// Represents a unique command signature consisting of namespace, name, and parameter count.
/// </summary>
/// <param name="Namespace">The namespace of the command.</param>
/// <param name="Name">The name of the command.</param>
/// <param name="ParameterCount">The number of parameters the command expects.</param>
public record CommandSignature(string Namespace, string Name, int ParameterCount);