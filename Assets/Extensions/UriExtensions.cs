using System;
using System.Linq;

public static class UriExtensions
{
    public static Uri CreateUriWithParameters(this Uri baseUri, params Tuple<string, string>[] parameters)
    {
        UriBuilder builder = new UriBuilder(baseUri);
        string query = string.Join("&", parameters.Select(x => $"{x.Item1}={x.Item2}"));
        builder.Query = $"?{query}";

        return builder.Uri;
    }
}
