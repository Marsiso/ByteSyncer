using ByteSyncer.Application.Extensions;
using ByteSyncer.Core.Application.Commands;
using ByteSyncer.Core.Application.Queries;
using FluentValidation;
using MediatR;

namespace ByteSyncer.Application.Application.Validators
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        private readonly IMediator _mediator;

        public RegisterCommandValidator(IMediator mediator)
        {
            _mediator = mediator;

            RuleFor(command => command.Email)
                .NotEmpty()
                .WithMessage("Emailová adresa je požadován.")
                .MaximumLength(256)
                .WithMessage("Emailová adresa může obsahovat nejvýše 256 znaků.")
                .EmailAddress()
                .WithMessage("Emailová adresa má neplatný formát.")
                .MustAsync(async (command, cancelationToken) => await EmailAddressExistsNot(command, mediator, cancelationToken))
                .WithMessage("Emailová adresa již existuje, vyberte prosím jinou nebo se přihlaste pomocí stávajícího účtu.");

            RuleFor(command => command.GivenName)
                .NotEmpty()
                .WithMessage("Jméno je požadováno.")
                .MaximumLength(256)
                .WithMessage("Jméno může obsahovat nejvýše 256 znaků.");

            RuleFor(command => command.FamilyName)
                .NotEmpty()
                .WithMessage("Příjmení je požadováno.")
                .MaximumLength(256)
                .WithMessage("Příjmení může obsahovat nejvýše 256 znaků.");

            RuleFor(command => command.Password)
                .NotEmpty()
                .WithMessage("Heslo je požadováno.")
                .MaximumLength(512)
                .WithMessage("Heslo může obsahovat nejvýše 512 znaků.")
                .HasNumericCharacter()
                .WithMessage("Heslo musí obsahovat alespoň 1 číslici.")
                .HasLowerCaseCharacter()
                .WithMessage("Heslo musí obsahovat alespoň 1 malé písmeno.")
                .HasUpperCaseCharacter()
                .WithMessage("Heslo musí obsahovat alespoň 1 velké písmeno.")
                .HasSpecialCharacter()
                .WithMessage("Heslo musí obsahovat alespoň 1 speciální znak.");

            RuleFor(command => command.PasswordRepeat)
                .NotEmpty()
                .WithMessage("Opakování hesla je požadováno.")
                .MaximumLength(512)
                .WithMessage("Opakování hesla může obsahovat nejvýše 512 znaků.")
                .Equal(command => command.Password)
                .WithMessage("Heslo a jeho opakování musí být shodné.");
        }

        private static async Task<bool> EmailAddressExistsNot(string? email, IMediator mediator, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return true;
            }

            EmailExistsQuery query = new EmailExistsQuery(email);
            EmailExistsQueryResult queryResult = await mediator.Send(query, cancellationToken);

            return !queryResult.Exists;
        }
    }
}
