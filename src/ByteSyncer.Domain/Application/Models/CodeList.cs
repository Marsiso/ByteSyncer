using ByteSyncer.Domain.Application.Models.Common;

namespace ByteSyncer.Domain.Application.Models
{
    public class CodeList : ChangeTrackingEntity
    {
        public string Name { get; set; } = string.Empty;


        public ICollection<CodeListItem>? CodeListItems { get; set; }
    }
}
