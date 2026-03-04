/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommunicationTests
 * FILE:        SimpleLogServerTests.cs
 * PURPOSE:     Simple Log Server Tests
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Net.Sockets;
using System.Text;
using Communication;
using Communication.Interfaces;

namespace CommunicationTests
{
    [TestClass]
    public class SimpleLogServerTests
    {
        // Use a random high port to avoid conflicts
        private const int TestPort = 55123;

        /// <summary>
        /// Actions the constructor should receive and process message.
        /// </summary>
        [TestMethod]
        public async Task ActionConstructor_ShouldReceiveAndProcessMessage()
        {
            // 1. Setup a TaskCompletionSource. This acts as our "Flag".
            //    When the server gets a message, it sets this flag.
            var messageReceivedSignal =
                new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            using var server = new SimpleLogServer(TestPort, (msg) =>
            {
                // Verify we aren't setting it twice
                messageReceivedSignal.TrySetResult(msg);
            });
            // 2. Start the Server
            server.Start();

            // 3. Send the message with a "Retry Policy"
            //    We try to connect 5 times. If the server is slow to start, this handles it.
            bool connected = await TrySendTcpMessageAsync(TestPort, "Hello World!");
            Assert.IsTrue(connected, "Could not connect to the server. It might not have started in time.");

            // 4. Wait for the result with a TIMEOUT
            //    If the signal isn't set in 2 seconds, we force a failure.
            //    This prevents the "Endless Run".
            var completedTask = await Task.WhenAny(messageReceivedSignal.Task, Task.Delay(2000));

            if (completedTask != messageReceivedSignal.Task)
            {
                Assert.Fail("Test Timed Out: Server accepted connection but never processed the message.");
            }

            Assert.AreEqual("Hello World!", messageReceivedSignal.Task.Result);
        }

        /// <summary>
        /// Tries the send TCP message asynchronous.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="message">The message.</param>
        /// <returns>Message Status.</returns>
        private async Task<bool> TrySendTcpMessageAsync(int port, string message)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    using var client = new TcpClient();
                    // Try to connect
                    await client.ConnectAsync("127.0.0.1", port);

                    await using var stream = client.GetStream();
                    // Important: Add NewLine (\n) so ReadLineAsync fires!
                    byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);
                    await stream.WriteAsync(data, 0, data.Length);
                    await stream.FlushAsync();

                    return true; // Success!
                }
                catch (SocketException)
                {
                    // Server not ready yet? Wait 100ms and try again.
                    await Task.Delay(100);
                }
            }

            return false; // Failed after 5 attempts
        }

        /// <summary>
        /// Interfaces the constructor should use processor class.
        /// </summary>
        [TestMethod]
        public async Task InterfaceConstructor_ShouldUseProcessorClass()
        {
            // Arrange
            var fakeProcessor = new FakeLogProcessor();
            var port = TestPort + 1;

            // Create server using the INTERFACE constructor
            using var server = new SimpleLogServer(port, fakeProcessor);
            // Act
            server.Start();

            // FIX: Use 'TrySendTcpMessageAsync' (Retry Logic) instead of 'SendTcpMessageAsync'
            // We also check the boolean result to ensure connection happened.
            bool connected = await TrySendTcpMessageAsync(port, "Hello from Class!");
            Assert.IsTrue(connected, "Could not connect to the server (Timeout).");

            // Wait for the processor to get the message (Polling with timeout)
            string received = null;
            for (var i = 0; i < 20; i++) // Try for 2 seconds (20 * 100ms)
            {
                if (fakeProcessor.LastMessage != null)
                {
                    received = fakeProcessor.LastMessage;
                    break;
                }

                await Task.Delay(100);
            }

            // Assert
            Assert.IsNotNull(received, "Message was never received by the processor.");
            Assert.AreEqual("Hello from Class!", received);
        }

        /// <summary>
        /// Servers the should disconnect slow clients.
        /// </summary>
        [TestMethod]
        public async Task Server_ShouldDisconnect_SlowClients()
        {
            // Arrange
            int port = 55150;
            using var server = new SimpleLogServer(port, msg => { });
            server.Start();
            await Task.Delay(100);

            using var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", port);
            await using var stream = client.GetStream();
            // Act: Send data WITHOUT a newline
            byte[] data = Encoding.UTF8.GetBytes("I am a bad client...");
            await stream.WriteAsync(data, 0, data.Length);

            // Don't send \n. Just wait.
            // The server should cut us off after 5 seconds.

            // Assert: Try to read from the stream. 
            // If the server disconnects us, ReadAsync returns 0.
            byte[] buffer = new byte[10];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            // If bytesRead is 0, it means the server closed the connection.
            Assert.AreEqual(0, bytesRead, "Server should have closed the connection due to timeout.");
        }

        /// <inheritdoc />
        /// <summary>
        /// Helper: A Fake Processor for testing
        /// </summary>
        /// <seealso cref="Communication.Interfaces.ILogProcessor" />
        private class FakeLogProcessor : ILogProcessor
        {
            /// <summary>
            /// Gets or sets the last message.
            /// </summary>
            /// <value>
            /// The last message.
            /// </value>
            public string LastMessage { get; private set; }

            /// <summary>
            /// Processes the received message (e.g., save to DB, log to file).
            /// </summary>
            /// <param name="message">The message received from the client.</param>
            /// <returns>Message Status.</returns>
            public Task ProcessMessageAsync(string message)
            {
                LastMessage = message;
                return Task.CompletedTask;
            }
        }
    }
}
