using Microsoft.EntityFrameworkCore.Migrations;

namespace LightenceServer.Migrations
{
    public partial class Update1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisionID",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "ServerLogs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "ServerLogs",
                newName: "TimeStamp");

            migrationBuilder.RenameColumn(
                name: "Uses",
                table: "ProductKeys",
                newName: "UseCount");

            migrationBuilder.RenameColumn(
                name: "CreateDate",
                table: "ProductKeys",
                newName: "CreationDate");

            migrationBuilder.RenameColumn(
                name: "FaceData",
                table: "BiometricLogins",
                newName: "Data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ServerLogs",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "ServerLogs",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "UseCount",
                table: "ProductKeys",
                newName: "Uses");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "ProductKeys",
                newName: "CreateDate");

            migrationBuilder.RenameColumn(
                name: "Data",
                table: "BiometricLogins",
                newName: "FaceData");

            migrationBuilder.AddColumn<string>(
                name: "VisionID",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
