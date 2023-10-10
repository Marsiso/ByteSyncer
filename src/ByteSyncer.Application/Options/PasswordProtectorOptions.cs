using ByteSyncer.Application.External;

namespace ByteSyncer.Application.Options
{
    public class PasswordProtectorOptions
    {
        public required string? Pepper { get; set; }
        public required int SaltSize { get; set; } = 16;
        public required int KeySize { get; set; } = 32;

        public required Argon2Type Algorithm { get; set; } = Argon2Type.Argon2id;

        /// <summary>
        ///     Represents the maximum amount of computations to perform. Raising this number will make the function require more CPU cycles to compute a key.
        ///     This number must be between crypto_pwhash_OPSLIMIT_MIN and crypto_pwhash_OPSLIMIT_MAX.
        /// </summary>
        public required long OperationsLimit { get; set; } = 4;

        /// <summary>
        ///     Memlimit is the maximum amount of RAM in bytes that the function will use.
        ///     This number must be between crypto_pwhash_MEMLIMIT_MIN and crypto_pwhash_MEMLIMIT_MAX.
        /// </summary>
        public required int MemoryLimit { get; set; } = 1_073_741_824;
    }
}
