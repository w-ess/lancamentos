using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lancamentos.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMensagensSaida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mensagens_saida",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    conteudo = table.Column<string>(type: "text", nullable: false),
                    correlacao_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ocorrida_em_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    publicada_em_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tentativas_publicacao = table.Column<int>(type: "integer", nullable: false),
                    ultimo_erro = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mensagens_saida", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mensagens_saida_publicada_ocorrida",
                table: "mensagens_saida",
                columns: new[] { "publicada_em_utc", "ocorrida_em_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mensagens_saida");
        }
    }
}
