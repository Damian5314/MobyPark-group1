using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace v2.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReservationDeleteBehaviorToCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ParkingLots_ParkingLotId",
                table: "Reservations");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ParkingLots_ParkingLotId",
                table: "Reservations",
                column: "ParkingLotId",
                principalTable: "ParkingLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ParkingLots_ParkingLotId",
                table: "Reservations");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ParkingLots_ParkingLotId",
                table: "Reservations",
                column: "ParkingLotId",
                principalTable: "ParkingLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
