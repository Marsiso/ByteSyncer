using System.Security.Cryptography;
using System.Text;
using ByteSyncer.Application.Options;
using ByteSyncer.Application.Utilities;
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

        public (string, string) HashPassword(string password)
        {
            ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));

            string passwordWithPepper = password + Options.Pepper;

            byte[] keyBytes = new byte[Options.KeySize];
            byte[] saltBytes = new byte[Options.SaltSize];

            SodiumLibrary.randombytes_buf(saltBytes, saltBytes.Length);

            int result = SodiumLibrary.crypto_pwhash(
                keyBytes,
                keyBytes.Length,
                Encoding.UTF8.GetBytes(passwordWithPepper),
                password.Length,
                saltBytes,
                Options.OperationsLimit,
                Options.MemoryLimit,
                (int)Options.Algorithm);

            if (result != 0)
            {
                throw new Exception();
            }

            return (Convert.ToBase64String(keyBytes), Convert.ToBase64String(saltBytes));
        }

        public bool VerifyPassword(string password, string passwordKey, string passwordSalt)
        {
            ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));
            ArgumentException.ThrowIfNullOrEmpty(passwordKey, nameof(passwordKey));
            ArgumentException.ThrowIfNullOrEmpty(passwordSalt, nameof(passwordSalt));

            string passwordWithPepper = password + Options.Pepper;

            byte[] keyBytes = new byte[Options.KeySize];

            byte[] originalKeyBytes = Convert.FromBase64String(passwordKey);
            byte[] originalSaltBytes = Convert.FromBase64String(passwordSalt);

            SodiumLibrary.randombytes_buf(originalSaltBytes, originalSaltBytes.Length);

            int result = SodiumLibrary.crypto_pwhash(
                keyBytes,
                keyBytes.Length,
                Encoding.UTF8.GetBytes(passwordWithPepper),
                password.Length,
                originalSaltBytes,
                Options.OperationsLimit,
                Options.MemoryLimit,
                (int)Options.Algorithm);

            if (result != 0)
            {
                throw new Exception();
            }

            return CryptographicOperations.FixedTimeEquals(keyBytes, originalKeyBytes);
        }
    }
}
