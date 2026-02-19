using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceAxis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationshipsFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_ApplicationUsers_UserId1",
                schema: "platform",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PlatformRecords_SysTables_TableId1",
                table: "PlatformRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordValues_PlatformRecords_RecordId",
                table: "RecordValues");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordValues_SysFields_FieldId1",
                table: "RecordValues");

            migrationBuilder.DropForeignKey(
                name: "FK_SysChoices_SysFields_FieldId",
                table: "SysChoices");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowTriggers_SysTables_TableId1",
                table: "WorkflowTriggers");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowTriggers_TableId1",
                table: "WorkflowTriggers");

            migrationBuilder.DropIndex(
                name: "IX_RecordValues_FieldId1",
                table: "RecordValues");

            migrationBuilder.DropIndex(
                name: "IX_PlatformRecords_TableId1",
                table: "PlatformRecords");

            migrationBuilder.DropColumn(
                name: "TableId1",
                table: "WorkflowTriggers");

            migrationBuilder.DropColumn(
                name: "FieldId1",
                table: "RecordValues");

            migrationBuilder.DropColumn(
                name: "TableId1",
                table: "PlatformRecords");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                schema: "platform",
                table: "AuditLogs",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_UserId1",
                schema: "platform",
                table: "AuditLogs",
                newName: "IX_AuditLogs_ApplicationUserId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "platform",
                table: "AuditLogs",
                type: "uniqueidentifier",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                schema: "platform",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_ApplicationUsers_ApplicationUserId",
                schema: "platform",
                table: "AuditLogs",
                column: "ApplicationUserId",
                principalSchema: "platform",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_ApplicationUsers_UserId",
                schema: "platform",
                table: "AuditLogs",
                column: "UserId",
                principalSchema: "platform",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordValues_PlatformRecords_RecordId",
                table: "RecordValues",
                column: "RecordId",
                principalTable: "PlatformRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SysChoices_SysFields_FieldId",
                table: "SysChoices",
                column: "FieldId",
                principalTable: "SysFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_ApplicationUsers_ApplicationUserId",
                schema: "platform",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_ApplicationUsers_UserId",
                schema: "platform",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordValues_PlatformRecords_RecordId",
                table: "RecordValues");

            migrationBuilder.DropForeignKey(
                name: "FK_SysChoices_SysFields_FieldId",
                table: "SysChoices");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId",
                schema: "platform",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                schema: "platform",
                table: "AuditLogs",
                newName: "UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_ApplicationUserId",
                schema: "platform",
                table: "AuditLogs",
                newName: "IX_AuditLogs_UserId1");

            migrationBuilder.AddColumn<Guid>(
                name: "TableId1",
                table: "WorkflowTriggers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FieldId1",
                table: "RecordValues",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TableId1",
                table: "PlatformRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "platform",
                table: "AuditLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTriggers_TableId1",
                table: "WorkflowTriggers",
                column: "TableId1");

            migrationBuilder.CreateIndex(
                name: "IX_RecordValues_FieldId1",
                table: "RecordValues",
                column: "FieldId1");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRecords_TableId1",
                table: "PlatformRecords",
                column: "TableId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_ApplicationUsers_UserId1",
                schema: "platform",
                table: "AuditLogs",
                column: "UserId1",
                principalSchema: "platform",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformRecords_SysTables_TableId1",
                table: "PlatformRecords",
                column: "TableId1",
                principalTable: "SysTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordValues_PlatformRecords_RecordId",
                table: "RecordValues",
                column: "RecordId",
                principalTable: "PlatformRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordValues_SysFields_FieldId1",
                table: "RecordValues",
                column: "FieldId1",
                principalTable: "SysFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SysChoices_SysFields_FieldId",
                table: "SysChoices",
                column: "FieldId",
                principalTable: "SysFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowTriggers_SysTables_TableId1",
                table: "WorkflowTriggers",
                column: "TableId1",
                principalTable: "SysTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
