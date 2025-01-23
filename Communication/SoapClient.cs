using System;
using System.Net.Http;

namespace Communication
{
    public class SoapClient
    {
        private static readonly HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(30) // Set a timeout for the request
        };
    }
}
