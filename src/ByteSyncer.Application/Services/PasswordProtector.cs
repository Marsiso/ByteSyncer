using System.Security.Cryptography;
using System.Text;
using ByteSyncer.Application.External;
using ByteSyncer.Application.Options;
using ByteSyncer.Domain.Contracts;
using CommunityToolkit.Diagnostics;
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

        public (string, string) HashPassword(string passwordValue)
        {
            Guard.IsNotNullOrWhiteSpace(passwordValue);

            string passwordValueWithPepper = passwordValue + Options.Pepper;

            byte[] derivedKeyBytes = new byte[Options.KeySize];
            byte[] randomlyGeneratedSaltBytes = new byte[Options.SaltSize];

            SodiumLibrary.randombytes_buf(randomlyGeneratedSaltBytes, randomlyGeneratedSaltBytes.Length);

            int passwordHashingResult = SodiumLibrary.crypto_pwhash(
                derivedKeyBytes,
                derivedKeyBytes.Length,
                Encoding.UTF8.GetBytes(passwordValueWithPepper),
                passwordValueWithPepper.Length,
                randomlyGeneratedSaltBytes,
                Options.OperationsLimit,
                Options.MemoryLimit,
                (int)Options.Algorithm);

            Guard.IsEqualTo(passwordHashingResult, 0);

            return (Convert.ToBase64String(derivedKeyBytes), Convert.ToBase64String(randomlyGeneratedSaltBytes));
        }

        public bool VerifyPassword(string passwordValue, string derivedKeyFromOriginalPasswordBase64, string originalPasswordSaltUsedForKeyDerivationBase64)
        {
            Guard.IsNotNullOrWhiteSpace(passwordValue);
            Guard.IsNotNullOrWhiteSpace(derivedKeyFromOriginalPasswordBase64);
            Guard.IsNotNullOrWhiteSpace(originalPasswordSaltUsedForKeyDerivationBase64);

            string passwordWithPepperToBeVerified = passwordValue + Options.Pepper;

            byte[] derivedKeyBytes = new byte[Options.KeySize];

            byte[] derivedKeyFromOriginalPasswordBytes = Convert.FromBase64String(derivedKeyFromOriginalPasswordBase64);
            byte[] originalPasswordSaltUsedForKeyDerivationBytes = Convert.FromBase64String(originalPasswordSaltUsedForKeyDerivationBase64);

            int passwordHashingResult = SodiumLibrary.crypto_pwhash(
                derivedKeyBytes,
                derivedKeyBytes.Length,
                Encoding.UTF8.GetBytes(passwordWithPepperToBeVerified),
                passwordWithPepperToBeVerified.Length,
                originalPasswordSaltUsedForKeyDerivationBytes,
                Options.OperationsLimit,
                Options.MemoryLimit,
                (int)Options.Algorithm);

            Guard.IsEqualTo(passwordHashingResult, 0);

            return CryptographicOperations.FixedTimeEquals(derivedKeyBytes, derivedKeyFromOriginalPasswordBytes);
        }
    }
}
