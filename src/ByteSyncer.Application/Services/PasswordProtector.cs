﻿using System.Security.Cryptography;
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

        public (string, string) HashPassword(string password)
        {
            Guard.IsNotNullOrWhiteSpace(password);

            string passwordWithPepper = password + Options.Pepper;

            byte[] keyBytes = new byte[Options.KeySize];
            byte[] saltBytes = new byte[Options.SaltSize];

            SodiumLibrary.randombytes_buf(saltBytes, saltBytes.Length);

            int result = SodiumLibrary.crypto_pwhash(
                keyBytes,
                keyBytes.Length,
                Encoding.UTF8.GetBytes(passwordWithPepper),
                passwordWithPepper.Length,
                saltBytes,
                Options.OperationsLimit,
                Options.MemoryLimit,
                (int)Options.Algorithm);

            Guard.Equals(result, 0);

            return (Convert.ToBase64String(keyBytes), Convert.ToBase64String(saltBytes));
        }

        public bool VerifyPassword(string password, string passwordKey, string passwordSalt)
        {
            Guard.IsNotNullOrWhiteSpace(password);
            Guard.IsNotNullOrWhiteSpace(passwordKey);
            Guard.IsNotNullOrWhiteSpace(passwordSalt);

            string passwordWithPepper = password + Options.Pepper;

            byte[] keyBytes = new byte[Options.KeySize];

            byte[] originalKeyBytes = Convert.FromBase64String(passwordKey);
            byte[] originalSaltBytes = Convert.FromBase64String(passwordSalt);

            int result = SodiumLibrary.crypto_pwhash(
                keyBytes,
                keyBytes.Length,
                Encoding.UTF8.GetBytes(passwordWithPepper),
                passwordWithPepper.Length,
                originalSaltBytes,
                Options.OperationsLimit,
                Options.MemoryLimit,
                (int)Options.Algorithm);

            Guard.Equals(result, 0);

            return CryptographicOperations.FixedTimeEquals(keyBytes, originalKeyBytes);
        }
    }
}
