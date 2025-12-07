
namespace service
{
    public static class AccessTokenStore
    {
        private static readonly AsyncLocal<string?> _token = new();

        public static string? Token
        {
            get => _token.Value;
            set => _token.Value = value;
        }
    }
}

