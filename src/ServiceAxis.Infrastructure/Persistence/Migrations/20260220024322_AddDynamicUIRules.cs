using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceAxis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicUIRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutomationActions_AutomationRules_RuleId",
                schema: "platform",
                table: "AutomationActions");

            migrationBuilder.DropForeignKey(
                name: "FK_AutomationConditions_AutomationRules_RuleId",
                schema: "platform",
                table: "AutomationConditions");

            migrationBuilder.DropForeignKey(
                name: "FK_AutomationTriggers_AutomationRules_RuleId",
                schema: "platform",
                table: "AutomationTriggers");

            migrationBuilder.CreateTable(
                name: "ClientScripts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    TriggerFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScriptCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExecutionOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientScripts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientScripts_SysFields_TriggerFieldId",
                        column: x => x.TriggerFieldId,
                        principalTable: "SysFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientScripts_SysTables_TableId",
                        column: x => x.TableId,
                        principalTable: "SysTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FieldRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TriggerFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConditionJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    ValueExpression = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExecutionOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldRules_SysFields_TargetFieldId",
                        column: x => x.TargetFieldId,
                        principalTable: "SysFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FieldRules_SysFields_TriggerFieldId",
                        column: x => x.TriggerFieldId,
                        principalTable: "SysFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FieldRules_SysTables_TableId",
                        column: x => x.TableId,
                        principalTable: "SysTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UiPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutionOrder = table.Column<int>(type: "int", nullable: false),
                    FormContext = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UiPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UiPolicies_SysTables_TableId",
                        column: x => x.TableId,
                        principalTable: "SysTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UiPolicyActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UiPolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UiPolicyActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UiPolicyActions_SysFields_TargetFieldId",
                        column: x => x.TargetFieldId,
                        principalTable: "SysFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UiPolicyActions_UiPolicies_UiPolicyId",
                        column: x => x.UiPolicyId,
                        principalTable: "UiPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UiPolicyConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UiPolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_UiPolicyConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UiPolicyConditions_SysFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "SysFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UiPolicyConditions_UiPolicies_UiPolicyId",
                        column: x => x.UiPolicyId,
                        principalTable: "UiPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientScripts_TableId",
                table: "ClientScripts",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientScripts_TriggerFieldId",
                table: "ClientScripts",
                column: "TriggerFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldRules_TableId",
                table: "FieldRules",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldRules_TargetFieldId",
                table: "FieldRules",
                column: "TargetFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldRules_TriggerFieldId",
                table: "FieldRules",
                column: "TriggerFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_UiPolicies_TableId",
                table: "UiPolicies",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_UiPolicyActions_TargetFieldId",
                table: "UiPolicyActions",
                column: "TargetFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_UiPolicyActions_UiPolicyId",
                table: "UiPolicyActions",
                column: "UiPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_UiPolicyConditions_FieldId",
                table: "UiPolicyConditions",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_UiPolicyConditions_UiPolicyId",
                table: "UiPolicyConditions",
                column: "UiPolicyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AutomationActions_AutomationRules_RuleId",
                schema: "platform",
                table: "AutomationActions",
                column: "RuleId",
                principalSchema: "platform",
                principalTable: "AutomationRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AutomationConditions_AutomationRules_RuleId",
                schema: "platform",
                table: "AutomationConditions",
                column: "RuleId",
                principalSchema: "platform",
                principalTable: "AutomationRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AutomationTriggers_AutomationRules_RuleId",
                schema: "platform",
                table: "AutomationTriggers",
                column: "RuleId",
                principalSchema: "platform",
                principalTable: "AutomationRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutomationActions_AutomationRules_RuleId",
                schema: "platform",
                table: "AutomationActions");

            migrationBuilder.DropForeignKey(
                name: "FK_AutomationConditions_AutomationRules_RuleId",
                schema: "platform",
                table: "AutomationConditions");

            migrationBuilder.DropForeignKey(
                name: "FK_AutomationTriggers_AutomationRules_RuleId",
                schema: "platform",
                table: "AutomationTriggers");

            migrationBuilder.DropTable(
                name: "ClientScripts");

            migrationBuilder.DropTable(
                name: "FieldRules");

            migrationBuilder.DropTable(
                name: "UiPolicyActions");

            migrationBuilder.DropTable(
                name: "UiPolicyConditions");

            migrationBuilder.DropTable(
                name: "UiPolicies");

            migrationBuilder.AddForeignKey(
                name: "FK_AutomationActions_AutomationRules_RuleId",
                schema: "platform",
                table: "AutomationActions",
                column: "RuleId",
                principalSchema: "platform",
                principalTable: "AutomationRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AutomationConditions_AutomationRules_RuleId",
                schema: "platform",
                table: "AutomationConditions",
                column: "RuleId",
                principalSchema: "platform",
                principalTable: "AutomationRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AutomationTriggers_AutomationRules_RuleId",
                schema: "platform",
                table: "AutomationTriggers",
                column: "RuleId",
                principalSchema: "platform",
                principalTable: "AutomationRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
