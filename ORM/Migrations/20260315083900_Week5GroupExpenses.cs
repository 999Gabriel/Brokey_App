using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ORM.Migrations
{
    /// <inheritdoc />
    public partial class Week5GroupExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Expenses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_GroupId_ExpenseDate",
                table: "Expenses",
                columns: new[] { "GroupId", "ExpenseDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Groups_GroupId",
                table: "Expenses",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Groups_GroupId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_GroupId_ExpenseDate",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Expenses");
        }
    }
}
