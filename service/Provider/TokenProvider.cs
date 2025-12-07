using Microsoft.Extensions.Caching.Memory;
using service.Dto;
using service.Services;

namespace service.Provider;

public class TokenProvider : ITokenProvider
{
    private readonly IAuthTokenService _authApi;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "ManagerAccessToken";

    public TokenProvider(IAuthTokenService authApi, IMemoryCache cache)
    {
        _authApi = authApi;
        _cache = cache;
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out string token))
            return token!;

        var response = await _authApi.GenerateTokenAsync(new AuthRequest
        {
            Login = "schedule",
            Password = "#scheduleservice#"
        }, cancellationToken);

        token = response.Data!;

        _cache.Set(CacheKey, token, TimeSpan.FromSeconds(30));
        // 30 segundos de segurança

        return token;
    }
}

