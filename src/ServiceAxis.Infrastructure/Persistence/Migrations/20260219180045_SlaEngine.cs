using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceAxis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SlaEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SlaInstances_SlaDefinitions_SlaDefinitionId",
                table: "SlaInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_SlaInstances_SlaPolicies_SlaPolicyId",
                table: "SlaInstances");

            migrationBuilder.DropIndex(
                name: "IX_SlaInstances_SlaDefinitionId",
                table: "SlaInstances");

            migrationBuilder.DropIndex(
                name: "IX_SlaInstances_SlaPolicyId",
                table: "SlaInstances");

            migrationBuilder.DropColumn(
                name: "EscalateOnBreach",
                table: "SlaPolicies");

            migrationBuilder.DropColumn(
                name: "NotifyOnBreach",
                table: "SlaPolicies");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "SlaPolicies");

            migrationBuilder.DropColumn(
                name: "ResolutionTimeMinutes",
                table: "SlaPolicies");

            migrationBuilder.DropColumn(
                name: "ResponseTimeMinutes",
                table: "SlaPolicies");

            migrationBuilder.DropColumn(
                name: "WarningThresholdPercent",
                table: "SlaPolicies");

            migrationBuilder.DropColumn(
                name: "PausedAt",
                table: "SlaInstances");

            migrationBuilder.DropColumn(
                name: "ResolutionBreachFired",
                table: "SlaInstances");

            migrationBuilder.DropColumn(
                name: "ResolutionDueAt",
                table: "SlaInstances");

            migrationBuilder.DropColumn(
                name: "ResolutionWarningFired",
                table: "SlaInstances");

            migrationBuilder.DropColumn(
                name: "TableName",
                table: "SlaInstances");

            migrationBuilder.DropColumn(
                name: "BusinessEndHour",
                table: "SlaDefinitions");

            migrationBuilder.DropColumn(
                name: "BusinessStartHour",
                table: "SlaDefinitions");

            migrationBuilder.DropColumn(
                name: "IsSystemDefinition",
                table: "SlaDefinitions");

            migrationBuilder.DropColumn(
                name: "ScheduleType",
                table: "SlaDefinitions");

            migrationBuilder.DropColumn(
                name: "TableName",
                table: "SlaDefinitions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "SlaDefinitions");

            migrationBuilder.DropColumn(
                name: "WorkingDaysJson",
                table: "SlaDefinitions");

            migrationBuilder.RenameTable(
                name: "SlaPolicies",
                newName: "SlaPolicies",
                newSchema: "platform");

            migrationBuilder.RenameTable(
                name: "SlaInstances",
                newName: "SlaInstances",
                newSchema: "platform");

            migrationBuilder.RenameTable(
                name: "SlaDefinitions",
                newName: "SlaDefinitions",
                newSchema: "platform");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                schema: "platform",
                table: "SlaInstances",
                newName: "TargetTime");

            migrationBuilder.RenameColumn(
                name: "SlaPolicyId",
                schema: "platform",
                table: "SlaInstances",
                newName: "TableId");

            migrationBuilder.RenameColumn(
                name: "ResponseWarningFired",
                schema: "platform",
                table: "SlaInstances",
                newName: "IsPaused");

            migrationBuilder.RenameColumn(
                name: "ResponseDueAt",
                schema: "platform",
                table: "SlaInstances",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "ResponseBreachFired",
                schema: "platform",
                table: "SlaInstances",
                newName: "IsBreached");

            migrationBuilder.RenameColumn(
                name: "RespondedAt",
                schema: "platform",
                table: "SlaInstances",
                newName: "CompletedTime");

            migrationBuilder.RenameColumn(
                name: "ResolvedAt",
                schema: "platform",
                table: "SlaInstances",
                newName: "BreachTime");

            migrationBuilder.RenameColumn(
                name: "PausedMinutes",
                schema: "platform",
                table: "SlaInstances",
                newName: "MetricType");

            migrationBuilder.AddColumn<Guid>(
                name: "PriorityFieldId",
                schema: "platform",
                table: "SlaPolicies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriorityValue",
                schema: "platform",
                table: "SlaPolicies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TableId",
                schema: "platform",
                table: "SlaPolicies",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TableId",
                schema: "platform",
                table: "SlaDefinitions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "BusinessCalendars",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkingDays = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartHour = table.Column<int>(type: "int", nullable: false),
                    EndHour = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessCalendars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SlaEscalationRules",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlaDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TriggerType = table.Column<int>(type: "int", nullable: false),
                    OffsetMinutes = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    TargetUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaEscalationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlaEscalationRules_SlaDefinitions_SlaDefinitionId",
                        column: x => x.SlaDefinitionId,
                        principalSchema: "platform",
                        principalTable: "SlaDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SlaTimerEvents",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlaInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    EventTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TriggeredBySystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaTimerEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlaTimerEvents_SlaInstances_SlaInstanceId",
                        column: x => x.SlaInstanceId,
                        principalSchema: "platform",
                        principalTable: "SlaInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SlaTargets",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlaDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MetricType = table.Column<int>(type: "int", nullable: false),
                    TargetDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    BusinessHoursOnly = table.Column<bool>(type: "bit", nullable: false),
                    BusinessCalendarId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlaTargets_BusinessCalendars_BusinessCalendarId",
                        column: x => x.BusinessCalendarId,
                        principalSchema: "platform",
                        principalTable: "BusinessCalendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SlaTargets_SlaDefinitions_SlaDefinitionId",
                        column: x => x.SlaDefinitionId,
                        principalSchema: "platform",
                        principalTable: "SlaDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SlaInstances_IsBreached_Status",
                schema: "platform",
                table: "SlaInstances",
                columns: new[] { "IsBreached", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SlaInstances_IsPaused",
                schema: "platform",
                table: "SlaInstances",
                column: "IsPaused");

            migrationBuilder.CreateIndex(
                name: "IX_SlaInstances_RecordId",
                schema: "platform",
                table: "SlaInstances",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_SlaInstances_TargetTime",
                schema: "platform",
                table: "SlaInstances",
                column: "TargetTime");

            migrationBuilder.CreateIndex(
                name: "IX_SlaEscalationRules_SlaDefinitionId",
                schema: "platform",
                table: "SlaEscalationRules",
                column: "SlaDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_SlaTargets_BusinessCalendarId",
                schema: "platform",
                table: "SlaTargets",
                column: "BusinessCalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_SlaTargets_SlaDefinitionId",
                schema: "platform",
                table: "SlaTargets",
                column: "SlaDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_SlaTimerEvents_SlaInstanceId",
                schema: "platform",
                table: "SlaTimerEvents",
                column: "SlaInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SlaEscalationRules",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "SlaTargets",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "SlaTimerEvents",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "BusinessCalendars",
                schema: "platform");

            migrationBuilder.DropIndex(
                name: "IX_SlaInstances_IsBreached_Status",
                schema: "platform",
                table: "SlaInstances");

            migrationBuilder.DropIndex(
                name: "IX_SlaInstances_IsPaused",
                schema: "platform",
                table: "SlaInstances");

            migrationBuilder.DropIndex(
                name: "IX_SlaInstances_RecordId",
                schema: "platform",
                table: "SlaInstances");

            migrationBuilder.DropIndex(
                name: "IX_SlaInstances_TargetTime",
                schema: "platform",
                table: "SlaInstances");

            migrationBuilder.DropColumn(
                name: "PriorityFieldId",
                schema: "platform",
                table: "SlaPolicies");

            migrationBuilder.DropColumn(
                name: "PriorityValue",
                schema: "platform",
                table: "SlaPolicies");

            migrationBuilder.DropColumn(
                name: "TableId",
                schema: "platform",
                table: "SlaPolicies");

            migrationBuilder.DropColumn(
                name: "TableId",
                schema: "platform",
                table: "SlaDefinitions");

            migrationBuilder.RenameTable(
                name: "SlaPolicies",
                schema: "platform",
                newName: "SlaPolicies");

            migrationBuilder.RenameTable(
                name: "SlaInstances",
                schema: "platform",
                newName: "SlaInstances");

            migrationBuilder.RenameTable(
                name: "SlaDefinitions",
                schema: "platform",
                newName: "SlaDefinitions");

            migrationBuilder.RenameColumn(
                name: "TargetTime",
                table: "SlaInstances",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "TableId",
                table: "SlaInstances",
                newName: "SlaPolicyId");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "SlaInstances",
                newName: "ResponseDueAt");

            migrationBuilder.RenameColumn(
                name: "MetricType",
                table: "SlaInstances",
                newName: "PausedMinutes");

            migrationBuilder.RenameColumn(
                name: "IsPaused",
                table: "SlaInstances",
                newName: "ResponseWarningFired");

            migrationBuilder.RenameColumn(
                name: "IsBreached",
                table: "SlaInstances",
                newName: "ResponseBreachFired");

            migrationBuilder.RenameColumn(
                name: "CompletedTime",
                table: "SlaInstances",
                newName: "RespondedAt");

            migrationBuilder.RenameColumn(
                name: "BreachTime",
                table: "SlaInstances",
                newName: "ResolvedAt");

            migrationBuilder.AddColumn<bool>(
                name: "EscalateOnBreach",
                table: "SlaPolicies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnBreach",
                table: "SlaPolicies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "SlaPolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ResolutionTimeMinutes",
                table: "SlaPolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ResponseTimeMinutes",
                table: "SlaPolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WarningThresholdPercent",
                table: "SlaPolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PausedAt",
                table: "SlaInstances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ResolutionBreachFired",
                table: "SlaInstances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolutionDueAt",
                table: "SlaInstances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "ResolutionWarningFired",
                table: "SlaInstances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TableName",
                table: "SlaInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BusinessEndHour",
                table: "SlaDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BusinessStartHour",
                table: "SlaDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemDefinition",
                table: "SlaDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleType",
                table: "SlaDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TableName",
                table: "SlaDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "SlaDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WorkingDaysJson",
                table: "SlaDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SlaInstances_SlaDefinitionId",
                table: "SlaInstances",
                column: "SlaDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_SlaInstances_SlaPolicyId",
                table: "SlaInstances",
                column: "SlaPolicyId");

            migrationBuilder.AddForeignKey(
                name: "FK_SlaInstances_SlaDefinitions_SlaDefinitionId",
                table: "SlaInstances",
                column: "SlaDefinitionId",
                principalTable: "SlaDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SlaInstances_SlaPolicies_SlaPolicyId",
                table: "SlaInstances",
                column: "SlaPolicyId",
                principalTable: "SlaPolicies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
