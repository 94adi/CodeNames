using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeNames.Migrations
{
    /// <inheritdoc />
    public partial class AddedInvitationCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InvitationCode",
                table: "GameRooms",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvitationCode",
                table: "GameRooms");
        }
    }
}
