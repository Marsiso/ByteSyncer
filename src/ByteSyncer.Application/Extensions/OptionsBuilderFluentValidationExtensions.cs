using ByteSyncer.Application.Validations;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ByteSyncer.Application.Extensions
{
    public static class OptionsBuilderFluentValidationExtensions
    {
        public static OptionsBuilder<TOptions> ValidateFluently<TOptions>(this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
        {
            optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(serviceProvider =>
            {
                IValidator<TOptions> optionsValidator = serviceProvider.GetRequiredService<IValidator<TOptions>>();

                return new FluentValidationOptions<TOptions>(optionsBuilder.Name, optionsValidator);
            });

            return optionsBuilder;
        }
    }
}
