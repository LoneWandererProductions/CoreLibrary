/*
 * COPYRIGHT:    See COPYING in the top level directory
 * PROJECT:      Communication
 * FILE:         HttpWireTracingHandler.cs
 * PURPOSE:      HTTP Message Handler middleware for Insomnia-like wire tracing with Trace/String toggle
 * PROGRAMMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communication
{
    /// <summary>
    /// Intercept Http messages and trace results. Supports toggling between System.Diagnostics.Trace and String output.
    /// </summary>
    /// <seealso cref="DelegatingHandler" />
    public sealed class HttpWireTracingHandler : DelegatingHandler
    {
        private readonly object _lockObj = new object();
        private string _lastTrace = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the log should be written to <see cref="System.Diagnostics.Trace"/>.
        /// Default is true.
        /// </summary>
        public bool LogToTrace { get; set; } = true;

        /// <summary>
        /// Optional callback that triggers whenever a new full wire trace string is generated.
        /// Perfect for UI bindings (e.g., updating a Blazor log terminal).
        /// </summary>
        public Action<string>? OnTraceCaptured { get; set; }

        /// <summary>
        /// Holds the exact string content of the very last captured HTTP wire trace.
        /// </summary>
        public string LastTrace
        {
            get
            {
                lock (_lockObj) { return _lastTrace; }
            }
            private set
            {
                lock (_lockObj) { _lastTrace = value; }
            }
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>
        /// The task object representing the asynchronous operation.
        /// </returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Lokaler Speicher für diese spezifische Anfrage (Thread-Safety!)
            var sb = new StringBuilder();

            sb.AppendLine("\n* ------------------ WIRE TRACE START ------------------ *");

            // 1. REQUEST LOGGING (>)
            sb.AppendLine($"\n> {request.Method} {request.RequestUri.PathAndQuery} HTTP/{request.Version}");
            sb.AppendLine($"> Host: {request.RequestUri.Host}");

            foreach (var header in request.Headers)
            {
                sb.AppendLine($"> {header.Key}: {string.Join(", ", header.Value)}");
            }

            if (request.Content != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    sb.AppendLine($"> {header.Key}: {string.Join(", ", header.Value)}");
                }

                // Buffer content safely so we don't disrupt the stream
                await request.Content.LoadIntoBufferAsync(cancellationToken);
                var reqBody = await request.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(reqBody))
                {
                    sb.AppendLine("> ");
                    sb.AppendLine(reqBody.Length > 1000 ? reqBody.Substring(0, 1000) + "... [TRUNCATED]" : reqBody);
                }
            }

            sb.AppendLine("\n* Technical upload complete, awaiting response...");

            // Execute the actual HTTP Call
            var response = await base.SendAsync(request, cancellationToken);

            // 2. RESPONSE LOGGING (<)
            sb.AppendLine($"\n< HTTP/{response.Version} {(int)response.StatusCode} {response.StatusCode}");

            foreach (var header in response.Headers)
            {
                sb.AppendLine($"< {header.Key}: {string.Join(", ", header.Value)}");
            }

            if (response.Content != null)
            {
                foreach (var header in response.Content.Headers)
                {
                    sb.AppendLine($"< {header.Key}: {string.Join(", ", header.Value)}");
                }

                var resBody = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(resBody))
                {
                    sb.AppendLine("< ");
                    sb.AppendLine(resBody);
                }
            }

            sb.AppendLine("\n* ------------------- WIRE TRACE END ------------------- *\n");

            // --- AUSWERTUNG DES WECHSELSCHALTERS ---
            var finalTraceLog = sb.ToString();
            LastTrace = finalTraceLog;

            // Schalter 1: Standard System.Diagnostics.Trace
            if (LogToTrace)
            {
                Trace.Write(finalTraceLog);
            }

            // Schalter 2: Event/Callback für String-Verarbeitung abfeuern
            OnTraceCaptured?.Invoke(finalTraceLog);

            return response;
        }
    }
}
