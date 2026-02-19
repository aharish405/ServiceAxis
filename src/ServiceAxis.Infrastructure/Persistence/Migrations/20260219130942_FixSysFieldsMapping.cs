using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceAxis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixSysFieldsMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SysFields_SysTables_TableId1",
                table: "SysFields");

            migrationBuilder.DropIndex(
                name: "IX_SysFields_TableId1",
                table: "SysFields");

            migrationBuilder.DropColumn(
                name: "TableId1",
                table: "SysFields");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TableId1",
                table: "SysFields",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SysFields_TableId1",
                table: "SysFields",
                column: "TableId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SysFields_SysTables_TableId1",
                table: "SysFields",
                column: "TableId1",
                principalTable: "SysTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
