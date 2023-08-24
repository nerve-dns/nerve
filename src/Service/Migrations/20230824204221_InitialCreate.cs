using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nerve.Service.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Counters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Queries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<long>(type: "INTEGER", nullable: false),
                    Client = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<ushort>(type: "INTEGER", nullable: false),
                    Domain = table.Column<string>(type: "TEXT", nullable: false),
                    ResponseCode = table.Column<byte>(type: "INTEGER", nullable: false),
                    Duration = table.Column<float>(type: "REAL", nullable: false),
                    Status = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queries", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Counters",
                columns: new[] { "Id", "Value" },
                values: new object[,]
                {
                    { 1, 0L },
                    { 2, 0L },
                    { 3, 0L }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Queries_Domain",
                table: "Queries",
                column: "Domain");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Counters");

            migrationBuilder.DropTable(
                name: "Queries");
        }
    }
}
