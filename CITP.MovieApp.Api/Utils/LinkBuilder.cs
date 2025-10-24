namespace CITP.MovieApp.Api.Utils;

public static class LinkBuilder
{
    public static string? PageLink(HttpRequest req, int targetPage, int pageSize, int total)
    {
        if (targetPage < 1) return null;
        var maxPage = (int)Math.Ceiling(total / (double)pageSize);
        if (targetPage > maxPage) return null;

        var dict = new Dictionary<string, string?>();
        foreach (var kv in req.Query) dict[kv.Key] = kv.Value.ToString();
        dict["page"] = targetPage.ToString();
        dict["pageSize"] = pageSize.ToString();

        var qs = QueryString.Create(dict);
        return req.Path + qs.ToUriComponent();
    }
}