using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Data
{
    [NopSchemaMigration("2024/01/01 12:00:00", "RepairAppointment base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<RepairCategory>();
            Create.TableFor<RepairProduct>();
            Create.TableFor<RepairType>();
            Create.TableFor<Domain.RepairAppointment>();
            Create.TableFor<RepairTimeSlot>();
        }
    }
}