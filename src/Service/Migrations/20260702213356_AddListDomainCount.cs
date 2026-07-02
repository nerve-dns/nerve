using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nerve.Service.Migrations
{
    /// <inheritdoc />
    public partial class AddListDomainCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DomainCount",
                table: "Lists",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DomainCount",
                table: "Lists");
        }
    }
}
