using ByteSyncer.Domain.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenIddict.Abstractions;

namespace ByteSyncer.IdentityProvider.Pages
{
    [Authorize]
    public class ConsentModel : PageModel
    {
        [BindProperty]
        public string? ReturnUrl { get; set; }

        public IActionResult OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? grant)
        {
            User.SetClaim(AuthorizationDefaults.ConsentNaming, grant);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, User);

            if (string.IsNullOrWhiteSpace(ReturnUrl))
            {
                return Redirect("/Index");
            }
            else
            {
                return Redirect(ReturnUrl);
            }
        }
    }
}
