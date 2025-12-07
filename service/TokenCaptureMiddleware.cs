
namespace service
{
    public class TokenCaptureMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenCaptureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var token))
            {
                AccessTokenStore.Token = token.ToString();
            }

            await _next(context);
        }
    }
}

