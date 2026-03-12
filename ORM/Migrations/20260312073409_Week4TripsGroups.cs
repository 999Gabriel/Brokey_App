using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ORM.Migrations
{
    /// <inheritdoc />
    public partial class Week4TripsGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Users_AdminUserId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Groups_GroupId",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Users_CreatedByUserId",
                table: "Trips");

            migrationBuilder.Sql("DROP INDEX `IX_Trips_GroupId` ON `Trips`;");
            migrationBuilder.Sql("DROP INDEX `IX_Groups_AdminUserId` ON `Groups`;");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Groups");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "Trips",
                newName: "BaseCurrency");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Trips",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Trips_CreatedByUserId",
                table: "Trips",
                newName: "IX_Trips_CreatedById");

            migrationBuilder.RenameColumn(
                name: "AdminUserId",
                table: "Groups",
                newName: "TripId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Trips",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "TripMembers",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "GroupMembers",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CreatedById",
                table: "Groups",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_TripId_Name",
                table: "Groups",
                columns: new[] { "TripId", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Trips_TripId",
                table: "Groups",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Users_CreatedById",
                table: "Groups",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Users_CreatedById",
                table: "Trips",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Trips_TripId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Users_CreatedById",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Users_CreatedById",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Groups_CreatedById",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_TripId_Name",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "TripMembers");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "GroupMembers");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "Trips",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "BaseCurrency",
                table: "Trips",
                newName: "Currency");

            migrationBuilder.RenameIndex(
                name: "IX_Trips_CreatedById",
                table: "Trips",
                newName: "IX_Trips_CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "TripId",
                table: "Groups",
                newName: "AdminUserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Trips",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Trips",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Trips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Groups",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_GroupId",
                table: "Trips",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_AdminUserId",
                table: "Groups",
                column: "AdminUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Users_AdminUserId",
                table: "Groups",
                column: "AdminUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Groups_GroupId",
                table: "Trips",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Users_CreatedByUserId",
                table: "Trips",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
