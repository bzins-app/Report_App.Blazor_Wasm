using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportAppWASM.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "TaskHeader",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ActivityName",
                table: "QueryStore",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "QueryStore",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "QueryStore",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ApplicationLogAdHocQueries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueryId = table.Column<int>(type: "int", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationInSeconds = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    JobDescription = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    NbrOfRows = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Error = table.Column<bool>(type: "bit", nullable: false),
                    RunBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogAdHocQueries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogAdHocQueries_ActivityId_QueryId_JobDescription",
                table: "ApplicationLogAdHocQueries",
                columns: new[] { "ActivityId", "QueryId", "JobDescription" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationLogAdHocQueries");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "TaskHeader");

            migrationBuilder.DropColumn(
                name: "ActivityName",
                table: "QueryStore");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "QueryStore");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "QueryStore");
        }
    }
}
