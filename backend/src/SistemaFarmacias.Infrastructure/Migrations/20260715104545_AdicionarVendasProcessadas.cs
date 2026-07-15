using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFarmacias.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarVendasProcessadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vendas_processadas",
                columns: table => new
                {
                    VendaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendas_processadas", x => x.VendaId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vendas_processadas");
        }
    }
}
