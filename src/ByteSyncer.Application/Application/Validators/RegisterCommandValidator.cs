using ByteSyncer.Core.Application.Commands;
using FluentValidation;

namespace ByteSyncer.Application.Application.Validators
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(command => command.Email)
                .NotEmpty()
                .MaximumLength(256)
                .EmailAddress();

            RuleFor(command => command.GivenName)
                .NotEmpty()
                .MaximumLength(256);

            RuleFor(command => command.FamilyName)
                .NotEmpty()
                .MaximumLength(256);

            RuleFor(command => command.Password)
                .NotEmpty()
                .MaximumLength(512);

            RuleFor(command => command.PasswordRepeat)
                .NotEmpty()
                .MaximumLength(512);

            When(command => !string.IsNullOrWhiteSpace(command.Password) && !string.IsNullOrWhiteSpace(command.PasswordRepeat), () =>
            {
                RuleFor(command => command.Password)
                    .Equal(command => command.PasswordRepeat);
            });
        }
    }
}
