namespace ByteSyncer.Domain.Contracts
{
    public interface IPasswordProtector
    {
        public (string, string) HashPassword(string password);
        public bool VerifyPassword(string password, string passwordKey, string passwordSalt);
    }
}
