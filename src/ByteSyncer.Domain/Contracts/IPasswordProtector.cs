namespace ByteSyncer.Domain.Contracts
{
    public interface IPasswordProtector
    {
        public (string, string) HashPassword(string passwordValue);
        public bool VerifyPassword(string passwordValue, string derivedKeyFromOriginalPasswordBase64, string originalPasswordSaltUsedForKeyDerivationBase64);
    }
}
