/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        ComResource.cs
 * PURPOSE:     Some Resources for the Communication Library.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace Communication;

/// <summary>
///     String constants and resources used in the Communication library.
/// </summary>
internal static class ComResource
{
    /// <summary>
    ///     The separator used in URLs.
    /// </summary>
    internal const string Separator = "//";

    /// <summary>
    ///     The formatting used for encoding.
    /// </summary>
    internal const string Formatting = "utf-8";

    /// <summary>
    ///     The SOAP header used in SOAP requests.
    /// </summary>
    internal const string SoapHeader = @"<?xml version=""1.0""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tns=""http://tempuri.org/"">
   <soapenv:Header/>
   <soapenv:Body>
      <tns:HelloWorld/>
   </soapenv:Body>
</soapenv:Envelope>";

    /// <summary>
    ///     The json header used in JSON requests.
    /// </summary>
    internal const string JsonHeader = "application/json";

    /// <summary>
    ///     The error URL empty message.
    /// </summary>
    internal const string ErrorUrlEmpty = "URL cannot be null or empty.";

    /// <summary>
    ///     The error method cannot be null or empty message.
    /// </summary>
    internal const string ErrorMethodCannotBeNull = "Method cannot be null or empty.";

    /// <summary>
    ///     The error message request that occurs when sending a request.
    /// </summary>
    internal const string ErrorMessageRequest = "An error occurred while sending the request.";

    /// <summary>
    ///     The error format used for displaying error messages.
    /// </summary>
    internal const string ErrorFormatOne = "Error: {0}";

    /// <summary>
    ///     The error format used for displaying error messages with additional context.
    /// </summary>
    internal const string ErrorFormatTwo = "Error: {0}. {1}";

    /// <summary>
    ///     The server status stop message.
    /// </summary>
    internal const string ServerStatusStop = "Server stopped.";

    /// <summary>
    ///     The answer message sent in response to a ping request.
    /// </summary>
    internal const string AnswerMessage = "PONG";

    /// <summary>
    ///     The backslash used in URLs.
    /// </summary>
    internal const string Backslash = "/";

    /// <summary>
    ///     The message answer sent in response to a ping request.
    /// </summary>
    internal const string MessageAnswer = "Responded to a ping.";

    /// <summary>
    ///     The message listening indicating the server is ready to accept connections.
    /// </summary>
    internal const string MessageListening = "Listening on port {0}...";

    /// <summary>
    ///     The error accepting communication message.
    /// </summary>
    internal const string ErrorAcceptingCommunication = "Error accepting connection: {0}";

    /// <summary>
    ///     The error sending request message.
    /// </summary>
    internal const string ErrorSendingRequest = "An error occurred while sending the HTTP request.";

    /// <summary>
    ///     The error saving file message.
    /// </summary>
    internal const string ErrorSavingFile = "Error saving file: {0}";

    /// <summary>
    ///     The error unexpected message.
    /// </summary>
    internal const string ErrorUnexpected = "Unexpected error: {0}";

    /// <summary>
    ///     The user agent header used in HTTP requests.
    /// </summary>
    internal const string UserAgent = "User-Agent";

    /// <summary>
    ///     The insomnia agent used in HTTP requests.
    /// </summary>
    internal const string InsomniaAgent = "insomnia/10.2.0";

    /// <summary>
    ///     The SOAP action header used in SOAP requests.
    /// </summary>
    internal const string SoapAction = "SOAPAction";

    /// <summary>
    ///     The format XML header used in XML requests.
    /// </summary>
    internal const string FormatXml = "text/xml";
}
