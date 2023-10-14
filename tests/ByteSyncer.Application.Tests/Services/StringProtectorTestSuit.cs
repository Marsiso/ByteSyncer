using ByteSyncer.Application.Options;
using ByteSyncer.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace ByteSyncer.Application.Tests.Services
{
    public class StringProtectorTestSuit
    {
        [Fact]
        public async Task EncryptThenEncodeToBase64_WhenGivenValidArguments_ThenReturnEncryptedStringEncodedToBase64()
        {
            // Arrange
            const string passphrase = nameof(passphrase);
            const string originalValue = nameof(originalValue);

            StringProtectorOptions options = new StringProtectorOptions
            {
                Cycles = 256_000,
                KeySize = 32,
                SaltSize = 16,
                Pepper = "SecurePepper"
            };

            IOptions<StringProtectorOptions> optionsProvider = Microsoft.Extensions.Options.Options.Create<StringProtectorOptions>(options);
            StringProtector stringProtector = new StringProtector(optionsProvider);

            // Act
            string encryptedValueEncodedToBase64 = await stringProtector.EncryptThenEncodeToBase64(passphrase, originalValue);

            // Assert
            encryptedValueEncodedToBase64.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task DecryptThenDecodeFromBase64_WhenGivenValidArguments_ThenReturnDecryptedStringDecodedFromBase64()
        {
            // Arrange
            const string passphrase = nameof(passphrase);
            const string originalValue = nameof(originalValue);

            StringProtectorOptions options = new StringProtectorOptions
            {
                Cycles = 256_000,
                KeySize = 32,
                SaltSize = 16,
                Pepper = "SecurePepper"
            };

            IOptions<StringProtectorOptions> optionsProvider = Microsoft.Extensions.Options.Options.Create<StringProtectorOptions>(options);
            StringProtector stringProtector = new StringProtector(optionsProvider);

            // Act
            string encrypedValueEncodedToBase64 = await stringProtector.EncryptThenEncodeToBase64(originalValue, passphrase);
            string decryptedValueDecodedFromBase64 = await stringProtector.DecodeFromBase64ThenDecrypt(encrypedValueEncodedToBase64, passphrase);

            // Assert
            encrypedValueEncodedToBase64.Should().NotBeNullOrWhiteSpace();

            decryptedValueDecodedFromBase64.Should().NotBeNullOrWhiteSpace();
            decryptedValueDecodedFromBase64.Should().BeEquivalentTo(originalValue);
        }
    }
}
