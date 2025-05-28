using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ExtendedSystemObjects;

namespace CommonLibraryTests
{
    [TestClass]
    public class MemoryManagerTest
    {
        [TestMethod]
        public void TestBasicAllocationAndFree()
        {
            using var manager = new MemoryManager(1024);

            var handle1 = manager.Allocate(100);
            var handle2 = manager.Allocate(200);

            var ptr1 = manager.Resolve(handle1);
            var ptr2 = manager.Resolve(handle2);

            Assert.AreNotEqual(IntPtr.Zero, ptr1);
            Assert.AreNotEqual(IntPtr.Zero, ptr2);
            Assert.AreNotEqual(ptr1, ptr2);

            manager.Free(handle1);

            // Now that handle1 is freed, we should be able to reallocate that space
            var handle3 = manager.Allocate(100);
            var ptr3 = manager.Resolve(handle3);

            Assert.AreNotEqual(IntPtr.Zero, ptr3);
        }

        [TestMethod]
        public void TestCompactMovesBlocks()
        {
            using var manager = new MemoryManager(1024);

            var handle1 = manager.Allocate(100);
            var handle2 = manager.Allocate(200);
            var ptrBefore = manager.Resolve(handle2);

            manager.Free(handle1); // Create a hole before handle2

            manager.Compact(); // Compact should move handle2 to start of buffer

            var ptrAfter = manager.Resolve(handle2);
            Assert.AreNotEqual(ptrBefore, ptrAfter);
        }

        [TestMethod]
        public void TestOutOfMemoryThrows()
        {
            using var manager = new MemoryManager(128);
            manager.Allocate(100);

            Assert.ThrowsException<OutOfMemoryException>(() =>
            {
                manager.Allocate(50);
            });
        }

        [TestMethod]
        public void TestResolveThrowsOnInvalidHandle()
        {
            using var manager = new MemoryManager(128);
            var handle = manager.Allocate(64);
            manager.Free(handle);

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                manager.Resolve(handle);
            });
        }
    }
}
