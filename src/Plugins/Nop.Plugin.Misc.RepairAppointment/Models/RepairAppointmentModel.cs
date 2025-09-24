using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.RepairAppointment.Models
{
    public record RepairAppointmentModel : BaseNopEntityModel
    {
        public RepairAppointmentModel()
        {
            AvailableDeviceTypes = new List<SelectListItem>();
            AvailableTimeSlots = new List<SelectListItem>();
            AvailableStatuses = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
            AvailableProducts = new List<SelectListItem>();
            AvailableRepairTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.CustomerName")]
        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.Email")]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.Phone")]
        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.DeviceType")]
        [Required]
        public string DeviceType { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.DeviceBrand")]
        public string? DeviceBrand { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.DeviceModel")]
        public string? DeviceModel { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.IssueDescription")]
        [Required]
        public string IssueDescription { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.RepairCategory")]
        public int? RepairCategoryId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.RepairProduct")]
        public int? RepairProductId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.RepairType")]
        public int? RepairTypeId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.EstimatedPrice")]
        public decimal? EstimatedPrice { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.AppointmentDate")]
        [Required]
        public DateTime AppointmentDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.TimeSlot")]
        public string? TimeSlot { get; set; }

        public int TimeSlotId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Fields.Status")]
        public int StatusId { get; set; }

        public string? StatusName { get; set; }

        public string? Notes { get; set; }

        public string? ConfirmationCode { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string? RepairCategoryName { get; set; }
        public string? RepairProductName { get; set; }
        public string? RepairTypeName { get; set; }

        public IList<SelectListItem> AvailableDeviceTypes { get; set; }
        public IList<SelectListItem> AvailableTimeSlots { get; set; }
        public IList<SelectListItem> AvailableStatuses { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableProducts { get; set; }
        public IList<SelectListItem> AvailableRepairTypes { get; set; }
    }

    public record RepairAppointmentListModel : BasePagedListModel<RepairAppointmentModel>
    {
    }

    public record RepairAppointmentSearchModel : BaseSearchModel
    {
        public RepairAppointmentSearchModel()
        {
            AvailableStatuses = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Search.FromDate")]
        public DateTime? FromDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Search.ToDate")]
        public DateTime? ToDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Search.Status")]
        public int StatusId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Search.SearchText")]
        public string? SearchText { get; set; }

        public IList<SelectListItem> AvailableStatuses { get; set; }
    }
}