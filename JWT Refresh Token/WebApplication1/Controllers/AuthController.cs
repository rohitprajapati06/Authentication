using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;


namespace WebApplication1.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        // Mock user validation
        if (model.Username == "user" && model.Password == "password")
        {
            var jwtHelper = new JwtHelper(_configuration);
            var tokens = jwtHelper.GenerateTokens("1", model.Username);

            // Save refresh token (In-memory for demo)
            RefreshTokenStore.RefreshTokens[tokens.RefreshToken] = model.Username;

            return Ok(tokens);
        }

        return Unauthorized("Invalid username or password.");
    }

    [HttpPost("refresh")]
    public IActionResult RefreshToken([FromBody] TokenModel model)
    {
        if (!RefreshTokenStore.RefreshTokens.ContainsKey(model.RefreshToken) ||
            RefreshTokenStore.RefreshTokens[model.RefreshToken] != "user")
        {
            return Unauthorized("Invalid refresh token.");
        }

        var jwtHelper = new JwtHelper(_configuration);
        var newTokens = jwtHelper.GenerateTokens("1", "user");

        // Replace the old refresh token
        RefreshTokenStore.RefreshTokens.Remove(model.RefreshToken);
        RefreshTokenStore.RefreshTokens[newTokens.RefreshToken] = "user";

        return Ok(newTokens);
    }

}

