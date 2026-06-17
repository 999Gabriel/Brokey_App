using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ORM.Migrations
{
    // GENERIERTE EF-Core-Migration (NICHT von Hand geschrieben).
    // Migration "Week5GroupExpenses": verknüpft Ausgaben mit Gruppen — fügt der Tabelle Expenses die
    // optionale Spalte GroupId (Fremdschlüssel auf Groups, SetNull) samt Index hinzu.
    /// <inheritdoc />
    public partial class Week5GroupExpenses : Migration
    {
        // Up(): fügt die GroupId-Spalte, den Index (GroupId, ExpenseDate) und den Fremdschlüssel zu Groups hinzu.
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

        // Down(): macht Up() rückgängig — entfernt Fremdschlüssel, Index und die GroupId-Spalte wieder.
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
