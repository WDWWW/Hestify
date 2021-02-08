﻿using System.Net.Http;

namespace Hestify.Helpers
{
    public static class HttpClientHelper
    {
        public static HestifyClient Resource(this HttpClient client, string relativeUri)
        {
            return new HestifyClient(client, relativeUri);
        }
    }
}