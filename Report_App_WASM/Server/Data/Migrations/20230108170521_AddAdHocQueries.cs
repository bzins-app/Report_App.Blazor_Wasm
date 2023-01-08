using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportAppWASM.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAdHocQueries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Parameters",
                table: "QueryStore",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSnippet",
                table: "DbTableDescriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IdDescriptions",
                table: "ActivityDbConnection",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "UseDescriptionsFromAnotherActivity",
                table: "ActivityDbConnection",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserSavedConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SaveName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeConfiguration = table.Column<int>(type: "int", nullable: false),
                    IdIntConfiguration = table.Column<int>(type: "int", nullable: false),
                    IdStringConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SavedValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSavedConfiguration", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSavedConfiguration_UserId_TypeConfiguration_IdIntConfiguration",
                table: "UserSavedConfiguration",
                columns: new[] { "UserId", "TypeConfiguration", "IdIntConfiguration" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSavedConfiguration");

            migrationBuilder.DropColumn(
                name: "Parameters",
                table: "QueryStore");

            migrationBuilder.DropColumn(
                name: "IsSnippet",
                table: "DbTableDescriptions");

            migrationBuilder.DropColumn(
                name: "IdDescriptions",
                table: "ActivityDbConnection");

            migrationBuilder.DropColumn(
                name: "UseDescriptionsFromAnotherActivity",
                table: "ActivityDbConnection");
        }
    }
}
