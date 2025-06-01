using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backtimetracker.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalSecondsToActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalSeconds",
                table: "Activities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalSeconds",
                table: "Activities");
        }
    }
}
