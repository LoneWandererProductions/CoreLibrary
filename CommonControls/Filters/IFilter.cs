/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/Filters/IFilter.cs
 * PURPOSE:     
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;

namespace CommonControls.Filters
{
    public interface IFilter
    {
        void Start();

        bool CheckFilter(string input);

        event EventHandler FilterChanged;
    }
}
