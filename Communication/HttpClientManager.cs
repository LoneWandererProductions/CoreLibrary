/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        HttpClientManager.cs
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
    ///      Http Client for sending requests.
    /// </summary>
    public static class HttpClientManager
    {

        /// <summary>
        ///  Singleton pattern is correct here
        /// </summary>
        private static readonly HttpClient HttpClient = new();

        /// <summary>
        ///      Calls a SOAP service asynchronously.
        /// </summary>
        /// <param name="url">The endpoint URL.</param>
        /// <param name="soapAction">The SOAP Action header.</param>
        /// <param name="soapXmlBody">The actual XML content.</param>
        /// <returns>Response string or null on failure.</returns>
        public static async Task<string?> CallSoapServiceAsync(string url, string soapAction, string soapXmlBody)
        {
            if (string.IsNullOrEmpty(url)) return null;

            try
            {
                // Create the content correctly with type "text/xml" right from the start
                using var content = new StringContent(soapXmlBody, Encoding.UTF8, ComResource.FormatXml); // "text/xml"

                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };

                // Add headers
                request.Headers.Add(ComResource.UserAgent, ComResource.InsomniaAgent);

                // Some SOAP services require the action to be quoted, e.g. "ActionName"
                if (!string.IsNullOrEmpty(soapAction))
                {
                    request.Headers.Add(ComResource.SoapAction, soapAction);
                }

                using var response = await HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                // In production, use a proper logger instead of Console.WriteLine
                Console.WriteLine(string.Format(ComResource.ErrorFormatOne, ex.Message));
                return null;
            }
        }

        /// <summary>
        /// Sends an HTTP request to the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="method">The method.</param>
        /// <param name="body">The body.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>Soap Message</returns>
        /// <exception cref="System.ArgumentException">
        /// url
        /// or
        /// method
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException"></exception>
        internal static async Task<string> SendMessageAsync(string url, string method, string? body = null, string contentType = ComResource.JsonHeader)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentException(ComResource.ErrorUrlEmpty, nameof(url));
            if (string.IsNullOrEmpty(method)) throw new ArgumentException(ComResource.ErrorMethodCannotBeNull, nameof(method));

            // Wrap HttpRequestMessage in 'using' to clean up resources immediately
            using var httpRequest = new HttpRequestMessage(new HttpMethod(method), url);

            if (!string.IsNullOrEmpty(body))
            {
                httpRequest.Content = new StringContent(body, Encoding.UTF8, contentType);
            }

            try
            {
                // Wrap HttpResponseMessage in 'using'
                using var response = await HttpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                // If failed, read the error message from the server (often contains validation details)
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(string.Format(ComResource.ErrorFormatTwo, response.StatusCode, errorContent));
            }
            catch (Exception)
            {
                // Don't wrap the exception in a new generic Exception; just rethrow or let it bubble up.
                // If you must wrap it, use a custom domain exception.
                throw;
            }
        }
    }
}
