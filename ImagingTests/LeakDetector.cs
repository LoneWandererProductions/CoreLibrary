/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImagingTests
 * FILE:        LeakDetector.cs
 * PURPOSE:     Really basic memory leak detector for unit tests. It runs an action and checks if the memory usage increased significantly.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;

namespace ImagingTests
{
    /// <summary>
    /// Really basic memory leak detector for unit tests. It runs an action and checks if the memory usage increased significantly.
    /// </summary>
    public static class LeakDetector
        {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Monitors the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="label">The label.</param>
        /// <param name="thresholdBytes">The threshold bytes.</param>
        public static void Monitor(Action action, string label, long thresholdBytes = 1024 * 1024)
            {
                if (!IsEnabled)
                {
                    action();
                    return;
                }

                // Clean the slate
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                long startMem = Process.GetCurrentProcess().PrivateMemorySize64;

                action();

                // Clean up after action
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                long endMem = Process.GetCurrentProcess().PrivateMemorySize64;
                long diff = endMem - startMem;

                if (diff > thresholdBytes)
                {
                    string msg = $"[LEAK WARNING] {label}: Memory increased by {diff / 1024.0 / 1024.0:F2} MB";
                    Debug.WriteLine(msg);
                    // In a Unit Test environment, you might throw an exception here:
                    // throw new Exception(msg);
                }
                else
                {
                    Debug.WriteLine($"[OK] {label}: Memory stable (Diff: {diff} bytes).");
                }
            }
        }
}
