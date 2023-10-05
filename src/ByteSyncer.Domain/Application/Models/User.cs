using ByteSyncer.Domain.Application.Models.Common;
using ByteSyncer.Domain.Files.Models;

namespace ByteSyncer.Domain.Application.Models
{
    public class User : ChangeTrackingEntity
    {
        public int RootFolderID { get; set; }
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? SecurityStamp { get; set; }


        public Folder? RootFolder { get; set; }
        public ICollection<UserRole>? UserRoles { get; set; }
    }
}
