using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceAxis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutomationRules",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExecutionMode = table.Column<int>(type: "int", nullable: false),
                    StopProcessingOnMatch = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRules_SysTables_TableId",
                        column: x => x.TableId,
                        principalTable: "SysTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AutomationActions",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationActions_AutomationRules_RuleId",
                        column: x => x.RuleId,
                        principalSchema: "platform",
                        principalTable: "AutomationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AutomationConditions",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Operator = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogicalGroup = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationConditions_AutomationRules_RuleId",
                        column: x => x.RuleId,
                        principalSchema: "platform",
                        principalTable: "AutomationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AutomationConditions_SysFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "SysFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AutomationExecutionLogs",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExecutionStatus = table.Column<int>(type: "int", nullable: false),
                    ExecutionMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationExecutionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationExecutionLogs_AutomationRules_RuleId",
                        column: x => x.RuleId,
                        principalSchema: "platform",
                        principalTable: "AutomationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AutomationTriggers",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationTriggers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationTriggers_AutomationRules_RuleId",
                        column: x => x.RuleId,
                        principalSchema: "platform",
                        principalTable: "AutomationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationActions_RuleId",
                schema: "platform",
                table: "AutomationActions",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationConditions_FieldId",
                schema: "platform",
                table: "AutomationConditions",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationConditions_RuleId",
                schema: "platform",
                table: "AutomationConditions",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationExecutionLogs_RuleId",
                schema: "platform",
                table: "AutomationExecutionLogs",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_TableId",
                schema: "platform",
                table: "AutomationRules",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationTriggers_RuleId",
                schema: "platform",
                table: "AutomationTriggers",
                column: "RuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationActions",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "AutomationConditions",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "AutomationExecutionLogs",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "AutomationTriggers",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "AutomationRules",
                schema: "platform");
        }
    }
}
