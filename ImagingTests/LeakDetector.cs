using System.Diagnostics;

namespace ImagingTests
{
        public static class LeakDetector
        {
            // Switch this off in Production
            public static bool IsEnabled { get; set; } = true;

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
