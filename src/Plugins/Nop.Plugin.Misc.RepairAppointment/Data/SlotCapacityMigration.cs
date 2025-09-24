using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Data
{
    [NopSchemaMigration("2024/12/25 12:00:00", "RepairAppointment add slot capacity table", MigrationProcessType.Update)]
    public class SlotCapacityMigration : AutoReversingMigration
    {
        public override void Up()
        {
            if (!Schema.Table(nameof(SlotCapacity)).Exists())
            {
                Create.TableFor<SlotCapacity>();
            }
        }
    }
}