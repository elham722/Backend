using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsrfController : ControllerBase
    {
        [HttpGet("token")]
        public IActionResult GetToken()
        {
            // Generate a new CSRF token
            var token = GenerateCsrfToken();
            
            // Store in session
            HttpContext.Session.SetString("CSRF_TOKEN", token);
            
            return Ok(new { token });
        }

        private string GenerateCsrfToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
} 