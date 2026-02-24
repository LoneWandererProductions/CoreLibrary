/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommunicationTests
 * FILE:        Communication.cs
 * PURPOSE:     Test for our communication
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Net.Sockets;
using System.Text;
using Communication;

namespace CommunicationTests
{
    /// <summary>
    ///     Some random stuff
    /// </summary>
    [TestClass]
    public class Communication
    {
        /// <summary>
        ///     The path
        /// </summary>
        private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), nameof(Communication));

        /// <summary>
        ///     Communications instance.
        /// </summary>
        [TestMethod]
        public async Task CommunicationsAsync()
        {
            var communication = new NetCom();
            // Use a URL that actually ends in a filename
            string url = "https://www.google.de/favicon.ico";
            string targetFolder = Path.Combine(Directory.GetCurrentDirectory(), "TestDownloads");

            bool success = await communication.SaveFile(targetFolder, url);

            // The helper logic will extract "favicon.ico"
            var expectedPath = Path.Combine(targetFolder, "favicon.ico");

            Assert.IsTrue(success, "Download failed");
            Assert.IsTrue(File.Exists(expectedPath), $"File not found at {expectedPath}");

            if (File.Exists(expectedPath)) File.Delete(expectedPath);
        }

        /// <summary>
        ///     Listeners the should respond with pong when pinged.
        /// </summary>
        [TestMethod]
        public async Task ListenerShouldRespondWithPongWhenPinged()
        {
            // Arrange
            const int port = 12345; // Example port
            var listener = new Listener(port);
            var cancellationTokenSource = new CancellationTokenSource();

            // Start the listener in a separate task to simulate real-world usage
            var listenerTask = Task.Run(() => listener.StartListeningAsync(cancellationTokenSource.Token),
                cancellationTokenSource.Token);

            // Wait for a brief moment to ensure the server is up and listening
            await Task.Delay(1000, cancellationTokenSource.Token);

            // Act - Connect to the server and send a message
            using (var client = new TcpClient("localhost", port))
            {
                var stream = client.GetStream();
                var requestMessage = Encoding.ASCII.GetBytes("PING");
                await stream.WriteAsync(requestMessage, cancellationTokenSource.Token);

                // Read the response
                var buffer = new byte[1024];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token);
                var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // Assert - Verify the response is "PONG"
                Assert.AreEqual("PONG", response, "Expected response was not received.");
            }

            // Stop the listener after the test
            await cancellationTokenSource.CancelAsync();
            await listenerTask; // Ensure listener has finished

            listener.StopListening();
        }
    }
}
