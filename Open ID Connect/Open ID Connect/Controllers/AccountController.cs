using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Open_ID_Connect.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly TestContext _context; // Inject your DbContext

    public AccountController(TestContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public IActionResult Login(string returnUrl = "/")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(ExternalLoginCallback), new { returnUrl })
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
        {
            return RedirectToAction("Login");
        }

        var claims = authenticateResult.Principal.Claims;
        var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // Extract Google ID
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
        var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
        var profilePhoto = claims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
        {
            return RedirectToAction("Login");
        }

        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            // Create a new user with Google ID
            user = new User
            {
                Id = googleId,
                FirstName = firstName ?? "Unknown",
                LasttName = lastName ?? "Unknown",
                Email = email,
                Password = "EXTERNAL_LOGIN", // Placeholder password
                ProfilePhoto = profilePhoto
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Update Google ID and Profile Photo if changed
            if (user.Id != googleId || user.ProfilePhoto != profilePhoto)
            {
                user.Id = googleId;
                user.ProfilePhoto = profilePhoto;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        // Sign in user
        var claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id)); // Store Google ID in claims
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LasttName}"));
        if (!string.IsNullOrEmpty(user.ProfilePhoto))
        {
            claimsIdentity.AddClaim(new Claim("ProfilePhoto", user.ProfilePhoto));
        }

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

        return LocalRedirect(returnUrl);
    }



    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public IActionResult Profile()
    {
        return View();
    }
}
