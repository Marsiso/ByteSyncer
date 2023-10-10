using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using ByteSyncer.Core.Application.Commands;
using ByteSyncer.Domain.Application.DataTransferObjects;
using ByteSyncer.Domain.Exceptions;
using MediatR;
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

        public string? ReturnUrl { get; set; }

        [BindProperty]
        public LoginInput? Form { get; set; }

        public EntityValidationException? ValidationException { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoginCommand command = _mapper.Map<LoginCommand>(Form);
            LoginCommandResult commandResult = await _mediator.Send(command);

            if (commandResult.Result == LoginCommandResultType.Succeded)
            {
                return Redirect("../Index");
            }

            ValidationException = commandResult?.Exception as EntityValidationException;

            return Page();
        }

        public bool HasValidationErrors([NotNullWhen(true)] string? propertyName)
        {
            bool hasValidationErrors = ValidationException is not null && ValidationException.Errors.ContainsKey(propertyName);

            return hasValidationErrors;
        }

        public string GetValidationErrorsClasses(string? propertyName)
        {
            string classes = string.Empty;

            if (HasValidationErrors(propertyName))
            {
                classes = "invalid peer border-red-500";
            }

            return classes;
        }

        public string GetValidationErrorsMessages(string? propertyName)
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
