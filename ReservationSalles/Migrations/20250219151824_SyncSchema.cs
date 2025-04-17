using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationSalles.Migrations
{
    /// <inheritdoc />
    public partial class SyncSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BookedBy",
                table: "Reservations",
                newName: "MeetingSubject");

            migrationBuilder.AddColumn<string>(
                name: "AttendeeFirstName",
                table: "Reservations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AttendeeLastName",
                table: "Reservations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendeeFirstName",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "AttendeeLastName",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "MeetingSubject",
                table: "Reservations",
                newName: "BookedBy");
        }
    }
}
