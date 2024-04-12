using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeNames.Migrations
{
    /// <inheritdoc />
    public partial class AddedLiveGameSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LiveGameSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameRoomId = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveGameSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveGameSession_GameRooms_GameRoomId",
                        column: x => x.GameRoomId,
                        principalTable: "GameRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveGameSession_GameRoomId",
                table: "LiveGameSession",
                column: "GameRoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiveGameSession");
        }
    }
}
