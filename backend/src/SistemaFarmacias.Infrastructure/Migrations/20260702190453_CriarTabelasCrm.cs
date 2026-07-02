using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFarmacias.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelasCrm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "farmacias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_farmacias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contatos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FarmaciaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Telefone = table.Column<string>(type: "text", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    UltimaInteracaoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaCompraEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalGasto = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contatos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contatos_farmacias_FarmaciaId",
                        column: x => x.FarmaciaId,
                        principalTable: "farmacias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "whatsapp_config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FarmaciaId = table.Column<Guid>(type: "uuid", nullable: false),
                    WhatsappNumberId = table.Column<string>(type: "text", nullable: false),
                    NomeExibicao = table.Column<string>(type: "text", nullable: false),
                    HorarioFuncionamento = table.Column<string>(type: "text", nullable: true),
                    Endereco = table.Column<string>(type: "text", nullable: true),
                    MensagemSaudacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_whatsapp_config", x => x.Id);
                    table.ForeignKey(
                        name: "FK_whatsapp_config_farmacias_FarmaciaId",
                        column: x => x.FarmaciaId,
                        principalTable: "farmacias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "interacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FarmaciaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContatoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Canal = table.Column<int>(type: "integer", nullable: false),
                    MensagemRecebida = table.Column<string>(type: "text", nullable: true),
                    MensagemEnviada = table.Column<string>(type: "text", nullable: true),
                    IntencaoDetectada = table.Column<string>(type: "text", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_interacoes_contatos_ContatoId",
                        column: x => x.ContatoId,
                        principalTable: "contatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_interacoes_farmacias_FarmaciaId",
                        column: x => x.FarmaciaId,
                        principalTable: "farmacias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reativacoes_enviadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FarmaciaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContatoId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnviadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reativacoes_enviadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reativacoes_enviadas_contatos_ContatoId",
                        column: x => x.ContatoId,
                        principalTable: "contatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reativacoes_enviadas_farmacias_FarmaciaId",
                        column: x => x.FarmaciaId,
                        principalTable: "farmacias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contatos_FarmaciaId_Telefone",
                table: "contatos",
                columns: new[] { "FarmaciaId", "Telefone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_interacoes_ContatoId",
                table: "interacoes",
                column: "ContatoId");

            migrationBuilder.CreateIndex(
                name: "IX_interacoes_FarmaciaId",
                table: "interacoes",
                column: "FarmaciaId");

            migrationBuilder.CreateIndex(
                name: "IX_reativacoes_enviadas_ContatoId",
                table: "reativacoes_enviadas",
                column: "ContatoId");

            migrationBuilder.CreateIndex(
                name: "IX_reativacoes_enviadas_FarmaciaId",
                table: "reativacoes_enviadas",
                column: "FarmaciaId");

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_config_FarmaciaId",
                table: "whatsapp_config",
                column: "FarmaciaId");

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_config_WhatsappNumberId",
                table: "whatsapp_config",
                column: "WhatsappNumberId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "interacoes");

            migrationBuilder.DropTable(
                name: "reativacoes_enviadas");

            migrationBuilder.DropTable(
                name: "whatsapp_config");

            migrationBuilder.DropTable(
                name: "contatos");

            migrationBuilder.DropTable(
                name: "farmacias");
        }
    }
}
