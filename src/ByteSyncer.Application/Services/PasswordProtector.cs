using ByteSyncer.Application.Options;
using ByteSyncer.Domain.Contracts;
using Microsoft.Extensions.Options;

namespace ByteSyncer.Application.Services
{
    public class PasswordProtector : IPasswordProtector
    {
        public PasswordProtectorOptions Options { get; set; }

        public PasswordProtector(IOptions<PasswordProtectorOptions> options)
        {
            Options = options.Value;
        }

        public string HashPassword(string password)
        {
            ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));

            string passwordWithPepper = password + Options.Pepper;

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(passwordWithPepper, Options.WorkFactor);

            return passwordHash;
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));
            ArgumentException.ThrowIfNullOrEmpty(passwordHash, nameof(passwordHash));

            string passwordWithPepper = password + Options.Pepper;

            bool matchFound = BCrypt.Net.BCrypt.Verify(passwordWithPepper, passwordHash);

            return matchFound;
        }
    }
}
