using System;
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
                name: "Lists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Ip = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    LastRefreshed = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Queries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Resolvers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Endpoint = table.Column<string>(type: "TEXT", nullable: false),
                    Protocol = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resolvers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Domains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<byte>(type: "INTEGER", nullable: false),
                    Source = table.Column<byte>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    ListId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Domains_Lists_ListId",
                        column: x => x.ListId,
                        principalTable: "Lists",
                        principalColumn: "Id");
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

            migrationBuilder.InsertData(
                table: "Resolvers",
                columns: new[] { "Id", "Endpoint", "Protocol" },
                values: new object[,]
                {
                    { 1, "https://unfiltered.joindns4.eu/dns-query", 1 },
                    { 2, "https://dns.quad9.net/dns-query", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Domains_Action",
                table: "Domains",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_Domains_ListId",
                table: "Domains",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_Domains_Source",
                table: "Domains",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Domains_Value",
                table: "Domains",
                column: "Value",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lists_Ip",
                table: "Lists",
                column: "Ip");

            migrationBuilder.CreateIndex(
                name: "IX_Lists_Location",
                table: "Lists",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_Queries_Client",
                table: "Queries",
                column: "Client");

            migrationBuilder.CreateIndex(
                name: "IX_Queries_Domain",
                table: "Queries",
                column: "Domain");

            migrationBuilder.CreateIndex(
                name: "IX_Queries_Status",
                table: "Queries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Queries_Timestamp",
                table: "Queries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Queries_Type",
                table: "Queries",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Resolvers_Endpoint",
                table: "Resolvers",
                column: "Endpoint");

            migrationBuilder.CreateIndex(
                name: "IX_Resolvers_Protocol",
                table: "Resolvers",
                column: "Protocol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Counters");

            migrationBuilder.DropTable(
                name: "Domains");

            migrationBuilder.DropTable(
                name: "Queries");

            migrationBuilder.DropTable(
                name: "Resolvers");

            migrationBuilder.DropTable(
                name: "Lists");
        }
    }
}
