using ByteSyncer.Domain.Application.Models.Common;

namespace ByteSyncer.Domain.Files.Models
{
    public class Folder : ChangeTrackingEntity
    {
        public int? ParentID { get; set; }
        public string Name { get; set; } = string.Empty;

        public Folder? Parent { get; set; }
        public ICollection<File>? Files { get; set; }
        public ICollection<Folder>? Children { get; set; }
    }
}
