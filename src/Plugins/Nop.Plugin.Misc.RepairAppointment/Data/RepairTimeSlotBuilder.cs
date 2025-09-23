using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Data
{
    public class RepairTimeSlotBuilder : NopEntityBuilder<RepairTimeSlot>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(RepairTimeSlot.Date)).AsDateTime().NotNullable()
                .WithColumn(nameof(RepairTimeSlot.TimeSlot)).AsString(50).NotNullable()
                .WithColumn(nameof(RepairTimeSlot.StartTime)).AsTime().NotNullable()
                .WithColumn(nameof(RepairTimeSlot.EndTime)).AsTime().NotNullable()
                .WithColumn(nameof(RepairTimeSlot.MaxAppointments)).AsInt32().NotNullable()
                .WithColumn(nameof(RepairTimeSlot.CurrentBookings)).AsInt32().NotNullable()
                .WithColumn(nameof(RepairTimeSlot.IsAvailable)).AsBoolean().NotNullable()
                .WithColumn(nameof(RepairTimeSlot.IsBlocked)).AsBoolean().NotNullable()
                .WithColumn(nameof(RepairTimeSlot.BlockReason)).AsString(200).Nullable();
        }
    }
}