/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommunicationTests
 * FILE:         HttpWireTracingHandlerTests.cs
 * PURPOSE:      Unit tests for HttpWireTracingHandler ensuring correct request/response interception and tracing
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.Net;
using System.Text;
using Communication;

namespace CommunicationTests
{
    /// <summary>
    /// Test our new Http wire tracer.
    /// </summary>
    [TestClass]
    public class HttpWireTracingHandlerTests
    {
        /// <summary>
        /// The test URL
        /// </summary>
        private const string TestUrl = "https://localhost/test/api/endpoint";

        /// <summary>
        /// Sends the asynchronous should log request and response to trace and return correct response.
        /// </summary>
        [TestMethod]
        public async Task SendAsync_ShouldLogRequestAndResponseToTrace_AndReturnCorrectResponse()
        {
            // Arrange
            using var stringWriter = new StringWriter();
            using var traceListener = new TextWriterTraceListener(stringWriter);
            Trace.Listeners.Add(traceListener);

            var expectedResponseBody = "{\"status\":\"success\",\"message\":\"f81dcbd2\"}";
            var innerHandler = new MockHttpMessageHandler(HttpStatusCode.OK, expectedResponseBody);

            var tracingHandler = new HttpWireTracingHandler
            {
                //switch Trace one and off
                LogToTrace = true,
                InnerHandler = innerHandler
            };

            using var client = new HttpClient(tracingHandler);
            var requestContent = new StringContent("{\"input\":\"data_from_riesa\"}", Encoding.UTF8, "application/json");

            try
            {
                // Act
                var response = await client.PostAsync(TestUrl, requestContent);
                var actualResponseBody = await response.Content.ReadAsStringAsync();

                // Erzwinge das Schreiben aller gepufferten Trace-Daten
                Trace.Flush();
                var traceOutput = stringWriter.ToString();

                // Assert 1: Überprüfung der echten HTTP-Funktionalität
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(expectedResponseBody, actualResponseBody);

                // Assert 2: Überprüfung der Wire-Trace Struktur (Insomnia Layout)
                Assert.IsTrue(traceOutput.Contains("WIRE TRACE START"), "Trace missing start boundary.");
                Assert.IsTrue(traceOutput.Contains("WIRE TRACE END"), "Trace missing end boundary.");

                // Assert 3: Validierung der Request-Daten (>)
                Assert.IsTrue(traceOutput.Contains("> POST /test/api/endpoint"), "Request method or path path not traced correctly.");
                Assert.IsTrue(traceOutput.Contains("> Host: localhost"), "Request host header missing in trace.");
                Assert.IsTrue(traceOutput.Contains("Content-Type: application/json"), "Content-Type header missing in request trace.");
                Assert.IsTrue(traceOutput.Contains("{\"input\":\"data_from_riesa\"}"), "Request body payload missing in trace.");

                // Assert 4: Validierung der Response-Daten (<)
                Assert.IsTrue(traceOutput.Contains("< HTTP/1.1 200 OK"), "Response status code line missing or incorrect in trace.");
                Assert.IsTrue(traceOutput.Contains(expectedResponseBody), "Response body payload missing in trace.");
            }
            finally
            {
                // Berebereinigung des Trace-Listeners, um Seiteneffekte auf andere Tests zu verhindern
                Trace.Listeners.Remove(traceListener);
            }
        }

        /// <summary>
        /// A local HTTP mock handler for simulating network traffic without using the physical connection.
        /// </summary>
        /// <seealso cref="HttpMessageHandler" />
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly string _content;

            public MockHttpMessageHandler(HttpStatusCode statusCode, string content)
            {
                _statusCode = statusCode;
                _content = content;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(_content, Encoding.UTF8, "application/json"),
                    Version = request.Version
                };

                return Task.FromResult(response);
            }
        }
    }
}
