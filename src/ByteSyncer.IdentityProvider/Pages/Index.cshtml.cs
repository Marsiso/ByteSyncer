using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Web;
using AutoMapper;
using ByteSyncer.Core.CQRS.Application.Commands;
using ByteSyncer.Domain.Application.DataTransferObjects;
using ByteSyncer.Domain.Application.Models;
using ByteSyncer.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ByteSyncer.IdentityProvider.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public IndexModel(IMapper mapper, IMediator mediator)
        {
            _mapper = mapper;
            _mediator = mediator;
        }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public LoginInput? Form { get; set; }

        public EntityValidationException? ValidationException { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            ValidateCredentialsCommand command = _mapper.Map<ValidateCredentialsCommand>(Form);
            ValidateCredentialsCommandResult commandResult = await _mediator.Send(command);

            if (commandResult.ResultType == ValidateCredentialsCommandResultType.ValidCredentials)
            {
                User user = commandResult.GetResult();

                List<Claim> claims = new List<Claim>()
                {
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.GivenName, user.GivenName),
                    new(ClaimTypes.Surname, user.FamilyName)
                };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                if (string.IsNullOrWhiteSpace(ReturnUrl))
                {
                    return Redirect("/Index");
                }
                else
                {
                    return Redirect(ReturnUrl);
                }
            };

            ValidationException = commandResult?.Exception as EntityValidationException;

            return Page();
        }

        public IActionResult OnPostSetCulture(string? culture, string? redirectUri)
        {
            return Page();
        }

        public bool HasValidationErrors([NotNullWhen(true)] string? propertyName)
        {
            bool hasValidationErrors = ValidationException is not null && ValidationException.Errors.ContainsKey(propertyName);

            return hasValidationErrors;
        }

        public string GetInputValidationErrorClasses(string? propertyName)
        {
            string classes = string.Empty;

            if (HasValidationErrors(propertyName))
            {
                classes = "is-invalid";
            }

            return classes;
        }

        public string GetFirstValidationError(string? propertyName)
        {
            string message = string.Empty;

            if (HasValidationErrors(propertyName))
            {
                message = ValidationException.Errors[propertyName].First();
            }

            return message;
        }

        public string GetRegisterPageRedirectUrl()
        {
            if (string.IsNullOrWhiteSpace(ReturnUrl))
            {
                return "/Register";
            }
            else
            {
                string returnUrlEncoded = HttpUtility.UrlEncode(ReturnUrl);
                return $"/Register?ReturnUrl={returnUrlEncoded}";
            }
        }
    }
}
