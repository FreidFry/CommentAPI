using Comment.Infrastructure.Interfaces;
using SixLabors.Fonts;

namespace CommentAPI.Extencions.LoadModules
{
    public class CaptchaConfig : ICaptchaConfig
    {
        public List<FontFamily> Families { get; } = [];
        public string Chars { get; } = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        public FontStyle[] Styles { get; } = [FontStyle.Bold, FontStyle.Italic, FontStyle.BoldItalic, FontStyle.Regular];
        public int Width { get; } = 150;
        public int Height { get; } = 50;
        public CaptchaConfig()
        {
            string fontsPath = Path.Combine(AppContext.BaseDirectory, "Assets", "ttf");

            var collection = new FontCollection();

            if (Directory.Exists(fontsPath))
            {
                var fontFiles = Directory.GetFiles(fontsPath, "*.ttf");
                foreach (var file in fontFiles)
                    Families.Add(collection.Add(file));
            }
        }
    }
}
