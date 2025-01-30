using System.Security.Claims;
using AuthService.Messages;

namespace PatientService.AuthProvider.IAuthProvider;

public interface IClaimsProvider
{
    void SetClaims(UserLoggedIn userLoggedIn);
    ClaimsPrincipal GetClaims();
}
