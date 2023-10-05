using ByteSyncer.Domain.Application.Models.Common;

namespace ByteSyncer.Domain.Application.Models
{
    public class UserRole : ChangeTrackingEntity
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }

        public User? User { get; set; }
        public Role? Role { get; set; }
    }
}
