using ByteSyncer.Domain.Application.Models.Common;

namespace ByteSyncer.Domain.Application.Models
{
    public class User : ChangeTrackingEntity
    {
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? PasswordSalt { get; set; }
    }
}
