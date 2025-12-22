using SixLabors.Fonts;

namespace Comment.Infrastructure.Interfaces
{
    public interface ICaptchaConfig
    {
        string Chars { get; }
        List<FontFamily> Families { get; }
        int Height { get; }
        FontStyle[] Styles { get; }
        int Width { get; }
    }
}