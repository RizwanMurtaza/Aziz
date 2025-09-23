using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Data
{
    public class RepairTypeBuilder : NopEntityBuilder<RepairType>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(RepairType.RepairCategoryId)).AsInt32().NotNullable()
                .WithColumn(nameof(RepairType.RepairProductId)).AsInt32().Nullable()
                .WithColumn(nameof(RepairType.Name)).AsString(200).NotNullable()
                .WithColumn(nameof(RepairType.Description)).AsString(1000).Nullable()
                .WithColumn(nameof(RepairType.EstimatedPrice)).AsDecimal(18, 4).NotNullable()
                .WithColumn(nameof(RepairType.EstimatedDurationMinutes)).AsInt32().NotNullable()
                .WithColumn(nameof(RepairType.IsActive)).AsBoolean().NotNullable()
                .WithColumn(nameof(RepairType.DisplayOrder)).AsInt32().NotNullable()
                .WithColumn(nameof(RepairType.CreatedOnUtc)).AsDateTime().NotNullable()
                .WithColumn(nameof(RepairType.UpdatedOnUtc)).AsDateTime().Nullable();
        }
    }
}