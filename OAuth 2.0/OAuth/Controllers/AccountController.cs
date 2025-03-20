using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OIDC.Models;
using System.Security.Claims;

namespace OIDC.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string returnUrl = "/")
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpGet]
    public IActionResult ExternalLogin(string provider, string returnUrl = "/")
    {
        var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/", string remoteError = null)
    {
        if (remoteError != null)
        {
            ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
            return RedirectToAction("Login");
        }

        var info = await HttpContext.AuthenticateAsync("ExternalCookie");
        if (info == null || !info.Succeeded)
        {
            return RedirectToAction("Login");
        }

        // Get all claims from the external login provider
        var claims = info.Principal.Claims.ToList();

        // Extract specific claims
        var provider = info.Properties.Items[".AuthScheme"]; // This will be "Facebook" or "Google"
        var userId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name);
        var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
        var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);

        // Create a view model to display the information
        var viewModel = new ExternalLoginViewModel
        {
            Provider = provider,
            UserId = userId,
            Email = email,
            Name = name,
            GivenName = givenName,
            Surname = surname,
            AllClaims = claims.Select(c => new ClaimViewModel { Type = c.Type, Value = c.Value }).ToList()
        };

        // Store the return URL in TempData
        TempData["ReturnUrl"] = returnUrl;

        // Return a view to display the information
        return View("ExternalLoginInfo", viewModel);
    }
}