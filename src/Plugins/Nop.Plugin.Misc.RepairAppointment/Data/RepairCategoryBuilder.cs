using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Data
{
    public class RepairCategoryBuilder : NopEntityBuilder<RepairCategory>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(RepairCategory.Name)).AsString(200).NotNullable()
                .WithColumn(nameof(RepairCategory.Description)).AsString(1000).Nullable()
                .WithColumn(nameof(RepairCategory.IsActive)).AsBoolean().NotNullable()
                .WithColumn(nameof(RepairCategory.DisplayOrder)).AsInt32().NotNullable()
                .WithColumn(nameof(RepairCategory.CreatedOnUtc)).AsDateTime().NotNullable()
                .WithColumn(nameof(RepairCategory.UpdatedOnUtc)).AsDateTime().Nullable();
        }
    }
}