using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace GitHubOAuthCore.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        [Route("[controller]/[action]")]
        public IActionResult Login(string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties() { RedirectUri = returnUrl });
        }
    }
}
