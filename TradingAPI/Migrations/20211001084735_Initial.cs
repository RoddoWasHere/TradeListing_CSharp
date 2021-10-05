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
                    Name = table.Column<string>(type: "text", nullable: true),
                    SymbolChar = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrument", x => x.Symbol);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentPair",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "varchar(767)", nullable: false),
                    BaseInstrumentId = table.Column<string>(type: "varchar(767)", nullable: true),
                    QuoteInstrumentId = table.Column<string>(type: "varchar(767)", nullable: true),
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
                        name: "FK_InstrumentPair_Instrument_BaseInstrumentId",
                        column: x => x.BaseInstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstrumentPair_Instrument_QuoteInstrumentId",
                        column: x => x.QuoteInstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    InstrumentPairId = table.Column<string>(type: "varchar(767)", nullable: true),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    UtcOpenTime = table.Column<long>(type: "bigint", nullable: false),
                    UtcCloseTime = table.Column<long>(type: "bigint", nullable: false),
                    High = table.Column<decimal>(type: "decimal(10,10)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(10,10)", nullable: false),
                    Open = table.Column<decimal>(type: "decimal(10,10)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(10,10)", nullable: false),
                    TradeCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceHistory_InstrumentPair_InstrumentPairId",
                        column: x => x.InstrumentPairId,
                        principalTable: "InstrumentPair",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentPair_BaseInstrumentId",
                table: "InstrumentPair",
                column: "BaseInstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentPair_QuoteInstrumentId",
                table: "InstrumentPair",
                column: "QuoteInstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_InstrumentPairId",
                table: "PriceHistory",
                column: "InstrumentPairId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceHistory");

            migrationBuilder.DropTable(
                name: "InstrumentPair");

            migrationBuilder.DropTable(
                name: "Instrument");
        }
    }
}
