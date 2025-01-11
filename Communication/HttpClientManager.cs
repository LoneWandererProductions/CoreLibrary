/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        Communication/HttpClientManager.cs
 * PURPOSE:     Handle Http Requests
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
    /// <summary>
    ///     Http Client for sending requests.
    /// </summary>
    internal static class HttpClientManager
    {
        /// <summary>
        ///     The HTTP client (static to be shared across all usages).
        /// </summary>
        private static readonly HttpClient HttpClient = new();

        /// <summary>
        ///     Sends an HTTP request to the specified URL with the given method and body.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="method">The HTTP method (GET, POST, PUT, DELETE, etc.).</param>
        /// <param name="body">The request body (optional).</param>
        /// <param name="contentType">The content type (default: application/json).</param>
        /// <returns>A <see cref="HttpResponseMessage" /> containing the response body.</returns>
        internal static async Task<string> SendMessageAsync(string url, string method, string body = null,
            string contentType = "application/json")
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL cannot be null or empty.", nameof(url));
            }

            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException("Method cannot be null or empty.", nameof(method));
            }

            var httpRequest = new HttpRequestMessage
            {
                Method = new HttpMethod(method),
                RequestUri = new Uri(url),
                Content = string.IsNullOrEmpty(body)
                    ? null
                    : new StringContent(body, Encoding.UTF8, contentType)
            };

            try
            {
                // Send the request and get the response
                var response = await HttpClient.SendAsync(httpRequest);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    // If the response is JSON or text, read the content
                    return await response.Content.ReadAsStringAsync(); // Return the response content
                }

                // Handle HTTP error status codes
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}. {errorContent}");
            }
            catch (Exception ex)
            {
                // Handle any exception that occurred during the request
                throw new Exception("An error occurred while sending the request.", ex);
            }
        }
    }
}
