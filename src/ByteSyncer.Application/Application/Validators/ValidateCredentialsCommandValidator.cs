using ByteSyncer.Core.CQRS.Application.Commands;
using ByteSyncer.Core.CQRS.Application.Queries;
using ByteSyncer.Domain.Application.Models;
using ByteSyncer.Domain.Contracts;
using FluentValidation;
using MediatR;

namespace ByteSyncer.Application.Application.Validators
{
    public class ValidateCredentialsCommandValidator : AbstractValidator<ValidateCredentialsCommand>
    {
        private readonly IMediator _mediator;
        private readonly IPasswordProtector _passwordProtector;

        public ValidateCredentialsCommandValidator(IMediator mediator, IPasswordProtector passwordProtector)
        {
            _mediator = mediator;
            _passwordProtector = passwordProtector;

            RuleFor(command => command.Email)
                .NotEmpty()
                .WithMessage("Emailová adresa je požadována.")
                .MaximumLength(256)
                .WithMessage("Emailová adresa může obsahovat nejvýše 256 znaků.")
                .EmailAddress()
                .WithMessage("Emailová adresa má neplatný formát.");

            RuleFor(command => command.Password)
               .NotEmpty()
               .WithMessage("Heslo je požadováno.")
               .MaximumLength(512)
               .WithMessage("Heslo může obsahovat nejvýše 512 znaků.");

            When(command => !string.IsNullOrWhiteSpace(command.Email) && !string.IsNullOrWhiteSpace(command.Password), () =>
            {
                RuleFor(command => command.Email)
                    .MustAsync(async (command, email, cancelationToken) => await HasValidCredentials(command, email, mediator, passwordProtector, cancelationToken))
                    .WithMessage("Emailová adresa nebo uživatelské heslo není platné.");
            });
        }

        private static async Task<bool> HasValidCredentials(ValidateCredentialsCommand command, string? email, IMediator mediator, IPasswordProtector passwordProtector, CancellationToken cancellationToken)
        {
            FindUserUsingEmailQuery query = new FindUserUsingEmailQuery(email);
            FindUserUsingEmailQueryResult queryResult = await mediator.Send(query, cancellationToken);

            if (queryResult.ResultType != FindUserUsingEmailQueryResultType.UserFound)
            {
                return false;
            }

            User user = queryResult.GetResult();

            bool passwordMatches = passwordProtector.VerifyPassword(command.Password, user.Password, user.PasswordSalt);

            return passwordMatches;
        }
    }
}
