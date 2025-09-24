using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.RepairAppointment.Data
{
    [NopSchemaMigration("2024/12/25 13:00:00", "RepairAppointment convert TimeSlotId to string", MigrationProcessType.Update)]
    public class TimeSlotIdMigration : Migration
    {
        public override void Up()
        {
            if (Schema.Table("RepairAppointment").Column("TimeSlotId").Exists())
            {
                // Drop the old integer column and create a new string column
                Delete.Column("TimeSlotId").FromTable("RepairAppointment");
                Alter.Table("RepairAppointment")
                    .AddColumn("TimeSlotId").AsString(100).NotNullable().WithDefaultValue("");
            }
        }

        public override void Down()
        {
            if (Schema.Table("RepairAppointment").Column("TimeSlotId").Exists())
            {
                // Revert back to integer column
                Delete.Column("TimeSlotId").FromTable("RepairAppointment");
                Alter.Table("RepairAppointment")
                    .AddColumn("TimeSlotId").AsInt32().NotNullable().WithDefaultValue(0);
            }
        }
    }
}