using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "transactions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_transactions_Category",
                table: "transactions",
                column: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_transactions_Category",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "transactions");
        }
    }
}
