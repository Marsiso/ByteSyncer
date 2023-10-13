using ByteSyncer.Core.CQRS.Files.Commands;
using FluentValidation;

namespace ByteSyncer.Application.Files.Validators
{
    public class CreateFolderCommandValidator : AbstractValidator<CreateFolderCommand>
    {
        public CreateFolderCommandValidator()
        {
            RuleFor(command => command.Name)
                .NotEmpty()
                .MaximumLength(256);
        }
    }
}
