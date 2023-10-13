using CommunityToolkit.Diagnostics;
using FluentValidation.Results;

namespace ByteSyncer.Core.Helpers
{
    public static class ValidationResultHelpers
    {
        public static Dictionary<string, string[]> DistinctErrorsByProperty(ValidationResult validationResult)
        {
            Guard.IsNotNull(validationResult);

            return validationResult.Errors
                .GroupBy(validationFailure =>
                    validationFailure.PropertyName,
                    validationFailure => validationFailure.ErrorMessage,
                    (propertyName, validationFailuresByProperty) => new
                    {
                        Key = propertyName,
                        Values = validationFailuresByProperty.Distinct().ToArray()
                    })
                .ToDictionary(
                group => group.Key,
                group => group.Values);
        }
    }
}
