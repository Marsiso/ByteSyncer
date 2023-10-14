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

        public ReadOnlySpan<byte> DeriveKeyFromPassword(string value)
        {
            Guard.IsNotNullOrWhiteSpace(value);

            string pepperWithValue = value + Options.Pepper;

            Span<byte> pepperWithValueBytes = stackalloc byte[pepperWithValue.Length];

            Encoding.UTF8.GetBytes(pepperWithValue, pepperWithValueBytes);

            Span<byte> emptySaltBytes = Array.Empty<byte>();
            Span<byte> keyBytes = stackalloc byte[Options.KeySize];

            Rfc2898DeriveBytes.Pbkdf2(pepperWithValueBytes, emptySaltBytes, keyBytes, Options.Cycles, HashAlgorithmName.SHA512);

            return keyBytes.ToArray();
        }

        public async Task<byte[]> Encrypt(string value, string passphrase)
        {
            Guard.IsNotNullOrWhiteSpace(value);
            Guard.IsNotNullOrWhiteSpace(passphrase);

            using Aes aes = Aes.Create();

            aes.Key = DeriveKeyFromPassword(passphrase).ToArray();
            aes.IV = IV;

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

            await cryptoStream.WriteAsync(Encoding.Unicode.GetBytes(value));
            await cryptoStream.FlushFinalBlockAsync();

            byte[] secureStringBytes = memoryStream.ToArray();

            return secureStringBytes;
        }

        public async Task<string> EncryptThenEncodeToBase64(string value, string passphrase)
        {
            Guard.IsNotNullOrWhiteSpace(value);
            Guard.IsNotNullOrWhiteSpace(passphrase);

            byte[] secureStringBytes = await Encrypt(value, passphrase);

            string secureString = Convert.ToBase64String(secureStringBytes);

            return secureString;
        }

        public async Task<string> Decode(byte[] secureStringBytes, string passphrase)
        {
            Guard.IsNotEmpty(secureStringBytes);
            Guard.IsNotNullOrWhiteSpace(passphrase);

            using Aes aes = Aes.Create();
            aes.Key = DeriveKeyFromPassword(passphrase).ToArray();
            aes.IV = IV;

            using MemoryStream memoryStream = new(secureStringBytes);
            using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);

            using MemoryStream memoryStreamCopy = new();

            await cryptoStream.CopyToAsync(memoryStreamCopy);

            byte[] memoryStreamCopyBuffer = memoryStreamCopy.ToArray();

            string value = Encoding.Unicode.GetString(memoryStreamCopyBuffer);

            return value;
        }

        public async Task<string> DecodeFromBase64ThenDecrypt(string secureString, string passphrase)
        {
            Guard.IsNotNullOrWhiteSpace(secureString);
            Guard.IsNotNullOrWhiteSpace(passphrase);

            byte[] secureStringBytes = Convert.FromBase64String(secureString);

            string value = await Decode(secureStringBytes, passphrase);

            return value;
        }
    }
}
