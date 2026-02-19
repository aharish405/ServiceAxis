using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceAxis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AssignmentLifecycleSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentStateId",
                table: "PlatformRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StateChangedAt",
                table: "PlatformRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecordAssignments",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AssignedGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecordAssignments_PlatformRecords_RecordId",
                        column: x => x.RecordId,
                        principalTable: "PlatformRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecordStateDefinitions",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StateName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsInitialState = table.Column<bool>(type: "bit", nullable: false),
                    IsFinalState = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordStateDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecordStateDefinitions_SysTables_TableId",
                        column: x => x.TableId,
                        principalTable: "SysTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StateTransitions",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    AllowedRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StateTransitions_RecordStateDefinitions_FromStateId",
                        column: x => x.FromStateId,
                        principalSchema: "platform",
                        principalTable: "RecordStateDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StateTransitions_RecordStateDefinitions_ToStateId",
                        column: x => x.ToStateId,
                        principalSchema: "platform",
                        principalTable: "RecordStateDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StateTransitions_SysTables_TableId",
                        column: x => x.TableId,
                        principalTable: "SysTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRecords_CurrentStateId",
                table: "PlatformRecords",
                column: "CurrentStateId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordAssignments_AssignedGroupId",
                schema: "platform",
                table: "RecordAssignments",
                column: "AssignedGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordAssignments_AssignedUserId",
                schema: "platform",
                table: "RecordAssignments",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordAssignments_RecordId",
                schema: "platform",
                table: "RecordAssignments",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordStateDefinitions_TableId",
                schema: "platform",
                table: "RecordStateDefinitions",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_StateTransitions_FromStateId",
                schema: "platform",
                table: "StateTransitions",
                column: "FromStateId");

            migrationBuilder.CreateIndex(
                name: "IX_StateTransitions_TableId",
                schema: "platform",
                table: "StateTransitions",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_StateTransitions_ToStateId",
                schema: "platform",
                table: "StateTransitions",
                column: "ToStateId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformRecords_RecordStateDefinitions_CurrentStateId",
                table: "PlatformRecords",
                column: "CurrentStateId",
                principalSchema: "platform",
                principalTable: "RecordStateDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlatformRecords_RecordStateDefinitions_CurrentStateId",
                table: "PlatformRecords");

            migrationBuilder.DropTable(
                name: "RecordAssignments",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "StateTransitions",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "RecordStateDefinitions",
                schema: "platform");

            migrationBuilder.DropIndex(
                name: "IX_PlatformRecords_CurrentStateId",
                table: "PlatformRecords");

            migrationBuilder.DropColumn(
                name: "CurrentStateId",
                table: "PlatformRecords");

            migrationBuilder.DropColumn(
                name: "StateChangedAt",
                table: "PlatformRecords");
        }
    }
}
