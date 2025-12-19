/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommunicationTests
 * FILE:        ComTransportTests.cs
 * PURPOSE:     Basic tests for ComTransport
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Communication;

namespace CommunicationTests
{
    /// <summary>
    /// Com transport tests
    /// </summary>
    [TestClass]
    public class ComTransportTests
    {
        /// <summary>
        /// Writes the should raise data received.
        /// </summary>
        [TestMethod]
        public async Task Write_ShouldRaise_DataReceived()
        {
            var fakePort = new FakeSerialPort();
            var transport = new ComTransport(fakePort);

            fakePort.Open();

            var task = transport.ReadOnceAsync(
                TimeSpan.FromMilliseconds(100));

            transport.Write("PING");

            var result = await task;

            Assert.AreEqual("PING", result);
        }
    }
}
