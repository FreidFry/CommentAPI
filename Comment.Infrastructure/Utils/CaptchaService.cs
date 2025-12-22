using Comment.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Comment.Infrastructure.Utils;

public class CaptchaService 
{
    private readonly IDistributedCache _cache;
    

    public CaptchaService(IDistributedCache cache)
    {
        _cache = cache;

        
    }

    public async Task<bool> ValidateAsync(string id, string inputCode)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(inputCode))
            return false;

        var cacheKey = $"captcha:{id}";
        var actualCode = await _cache.GetStringAsync(cacheKey);

        if (actualCode == null) return false;

        await _cache.RemoveAsync(cacheKey);

        return actualCode.Equals(inputCode, StringComparison.OrdinalIgnoreCase);
    }

    
}