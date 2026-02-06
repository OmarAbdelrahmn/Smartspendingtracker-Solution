using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Smartspendingtracker.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEnglish = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameArabic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromCurrency = table.Column<int>(type: "int", nullable: false),
                    ToCurrency = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    ConvertedAmountInEGP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    ExchangeRateUsed = table.Column<decimal>(type: "decimal(18,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Color", "IconClass", "Keywords", "NameArabic", "NameEnglish" },
                values: new object[,]
                {
                    { 1, "#FF6B6B", "fa-utensils", "أكل,مطعم,قهوة,فطار,غدا,عشا,طعام,food,restaurant,coffee,lunch,dinner,breakfast", "طعام", "Food" },
                    { 2, "#4ECDC4", "fa-car", "مواصلات,بنزين,تاكسي,سيارة,transport,taxi,gas,car,fuel,uber", "مواصلات", "Transportation" },
                    { 3, "#95E1D3", "fa-file-invoice-dollar", "فواتير,كهرباء,ماء,نت,انترنت,bills,electricity,water,internet,utilities", "فواتير", "Bills" },
                    { 4, "#F38181", "fa-home", "إيجار,ايجار,سكن,rent,housing", "إيجار", "Rent" },
                    { 5, "#AA96DA", "fa-shopping-bag", "تسوق,ملابس,شراء,shopping,clothes,purchase,buy", "تسوق", "Shopping" },
                    { 6, "#6C757D", "fa-folder", "أخرى,متفرقات,other,miscellaneous,various", "أخرى", "Other" }
                });

            migrationBuilder.InsertData(
                table: "ExchangeRates",
                columns: new[] { "Id", "FromCurrency", "Month", "Rate", "ToCurrency", "UpdatedAt", "Year" },
                values: new object[] { 1, 2, 2, 13.5m, 1, new DateTime(2026, 2, 6, 22, 40, 44, 366, DateTimeKind.Utc).AddTicks(3953), 2026 });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_NameArabic",
                table: "Categories",
                column: "NameArabic");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_NameEnglish",
                table: "Categories",
                column: "NameEnglish");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Year_Month_FromCurrency_ToCurrency",
                table: "ExchangeRates",
                columns: new[] { "Year", "Month", "FromCurrency", "ToCurrency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CategoryId",
                table: "Expenses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_DateTime",
                table: "Expenses",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_DateTime_CategoryId",
                table: "Expenses",
                columns: new[] { "DateTime", "CategoryId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
