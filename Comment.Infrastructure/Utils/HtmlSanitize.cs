using Ganss.Xss;

namespace Comment.Infrastructure.Utils
{
    public class HtmlSanitize : IHtmlSanitize
    {
        private readonly HtmlSanitizer Sanitizer = new();

        public HtmlSanitize()
        {
            Sanitizer = new();
            Sanitizer.AllowedTags.Add("a");
            Sanitizer.AllowedTags.Add("code");
            Sanitizer.AllowedTags.Add("strong");
            Sanitizer.AllowedTags.Add("i");
        }


        public string Sanitize(string html) => Sanitizer.Sanitize(html);
    }
}
