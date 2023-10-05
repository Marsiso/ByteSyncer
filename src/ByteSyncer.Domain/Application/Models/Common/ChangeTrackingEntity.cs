namespace ByteSyncer.Domain.Application.Models.Common
{
    public class ChangeTrackingEntity : EntityBase
    {
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public User? UserCreatedBy { get; set; }
        public User? UserUpdatedBy { get; set; }
    }
}
