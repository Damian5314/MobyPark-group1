using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace v2.Migrations
{
    /// <inheritdoc />
    public partial class AddParkingLotForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_ParkingLotId",
                table: "ParkingSessions",
                column: "ParkingLotId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_ParkingLots_ParkingLotId",
                table: "ParkingSessions",
                column: "ParkingLotId",
                principalTable: "ParkingLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_ParkingLots_ParkingLotId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_ParkingLotId",
                table: "ParkingSessions");
        }
    }
}
