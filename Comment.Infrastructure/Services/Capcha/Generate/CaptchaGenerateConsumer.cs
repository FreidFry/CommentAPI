using Comment.Infrastructure.Interfaces;
using Comment.Infrastructure.Services.Capcha.CapchaGenerate.Request;
using Comment.Infrastructure.Services.Capcha.CapchaGenerate.Response;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Comment.Infrastructure.Services.Capcha.CapchaGenerate
{
    public class CaptchaGenerateConsumer : IConsumer<CaptchaGenerateRequest>
    {
        private readonly IDistributedCache _cache;
        private readonly ICaptchaConfig _captchaConfig;

        public CaptchaGenerateConsumer(IDistributedCache cache, ICaptchaConfig captchaConfig)
        {
            _cache = cache;
            _captchaConfig = captchaConfig;
        }

        /// <summary>
        /// Processes a request to generate a new CAPTCHA.
        /// This method generates a random code, stores it in the distributed cache, and renders a noise-protected image.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="CaptchaGenerateRequest"/>.</param>
        /// <returns>
        /// A <see cref="CaptchaGenerateResponse"/> containing a unique tracking ID and the Base64-encoded PNG image.
        /// </returns>
        /// <remarks>
        /// The generation process follows these steps:
        /// <list type="number">
        /// <item>Generates a 5-character alphanumeric code.</item>
        /// <item>Persists the code in Redis (via <see cref="IDistributedCache"/>) with a 5-minute expiration.</item>
        /// <item>Renders the image using <c>SixLabors.ImageSharp</c> with random fonts, styles, and sizes.</item>
        /// <item>Applies security noise: random background lines, colored text, and pixel-based "salt".</item>
        /// <item>Converts the resulting image into a Base64 string for safe transport.</item>
        /// </list>
        /// </remarks>
        public async Task Consume(ConsumeContext<CaptchaGenerateRequest> context)
        {
            var code = GenerateRandomCode(5);
            var id = Guid.NewGuid().ToString("N");

            await _cache.SetStringAsync(
                $"captcha:{id}",
                code,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }
            );

            var randomFamily = _captchaConfig.Families[Random.Shared.Next(_captchaConfig.Families.Count - 1)];

            float randomSize = Random.Shared.Next(22, 29);

            FontStyle randomStyle = _captchaConfig.Styles[Random.Shared.Next(_captchaConfig.Styles.Length - 1)];

            var font = randomFamily.CreateFont(randomSize, randomStyle);

            using var image = new Image<Rgba32>(_captchaConfig.Width, _captchaConfig.Height);

            image.Mutate(ctx =>
            {
                ctx.Fill(Color.WhiteSmoke);

                // Линии шума
                for (int i = 0; i < 10; i++)
                {
                    var color = GetRandomColor(100, 200);
                    var p1 = new PointF(Random.Shared.Next( _captchaConfig.Width), Random.Shared.Next(_captchaConfig.Height));
                    var p2 = new PointF(Random.Shared.Next(_captchaConfig.Width), Random.Shared.Next(_captchaConfig.Height));
                    ctx.DrawLine(color, 1.2f, p1, p2);
                }

                // Отрисовка текста
                for (int i = 0; i < code.Length; i++)
                {
                    var character = code[i].ToString();
                    var x = 15 + (i * 25);
                    var y = Random.Shared.Next(5, 12);

                    ctx.DrawText(
                        character,
                        font,
                        GetRandomColor(0, 150),
                        new PointF(x, y)
                    );
                }

                // Точки (шум)
                for (int i = 0; i < 200; i++)
                {
                    var x = Random.Shared.Next(_captchaConfig.Width);
                    var y = Random.Shared.Next(_captchaConfig.Height);
                    image[x, y] = Color.Silver;
                }
            });

            using var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);

            await context.RespondAsync(new CaptchaGenerateResponse(id, Convert.ToBase64String(ms.ToArray())));
        }

        private string GenerateRandomCode(int length)
        {
            return new string(Enumerable.Repeat(_captchaConfig.Chars, length)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }

        private Color GetRandomColor(int min, int max)
        {
            return Color.FromRgb(
                (byte)Random.Shared.Next(min, max),
                (byte)Random.Shared.Next(min, max),
                (byte)Random.Shared.Next(min, max)
            );
        }
    }
}
