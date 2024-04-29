/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonFilter
 * FILE:        CommonFilter/IFilter.cs
 * PURPOSE:     
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;

namespace CommonFilter
{
    public interface IFilter
    {
        void Start();

        bool CheckFilter(string input);

        event EventHandler FilterChanged;
    }
}
