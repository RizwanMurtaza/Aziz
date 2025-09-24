using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Data
{
    public class SlotCapacityBuilder : NopEntityBuilder<SlotCapacity>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SlotCapacity.Date)).AsDate().NotNullable()
                .WithColumn(nameof(SlotCapacity.StartTime)).AsTime().NotNullable()
                .WithColumn(nameof(SlotCapacity.EndTime)).AsTime().NotNullable()
                .WithColumn(nameof(SlotCapacity.MaxAppointments)).AsInt32().NotNullable()
                .WithColumn(nameof(SlotCapacity.CurrentBookings)).AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn(nameof(SlotCapacity.IsActive)).AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn(nameof(SlotCapacity.Notes)).AsString(500).Nullable()
                .WithColumn(nameof(SlotCapacity.CreatedOnUtc)).AsDateTime2().NotNullable()
                .WithColumn(nameof(SlotCapacity.ModifiedOnUtc)).AsDateTime2().Nullable();
        }
    }
}