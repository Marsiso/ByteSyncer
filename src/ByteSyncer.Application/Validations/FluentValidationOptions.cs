using ByteSyncer.Core.Helpers;
using CommunityToolkit.Diagnostics;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;

namespace ByteSyncer.Application.Validations
{
    public class FluentValidationOptions<TOptions> : IValidateOptions<TOptions> where TOptions : class
    {
        public readonly IValidator<TOptions> OptionsValidator;

        public FluentValidationOptions(string name, IValidator<TOptions> optionsValidator)
        {
            Name = name;
            OptionsValidator = optionsValidator;
        }

        public string? Name { get; }

        public ValidateOptionsResult Validate(string? optionsName, TOptions options)
        {
            if (optionsName is not null && !optionsName.Equals(Name, StringComparison.OrdinalIgnoreCase))
            {
                return ValidateOptionsResult.Skip;
            }

            Guard.IsNotNull(options);

            ValidationContext<TOptions> validationContext = new ValidationContext<TOptions>(options);
            ValidationResult validationResult = OptionsValidator.Validate(validationContext);

            if (validationResult.IsValid)
            {
                return ValidateOptionsResult.Success;
            }

            Dictionary<string, string[]> validationFailures = ValidationResultHelpers.DistinctErrorsByProperty(validationResult);

            string failureMessage = validationFailures.Select(kvp => $"Options '{typeof(TOptions).Name}' has validation errors. Property: '{kvp.Key}' Errors: '{string.Join(" ", kvp.Value)}'.")
                                                      .Aggregate((l, r) => string.Join(" ", l, r));

            return ValidateOptionsResult.Fail(failureMessage);
        }
    }
}
