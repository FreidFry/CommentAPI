using Comment.Infrastructure.Interfaces;
using Ganss.Xss;

namespace Comment.Infrastructure.Utils
{
    public class HtmlSanitize : IHtmlSanitize
    {
        private readonly HtmlSanitizer Sanitizer = new();

        public HtmlSanitize()
        {
            Sanitizer = new();
            Sanitizer.AllowedAttributes.Clear();
            Sanitizer.AllowedCssProperties.Clear();
            Sanitizer.AllowedAtRules.Clear();
            Sanitizer.PostProcessNode += (s, e) => {
                if (e.Node is AngleSharp.Html.Dom.IHtmlAnchorElement a)
                {
                    a.SetAttribute("rel", "nofollow");
                }
            };
            Sanitizer.AllowedTags.Clear();
            Sanitizer.AllowedTags.Add("a");
            Sanitizer.AllowedTags.Add("code");
            Sanitizer.AllowedTags.Add("strong");
            Sanitizer.AllowedTags.Add("i");
            Sanitizer.AllowedAttributes.Add("href");
        }

        public bool HasTextContent(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return false;

            var san = new HtmlSanitizer();
            san.AllowedTags.Clear();
            san.AllowedClasses.Clear();
            san.AllowedAttributes.Clear();
            var plain = san.Sanitize(html);

            return string.IsNullOrWhiteSpace(plain);
        }

        public string Sanitize(string html) => Sanitizer.Sanitize(html);
    }
}
