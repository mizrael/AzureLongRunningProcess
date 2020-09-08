using System;
using Microsoft.AspNetCore.Http;

namespace AzureLongRunningProcess.Utils
{
    public class EntityUriHelpers
    {
        public static Uri BuildAbsolute(string location, HttpRequest req)
        {
            var uri = new UriBuilder(req.Scheme, req.Host.Host)
            {
                Path = $@"api/{location}",
            };
            if (req.Host.Port.HasValue)
            {
                uri.Port = req.Host.Port.Value;
            }

            return uri.Uri;
        }
    }
}