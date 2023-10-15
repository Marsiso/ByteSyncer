using System.Security.Cryptography;
using System.Text;
using ByteSyncer.Application.Options;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Options;

namespace ByteSyncer.Application.Services
{
    public class StringProtector
    {
        private static readonly byte[] IV =
        {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
        };

        public readonly StringProtectorOptions Options;

        public StringProtector(IOptions<StringProtectorOptions> options)
        {
            Guard.IsNotNull(options);
            Guard.IsNotNull(options.Value);

            Options = options.Value;

            Guard.IsGreaterThanOrEqualTo(Options.KeySize, 16);
            Guard.IsGreaterThanOrEqualTo(Options.SaltSize, 16);
            Guard.IsGreaterThanOrEqualTo(Options.Cycles, 0);
        }

        public ReadOnlySpan<byte> DeriveKeyFromPassword(string stringValue)
        {
            Guard.IsNotNullOrWhiteSpace(stringValue);

            string stringValueWithPepper = stringValue + Options.Pepper;

            Span<byte> stringWithPepperBytes = stackalloc byte[stringValueWithPepper.Length];

            Encoding.UTF8.GetBytes(stringValueWithPepper, stringWithPepperBytes);

            Span<byte> emptySaltBytes = Array.Empty<byte>();
            Span<byte> derivedKeyBytes = stackalloc byte[Options.KeySize];

            Rfc2898DeriveBytes.Pbkdf2(stringWithPepperBytes, emptySaltBytes, derivedKeyBytes, Options.Cycles, HashAlgorithmName.SHA512);

            return derivedKeyBytes.ToArray();
        }

        public async Task<byte[]> Encrypt(string value, string passphrase)
        {
            Guard.IsNotNullOrWhiteSpace(value);
            Guard.IsNotNullOrWhiteSpace(passphrase);

            using Aes aes = Aes.Create();

            aes.Key = DeriveKeyFromPassword(passphrase).ToArray();
            aes.IV = IV;

            using MemoryStream memoryStream = new();
            await using CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

            await cryptoStream.WriteAsync(Encoding.Unicode.GetBytes(value));
            await cryptoStream.FlushFinalBlockAsync();

            byte[] encryptedStringValueBytes = memoryStream.ToArray();

            return encryptedStringValueBytes;
        }

        public async Task<string> EncryptThenEncodeToBase64(string stringValue, string passphrase)
        {
            Guard.IsNotNullOrWhiteSpace(stringValue);
            Guard.IsNotNullOrWhiteSpace(passphrase);

            byte[] encryptedStringValueBytes = await Encrypt(stringValue, passphrase);

            string encryptedStringValueBase64 = Convert.ToBase64String(encryptedStringValueBytes);

            return encryptedStringValueBase64;
        }

        public async Task<string> Decrypt(byte[] encodedStringValueBytes, string passphrase)
        {
            Guard.IsNotEmpty(encodedStringValueBytes);
            Guard.IsNotNullOrWhiteSpace(passphrase);

            using Aes aes = Aes.Create();
            aes.Key = DeriveKeyFromPassword(passphrase).ToArray();
            aes.IV = IV;

            using MemoryStream memoryStream = new(encodedStringValueBytes);
            await using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);

            using MemoryStream memoryStreamCopy = new();

            await cryptoStream.CopyToAsync(memoryStreamCopy);

            byte[] memoryStreamCopyBuffer = memoryStreamCopy.ToArray();

            string decryptedStringValueBytes = Encoding.Unicode.GetString(memoryStreamCopyBuffer);

            return decryptedStringValueBytes;
        }

        public async Task<string> DecodeFromBase64ThenDecrypt(string encodedStringValueBase64, string passphrase)
        {
            Guard.IsNotNullOrWhiteSpace(encodedStringValueBase64);
            Guard.IsNotNullOrWhiteSpace(passphrase);

            byte[] decodedStringValueBytes = Convert.FromBase64String(encodedStringValueBase64);

            string decryptedStringValue = await Decrypt(decodedStringValueBytes, passphrase);

            return decryptedStringValue;
        }
    }
}
