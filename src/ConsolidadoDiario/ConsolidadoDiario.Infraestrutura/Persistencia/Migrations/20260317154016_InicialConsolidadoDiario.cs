using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsolidadoDiario.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class InicialConsolidadoDiario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lancamentos_processados",
                columns: table => new
                {
                    lancamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_lancamento = table.Column<DateOnly>(type: "date", nullable: false),
                    correlacao_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ocorrido_em_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processado_em_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lancamentos_processados", x => x.lancamento_id);
                });

            migrationBuilder.CreateTable(
                name: "saldos_diarios",
                columns: table => new
                {
                    data = table.Column<DateOnly>(type: "date", nullable: false),
                    total_creditos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_debitos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    atualizado_em_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saldos_diarios", x => x.data);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lancamentos_processados_evento_id",
                table: "lancamentos_processados",
                column: "evento_id");

            migrationBuilder.CreateIndex(
                name: "ix_lancamentos_processados_processado_em_utc",
                table: "lancamentos_processados",
                column: "processado_em_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lancamentos_processados");

            migrationBuilder.DropTable(
                name: "saldos_diarios");
        }
    }
}
