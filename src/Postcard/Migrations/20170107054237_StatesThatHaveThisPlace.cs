using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Postcard.Migrations
{
    public partial class StatesThatHaveThisPlace : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumberOfStatesThatHaveThisPlace",
                table: "PlaceNodes",
                newName: "numberOfStatesThatHaveThisPlace");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "numberOfStatesThatHaveThisPlace",
                table: "PlaceNodes",
                newName: "NumberOfStatesThatHaveThisPlace");
        }
    }
}
