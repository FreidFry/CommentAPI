namespace Comment.Infrastructure.Utils
{
    public interface IHtmlSanitize
    {
        string Sanitize(string html);
    }
}