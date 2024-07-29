namespace CodeFirstApi.Models.Sso
{
    public class LoginResponse
    {
        public bool IsLoggedIn { get; set; } = false;
        public TokenStrings? Tokens { get; set; }
    }
}
