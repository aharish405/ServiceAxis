using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceAxis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnterprisePlatfromFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecordValues_RecordId",
                table: "RecordValues");

            migrationBuilder.DropIndex(
                name: "IX_PlatformRecords_TableId",
                table: "PlatformRecords");

            migrationBuilder.AlterColumn<string>(
                name: "RecordNumber",
                table: "PlatformRecords",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "RecordAudit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordAudit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecordAudit_PlatformRecords_RecordId",
                        column: x => x.RecordId,
                        principalTable: "PlatformRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecordValues_RecordId_FieldId",
                table: "RecordValues",
                columns: new[] { "RecordId", "FieldId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRecords_RecordNumber",
                table: "PlatformRecords",
                column: "RecordNumber",
                unique: true,
                filter: "[RecordNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRecords_TableId_CreatedAt",
                table: "PlatformRecords",
                columns: new[] { "TableId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RecordAudit_RecordId",
                table: "RecordAudit",
                column: "RecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecordAudit");

            migrationBuilder.DropIndex(
                name: "IX_RecordValues_RecordId_FieldId",
                table: "RecordValues");

            migrationBuilder.DropIndex(
                name: "IX_PlatformRecords_RecordNumber",
                table: "PlatformRecords");

            migrationBuilder.DropIndex(
                name: "IX_PlatformRecords_TableId_CreatedAt",
                table: "PlatformRecords");

            migrationBuilder.AlterColumn<string>(
                name: "RecordNumber",
                table: "PlatformRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecordValues_RecordId",
                table: "RecordValues",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRecords_TableId",
                table: "PlatformRecords",
                column: "TableId");
        }
    }
}
