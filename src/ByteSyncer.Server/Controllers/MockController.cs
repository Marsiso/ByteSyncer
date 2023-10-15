using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ByteSyncer.Server.Controllers
{
    [ApiController]
    [Route("api/[Controller]/[Action]")]
    public class MockController : ControllerBase
    {
        /// <summary>
        ///     Gets the authenticated user name from the claims principal.
        /// </summary>
        /// <remarks>
        ///     GET api/mock/getusername
        ///     {
        ///         "name": "User name: 'Petr Pavel'."
        ///     }
        /// </remarks>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetUserName()
        {
            return Ok(new { Name = $"User name: '{HttpContext.User?.Identity?.Name}'." });
        }
    }
}
