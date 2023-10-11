using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using AutoMapper;
using ByteSyncer.Core.Application.Commands;
using ByteSyncer.Domain.Application.DataTransferObjects;
using ByteSyncer.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ByteSyncer.IdentityProvider.Pages.Authentication
{
    public class LoginModel : PageModel
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public LoginModel(IMapper mapper, IMediator mediator)
        {
            _mapper = mapper;
            _mediator = mediator;
        }

        [BindProperty]
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public LoginInput? Form { get; set; }

        public EntityValidationException? ValidationException { get; set; }

        public IActionResult OnGet(string? returnUrl)
        {
            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoginCommand command = _mapper.Map<LoginCommand>(Form);
            LoginCommandResult commandResult = await _mediator.Send(command);

            if (commandResult.Result == LoginCommandResultType.Succeded)
            {
                List<Claim> claims = new List<Claim>()
                {
                    new(ClaimTypes.Email, commandResult.User.Email),
                    new(ClaimTypes.GivenName, commandResult.User.GivenName),
                    new(ClaimTypes.Surname, commandResult.User.FamilyName)
                };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                if (string.IsNullOrWhiteSpace(ReturnUrl))
                {
                    return Redirect("../Index");
                }
                else
                {
                    return Redirect(ReturnUrl);
                }
            };

            ValidationException = commandResult?.Exception as EntityValidationException;

            return Page();
        }

        public bool HasValidationErrors([NotNullWhen(true)] string? propertyName)
        {
            bool hasValidationErrors = ValidationException is not null && ValidationException.Errors.ContainsKey(propertyName);

            return hasValidationErrors;
        }

        public string GetValidationInputClass(string? propertyName)
        {
            string classes = string.Empty;

            if (HasValidationErrors(propertyName))
            {
                classes = "is-invalid";
            }

            return classes;
        }

        public string GetFirstValidationErrorMessage(string? propertyName)
        {
            string message = string.Empty;

            if (HasValidationErrors(propertyName))
            {
                message = ValidationException.Errors[propertyName].First();
            }

            return message;
        }
    }
}
