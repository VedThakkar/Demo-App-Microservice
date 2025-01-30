using System.Security.Claims;
using AuthService.Messages;
using DoctorService.AuthProvider.IAuthProvider;
using Serilog;

namespace DoctorService.AuthProvider;

public class ClaimsProvider : IClaimsProvider
{
    //private ClaimsPrincipal _currentClaims;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimsProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetClaims(UserLoggedIn userLoggedIn)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userLoggedIn.UserId.ToString()),
            new Claim(ClaimTypes.Email, userLoggedIn.Email),
            new Claim(ClaimTypes.Role, userLoggedIn.Role),
            new Claim("AccessToken", userLoggedIn.AccessToken),
            new Claim("RefreshToken", userLoggedIn.RefreshToken)
        };
        
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        //_currentClaims = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        
        // Set the ClaimsPrincipal to the current HTTP context
        _httpContextAccessor.HttpContext.User = principal;
        
        // Log claims to verify they are set properly
        Log.Information("Claims set: {Claims}", string.Join(", ", claims.Select(c => $"{c.Type}: {c.Value}")));
    }

    public ClaimsPrincipal GetClaims()
    {
        //return _currentClaims;
        return _httpContextAccessor.HttpContext?.User;
    }
}
