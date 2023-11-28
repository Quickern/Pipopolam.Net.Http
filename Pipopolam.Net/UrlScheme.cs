using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipopolam.Net
{
    public enum UrlScheme
    {
        Http,
        Https
    }

    public static class UrlSchemeUtility
    {
        public static string ToScheme(this UrlScheme scheme)
        {
            switch (scheme)
            {
                case UrlScheme.Http:
                    return "http";
                case UrlScheme.Https:
                    return "https";
                default:
                    throw new NotSupportedException($"Scheme {scheme} is not supported!");
            }
        }

        public static UrlScheme GetSchemeByName(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "http":
                    return UrlScheme.Http;
                case "https":
                    return UrlScheme.Https;
                default:
                    throw new NotSupportedException($"Scheme {name} is not supported!");
            }
        }
    }
}
