/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        Communication/HttpClientManager.cs
 * PURPOSE:     Handle Http Requests
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Communication;

/// <summary>
///     Http Client for sending requests.
/// </summary>
public static class HttpClientManager
{
    /// <summary>
    ///     The HTTP client (static to be shared across all usages).
    /// </summary>
    private static readonly HttpClient HttpClient = new();

    /// <summary>
    ///     Calls the SOAP service asynchronous.
    /// </summary>
    /// <returns>Message as string</returns>
    public static async Task<string?> CallSoapServiceAsync()
    {
        //target url
        var requestUri = new Uri(string.Empty);
        //define commmand in xml body, see CreateSoapRequest
        var soapAction = string.Empty;
        var soapRequest = CreateSoapRequest();
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Headers = { { ComResource.UserAgent, ComResource.InsomniaAgent }, { ComResource.SoapAction, soapAction } },
            Content = new StringContent(soapRequest)
            {
                // Set the correct Content-Type to 'text/xml; charset=utf-8'
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue(ComResource.FormatXml)
                    {
                        CharSet = ComResource.Formatting
                    }
                }
            }
        };
        try
        {
            using var response = await HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode(); // Will throw if not successful
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            // Handle errors (e.g., network issues, timeouts, etc.)
            Console.WriteLine(ComResource.ErrorFormatOne, ex.Message);
            return null;
        }
    }

    /// <summary>
    ///     Creates the SOAP request.
    /// </summary>
    /// <returns>Soap xml body</returns>
    private static string CreateSoapRequest()
    {
        // You can structure the SOAP request body using string interpolation for readability
        return ComResource.SoapHeader;
    }

    /// <summary>
    ///     Sends an HTTP request to the specified URL with the given method and body.
    /// </summary>
    /// <param name="url">The URL to send the request to.</param>
    /// <param name="method">The HTTP method (GET, POST, PUT, DELETE, etc.).</param>
    /// <param name="body">The request body (optional).</param>
    /// <param name="contentType">The content type (default: application/json).</param>
    /// <returns>A <see cref="HttpResponseMessage" /> containing the response body.</returns>
    internal static async Task<string> SendMessageAsync(string url, string method, string? body = null,
        string contentType = ComResource.JsonHeader)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException(ComResource.ErrorUrlEmpty, nameof(url));
        }

        if (string.IsNullOrEmpty(method))
        {
            throw new ArgumentException(ComResource.ErrorMethodCannotBeNull, nameof(method));
        }

        var httpRequest = new HttpRequestMessage
        {
            Method = new HttpMethod(method),
            RequestUri = new Uri(url),
            Content = string.IsNullOrEmpty(body) ? null : new StringContent(body, Encoding.UTF8, contentType)
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
            throw new Exception(string.Format(ComResource.ErrorFormatTwo, response.StatusCode, errorContent));
        }
        catch (Exception ex)
        {
            // Handle any exception that occurred during the request
            throw new Exception(ComResource.ErrorMessageRequest, ex);
        }
    }
}
