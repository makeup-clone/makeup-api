using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConnectedProductCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CategoryId",
                table: "tblProducts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_tblProducts_CategoryId",
                table: "tblProducts",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_tblProducts_tblCategories_CategoryId",
                table: "tblProducts",
                column: "CategoryId",
                principalTable: "tblCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblProducts_tblCategories_CategoryId",
                table: "tblProducts");

            migrationBuilder.DropIndex(
                name: "IX_tblProducts_CategoryId",
                table: "tblProducts");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "tblProducts");
        }
    }
}
