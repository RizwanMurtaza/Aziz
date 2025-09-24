using Nop.Core;
using System;

namespace Nop.Plugin.Misc.RepairAppointment.Domain
{
    public class RepairAppointment : BaseEntity
    {
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string? DeviceBrand { get; set; }
        public string? DeviceModel { get; set; }
        public string IssueDescription { get; set; } = string.Empty;
        public int? RepairCategoryId { get; set; }
        public int? RepairProductId { get; set; }
        public int? RepairTypeId { get; set; }
        public decimal? EstimatedPrice { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string TimeSlotId { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; }
        public int? CustomerId { get; set; }
        public string? Notes { get; set; }
        public string ConfirmationCode { get; set; } = string.Empty;
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? ModifiedOnUtc { get; set; }
        public bool ReminderSent { get; set; }
        public bool ConfirmationSent { get; set; }
    }

}