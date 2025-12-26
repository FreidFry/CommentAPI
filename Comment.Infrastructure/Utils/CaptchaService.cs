using Microsoft.Extensions.Caching.Distributed;

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