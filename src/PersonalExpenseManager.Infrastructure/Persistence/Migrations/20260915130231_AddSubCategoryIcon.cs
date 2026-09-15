using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalExpenseManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubCategoryIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "IncomeSubCategories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "ExpenseSubCategories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "IncomeSubCategories");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "ExpenseSubCategories");
        }
    }
}
