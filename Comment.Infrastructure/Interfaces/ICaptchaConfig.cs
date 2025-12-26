using SixLabors.Fonts;

namespace Comment.Infrastructure.Interfaces
{
    /// <summary>
    /// Defines the configuration settings required for generating CAPTCHA images.
    /// </summary>
    public interface ICaptchaConfig
    {

        /// <summary>
        /// Gets the set of characters used to generate the random CAPTCHA text.
        /// </summary>
        string Chars { get; }

        /// <summary>
        /// Gets the list of font families available for rendering the CAPTCHA text.
        /// </summary>
        List<FontFamily> Families { get; }

        /// <summary>
        /// Gets the total height of the generated CAPTCHA image in pixels.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the total width of the generated CAPTCHA image in pixels.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the collection of font styles (e.g., Bold, Italic) to be applied to the CAPTCHA text.
        /// </summary>
        FontStyle[] Styles { get; }
    }
}