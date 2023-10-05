using AutoMapper;
using ByteSyncer.Core.Application.Commands;
using ByteSyncer.Domain.Application.DataTransferObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ByteSyncer.IdentityProvider.Pages.Authentication
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

        public string? ReturnUrl { get; set; }

        [BindProperty]
        public RegisterInput? Form { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            RegisterCommand command = _mapper.Map<RegisterCommand>(Form);
            RegisterCommandResult commandResult = await _mediator.Send(command);

            if (commandResult.Result == RegisterCommandResultType.Succeded)
            {
                return Redirect("../Index");
            }

            return Page();
        }
    }
}
