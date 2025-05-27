/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/Communication.cs
 * PURPOSE:     Test for our communication
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
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
            _ = await communication.SaveFile(_path, "https://www.google.de/");
            var path = Path.Combine(Directory.GetCurrentDirectory(), nameof(Communication), "www.google.de");

            Assert.IsTrue(File.Exists(path), "File not found");

            File.Delete(path);
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
            var listenerTask = Task.Run(() => listener.StartListening(cancellationTokenSource.Token),
                cancellationTokenSource.Token);

            // Wait for a brief moment to ensure the server is up and listening
            await Task.Delay(1000, cancellationTokenSource.Token);

            // Act - Connect to the server and send a message
            using (var client = new TcpClient("localhost", port))
            {
                var stream = client.GetStream();
                var requestMessage = Encoding.ASCII.GetBytes("PING");
                stream.Write(requestMessage, 0, requestMessage.Length);

                // Read the response
                var buffer = new byte[1024];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // Assert - Verify the response is "PONG"
                Assert.AreEqual("PONG", response, "Expected response was not received.");
            }

            // Stop the listener after the test
            cancellationTokenSource.Cancel();
            await listenerTask; // Ensure listener has finished

            listener.StopListening();
        }
    }
}
