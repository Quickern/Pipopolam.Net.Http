using System;

namespace Pipopolam.Net.Http;

public enum UrlScheme
{
    Https,
    Http
}

internal static class UrlSchemeUtility
{
    public static string ToScheme(this UrlScheme scheme)
    {
        switch (scheme)
        {
            case UrlScheme.Https:
                return "https";
            case UrlScheme.Http:
                return "http";
            default:
                throw new NotSupportedException($"Scheme {scheme} is not supported!");
        }
    }

    public static UrlScheme GetSchemeByName(string name)
    {
        switch (name.ToLowerInvariant())
        {
            case "https":
                return UrlScheme.Https;
            case "http":
                return UrlScheme.Http;
            default:
                throw new NotSupportedException($"Scheme {name} is not supported!");
        }
    }
}
