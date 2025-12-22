namespace Comment.Infrastructure.Interfaces
{
    public interface IHtmlSanitize
    {
        string Sanitize(string html);
    }
}