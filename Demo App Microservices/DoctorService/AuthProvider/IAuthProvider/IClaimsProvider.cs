using System.Security.Claims;
using AuthService.Messages;

namespace DoctorService.AuthProvider.IAuthProvider;

public interface IClaimsProvider
{
    void SetClaims(UserLoggedIn userLoggedIn);
    ClaimsPrincipal GetClaims();
}