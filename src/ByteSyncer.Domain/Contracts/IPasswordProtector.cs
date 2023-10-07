namespace ByteSyncer.Domain.Contracts
{
    public interface IPasswordProtector
    {
        public string HashPassword(string password);
        public bool VerifyPassword(string password, string passwordHash);
    }
}
