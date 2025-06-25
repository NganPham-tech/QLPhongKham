using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLPhongKham.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialtyToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Specialty",
                table: "Services",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Specialty",
                table: "Services");
        }
    }
}
