using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Data
{
    public class RepairAppointmentBuilder : NopEntityBuilder<Domain.RepairAppointment>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Domain.RepairAppointment.CustomerName)).AsString(200).NotNullable()
                .WithColumn(nameof(Domain.RepairAppointment.Email)).AsString(200).NotNullable()
                .WithColumn(nameof(Domain.RepairAppointment.Phone)).AsString(50).NotNullable()
                .WithColumn(nameof(Domain.RepairAppointment.DeviceType)).AsString(50).NotNullable()
                .WithColumn(nameof(Domain.RepairAppointment.DeviceBrand)).AsString(100).Nullable()
                .WithColumn(nameof(Domain.RepairAppointment.DeviceModel)).AsString(100).Nullable()
                .WithColumn(nameof(Domain.RepairAppointment.IssueDescription)).AsString(1000).NotNullable()
                .WithColumn(nameof(Domain.RepairAppointment.RepairCategoryId)).AsInt32().Nullable()
                .WithColumn(nameof(Domain.RepairAppointment.RepairProductId)).AsInt32().Nullable()
                .WithColumn(nameof(Domain.RepairAppointment.RepairTypeId)).AsInt32().Nullable()
                .WithColumn(nameof(Domain.RepairAppointment.EstimatedPrice)).AsDecimal(18, 4).Nullable()
                .WithColumn(nameof(Domain.RepairAppointment.AppointmentDate)).AsDateTime().NotNullable()
                .WithColumn(nameof(Domain.RepairAppointment.TimeSlot)).AsString(50).NotNullable()
                .WithColumn(nameof(Domain.RepairAppointment.TimeSlotId)).AsInt32().NotNullable()
                .WithColumn(nameof(Domain.RepairAppointment.Status)).AsInt32().NotNullable()
                .WithColumn(nameof(Domain.RepairAppointment.CustomerId)).AsInt32().Nullable()
                .WithColumn(nameof(Domain.RepairAppointment.Notes)).AsString(500).Nullable()
                .WithColumn(nameof(Domain.RepairAppointment.ConfirmationCode)).AsString(50).Nullable()
                .WithColumn(nameof(Domain.RepairAppointment.CreatedOnUtc)).AsDateTime().NotNullable()
                .WithColumn(nameof(Domain.RepairAppointment.ModifiedOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(Domain.RepairAppointment.ReminderSent)).AsBoolean().NotNullable()
                .WithColumn(nameof(Domain.RepairAppointment.ConfirmationSent)).AsBoolean().NotNullable();
        }
    }
}