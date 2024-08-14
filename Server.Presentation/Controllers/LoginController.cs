using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.BusinessLogic.Interfaces;
using Server.DTOs.Models.Auth;
using Server.DTOs.Models.Login;

namespace Server.Presentation.Controllers
{
    [ApiController]
    [Route("api/auth/[controller]")]
    [AllowAnonymous]
    public class LoginController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public LoginController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid login request." });
            }

            try
            {
                Token token = await _accountService.LoginAsync(request);

                if (string.IsNullOrEmpty(token.AccessToken) || string.IsNullOrEmpty(token.RefreshToken))
                    return StatusCode(500, new { message = "An error occurred while processing your request. Please try again later." });

                Response.Cookies.Append("refreshToken", token.RefreshToken, new CookieOptions()
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.Now.AddDays(7)
                });

                return Ok(token);
            }
            catch (Exception ex) when (ex.Message.Contains("Account not found"))
            {
                return Unauthorized(new { message = "Account not found." });
            }
            catch (Exception ex) when (ex.Message.Contains("Incorrect Password"))
            {
                return Unauthorized(new { message = "Incorrect password." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request. Please try again later." });
            }
        }
    }
}
