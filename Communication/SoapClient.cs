using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Communication
{
    public class SoapClient
    {
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30) // Set a timeout for the request
        };


    }
}
