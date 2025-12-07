using Refit;
using service.Dto;

namespace service.Services
{
	public interface IAuthTokenService
	{
        [Post("/user/login")]
        Task<AuthResponse> GenerateTokenAsync([Body] AuthRequest request, CancellationToken cancellationToken);
    }
}

