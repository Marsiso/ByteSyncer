using ByteSyncer.Domain.Application.Models.Common;

namespace ByteSyncer.Domain.Application.Models
{
    public class CodeListItem : ChangeTrackingEntity
    {
        public int CodeListID { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
