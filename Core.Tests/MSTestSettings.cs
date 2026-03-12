/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     UnknownNamespace
 * FILE:        MSTestSettings.cs
 * PURPOSE:     Basic MSTest settings for the test assembly. This file can be used to configure parallel test execution and other assembly-level settings for MSTest.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
