using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CsInvite.Shared.Migrations
{
    public partial class FriendColumnName2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friends_AspNetUsers_FriendUserId",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_Friends_FriendUserId",
                table: "Friends");

            migrationBuilder.DropColumn(
                name: "FriendUserId",
                table: "Friends");

            migrationBuilder.AddColumn<string>(
                name: "OtherUserId",
                table: "Friends",
                type: "varchar(36)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friends_OtherUserId",
                table: "Friends",
                column: "OtherUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friends_AspNetUsers_OtherUserId",
                table: "Friends",
                column: "OtherUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friends_AspNetUsers_OtherUserId",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_Friends_OtherUserId",
                table: "Friends");

            migrationBuilder.DropColumn(
                name: "OtherUserId",
                table: "Friends");

            migrationBuilder.AddColumn<string>(
                name: "FriendUserId",
                table: "Friends",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friends_FriendUserId",
                table: "Friends",
                column: "FriendUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friends_AspNetUsers_FriendUserId",
                table: "Friends",
                column: "FriendUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
