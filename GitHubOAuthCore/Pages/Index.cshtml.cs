using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GitHubOAuthCore.Pages
{
    public class IndexModel : PageModel
    {
        public string GitHubName { get; set; }
        public string GitHubLogin { get; set; }
        public string GitHubUrl { get; set; }
        public string GitHubAvatar { get; set; }
        public string GitHubEmail { get; set; }
        public string GitHubBio { get; set; }
        public string GitHubCreatedAt { get; set; }


        public void OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                GitHubName = User.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;
                GitHubLogin = User.FindFirst(c => c.Type == "urn:github:login")?.Value;
                GitHubUrl = User.FindFirst(c => c.Type == "urn:github:url")?.Value;
                GitHubAvatar = User.FindFirst(c => c.Type == "urn:github:avatar")?.Value;
                GitHubEmail = User.FindFirst(c => c.Type == "urn:github:email")?.Value;
                GitHubBio = User.FindFirst(c => c.Type == "urn:github:bio")?.Value;
                GitHubCreatedAt = User.FindFirst(c => c.Type == "urn:github:created_at")?.Value;
            }
        }
    }
}
