using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ByteSyncer.Web.Controllers
{
    [ApiController]
    [Route("[Controller]/[Action]")]
    public class ResourceController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetSecretResources()
        {
            string? email = HttpContext.User?.Identity?.Name;
            return Ok($"user: {email}");
        }
    }
}
