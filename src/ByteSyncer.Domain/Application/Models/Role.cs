using ByteSyncer.Domain.Application.Models.Common;

namespace ByteSyncer.Domain.Application.Models
{
    public class Role : ChangeTrackingEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Permissions { get; set; } = string.Empty;

        public ICollection<UserRole>? UserRoles { get; set; }
    }
}
