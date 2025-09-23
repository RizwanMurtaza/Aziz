using Nop.Core;

namespace Nop.Plugin.Misc.RepairAppointment.Domain
{
    public class RepairType : BaseEntity
    {
        public int RepairCategoryId { get; set; }
        public int? RepairProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedPrice { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }
}