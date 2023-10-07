using FluentValidation;

namespace ByteSyncer.Application.Extensions
{
    public static class FluentValidationRuleBuilderExtensions
    {
        public const string SpecialCharacters = @"!@#$%^&*()_+-=~`[]{};:',<.>/?\|";

        public static IRuleBuilderOptions<T, string?> Url<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.Must(url =>
            {
                bool valid = !string.IsNullOrWhiteSpace(url);

                if (valid)
                {
                    valid = Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
                }

                return valid;
            });
        }

        public static IRuleBuilderOptions<T, string?> HasNumericCharacter<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.Must(value =>
            {
                ReadOnlySpan<char> span = value.AsSpan();

                if (span.IsEmpty)
                {
                    return false;
                }

                foreach (char c in span)
                {
                    if (char.IsNumber(c))
                    {
                        return true;
                    }
                }

                return false;
            });
        }

        public static IRuleBuilderOptions<T, string?> HasSpecialCharacter<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.Must(value =>
            {
                ReadOnlySpan<char> span = value.AsSpan();

                if (span.IsEmpty)
                {
                    return false;
                }

                foreach (char c in span)
                {
                    if (SpecialCharacters.Contains(c))
                    {
                        return true;
                    }
                }

                return false;
            });
        }

        public static IRuleBuilderOptions<T, string?> HasLowerCaseCharacter<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.Must(value =>
            {
                ReadOnlySpan<char> span = value.AsSpan();

                if (span.IsEmpty)
                {
                    return false;
                }

                foreach (char c in span)
                {
                    if (char.IsLower(c))
                    {
                        return true;
                    }
                }

                return false;
            });
        }

        public static IRuleBuilderOptions<T, string?> HasUpperCaseCharacter<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.Must(value =>
            {
                ReadOnlySpan<char> span = value.AsSpan();

                if (span.IsEmpty)
                {
                    return false;
                }

                foreach (char c in span)
                {
                    if (char.IsUpper(c))
                    {
                        return true;
                    }
                }

                return false;
            });
        }
    }
}
