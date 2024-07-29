using Microsoft.AspNetCore.Identity;

namespace CodeFirstApi.Models.Sso
{
    public class ExtendedIdentityUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}
