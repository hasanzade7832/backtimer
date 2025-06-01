using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backtimetracker.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDownloadDateس : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Time",
                table: "Downloads",
                newName: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Downloads",
                newName: "Time");
        }
    }
}
