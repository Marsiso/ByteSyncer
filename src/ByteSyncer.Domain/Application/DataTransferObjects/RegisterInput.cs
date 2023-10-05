namespace ByteSyncer.Domain.Application.DataTransferObjects
{
    public class RegisterInput
    {
        public string? Email { get; set; }
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
        public string? Password { get; set; }
        public string? PasswordRepeat { get; set; }
    }
}
