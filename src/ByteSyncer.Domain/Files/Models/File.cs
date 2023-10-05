using ByteSyncer.Domain.Application.Models.Common;

namespace ByteSyncer.Domain.Files.Models
{
    public class File : ChangeTrackingEntity
    {
        public int FolderID { get; set; }
        public string SafeName { get; set; } = string.Empty;
        public string UnsafeName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public long Size { get; set; }

        public Folder? Folder { get; set; }
    }
}
