using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Data
{
    public class RepairProductBuilder : NopEntityBuilder<RepairProduct>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(RepairProduct.RepairCategoryId)).AsInt32().NotNullable()
                .WithColumn(nameof(RepairProduct.Name)).AsString(200).NotNullable()
                .WithColumn(nameof(RepairProduct.Brand)).AsString(100).Nullable()
                .WithColumn(nameof(RepairProduct.Model)).AsString(100).Nullable()
                .WithColumn(nameof(RepairProduct.Description)).AsString(1000).Nullable()
                .WithColumn(nameof(RepairProduct.IsActive)).AsBoolean().NotNullable()
                .WithColumn(nameof(RepairProduct.DisplayOrder)).AsInt32().NotNullable()
                .WithColumn(nameof(RepairProduct.CreatedOnUtc)).AsDateTime().NotNullable()
                .WithColumn(nameof(RepairProduct.UpdatedOnUtc)).AsDateTime().Nullable();
        }
    }
}