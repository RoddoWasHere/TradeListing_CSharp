using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace TradingAPI.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Instrument",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "varchar(767)", nullable: false),
                    SymbolChar = table.Column<string>(type: "text", nullable: true),
                    SymbolName = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrument", x => x.Symbol);
                });

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Age = table.Column<int>(type: "int", nullable: false),
                    IsPlayer = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentPair",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "varchar(767)", nullable: false),
                    BaseInstrumentSymbol = table.Column<string>(type: "varchar(767)", nullable: true),
                    QuoteInstrumentSymbol = table.Column<string>(type: "varchar(767)", nullable: true),
                    IceBergAllowed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSpotTradingAllowed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsMarginTradingAllowed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OcoAllowed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QuoteOrderQuantityMarketAllowed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BaseCommissionPrecision = table.Column<int>(type: "int", nullable: false),
                    QuoteCommissionPrecision = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentPair", x => x.Symbol);
                    table.ForeignKey(
                        name: "FK_InstrumentPair_Instrument_BaseInstrumentSymbol",
                        column: x => x.BaseInstrumentSymbol,
                        principalTable: "Instrument",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstrumentPair_Instrument_QuoteInstrumentSymbol",
                        column: x => x.QuoteInstrumentSymbol,
                        principalTable: "Instrument",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentPair_BaseInstrumentSymbol",
                table: "InstrumentPair",
                column: "BaseInstrumentSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentPair_QuoteInstrumentSymbol",
                table: "InstrumentPair",
                column: "QuoteInstrumentSymbol");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstrumentPair");

            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropTable(
                name: "Instrument");
        }
    }
}
