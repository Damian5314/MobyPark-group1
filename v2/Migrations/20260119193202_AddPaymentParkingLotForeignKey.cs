using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace v2.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentParkingLotForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Payments_ParkingLotId",
                table: "Payments",
                column: "ParkingLotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_ParkingLots_ParkingLotId",
                table: "Payments",
                column: "ParkingLotId",
                principalTable: "ParkingLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_ParkingLots_ParkingLotId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ParkingLotId",
                table: "Payments");
        }
    }
}
