namespace ByteSyncer.Application.Options
{
    public class StringProtectorOptions
    {
        public const string SectionName = "StringProtector";

        public required int Cycles = 256_000;

        /// <summary>
        ///     Defaults to 32B.
        /// </summary>
        public required int KeySize = 32;

        /// <summary>
        ///     Defaults to 16 B.
        /// </summary>
        public required int SaltSize = 16;

        public required string? Pepper { get; set; }
    }
}
