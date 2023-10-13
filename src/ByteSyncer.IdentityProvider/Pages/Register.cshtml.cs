using System.Diagnostics.CodeAnalysis;
using System.Web;
using AutoMapper;
using ByteSyncer.Core.CQRS.Application.Commands;
using ByteSyncer.Domain.Application.DataTransferObjects;
using ByteSyncer.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ByteSyncer.IdentityProvider.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public RegisterModel(IMapper mapper, IMediator mediator)
        {
            _mapper = mapper;
            _mediator = mediator;
        }

        [BindProperty]
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public RegisterInput? Form { get; set; }

        public EntityValidationException? ValidationException { get; set; }

        public IActionResult OnGet(string? returnUrl)
        {
            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostRegisterAsync()
        {
            RegisterUserCommand command = _mapper.Map<RegisterUserCommand>(Form);
            RegisterUserCommandResult commandResult = await _mediator.Send(command);

            if (commandResult.ResultType == RegisterUserCommandResultType.UserCreated)
            {
                return Redirect(GetLoginPageRedirectUrl());
            }

            ValidationException = commandResult?.Exception as EntityValidationException;

            return Page();
        }

        public bool HasValidationErrors([NotNullWhen(true)] string? propertyName)
        {
            bool hasValidationErrors = ValidationException is not null && ValidationException.Errors.ContainsKey(propertyName);

            return hasValidationErrors;
        }

        public string GeInputValidationErrorClasses(string? propertyName)
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

        public string GetLoginPageRedirectUrl()
        {
            if (string.IsNullOrWhiteSpace(ReturnUrl))
            {
                return "/Index";
            }
            else
            {
                string returnUrlEncoded = HttpUtility.UrlEncode(ReturnUrl);
                return $"/Index?ReturnUrl={returnUrlEncoded}";
            }
        }
    }
}
