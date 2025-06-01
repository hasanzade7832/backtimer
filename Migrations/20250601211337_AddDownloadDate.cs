using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backtimetracker.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloadDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Date",
                table: "Downloads",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Downloads");
        }
    }
}
