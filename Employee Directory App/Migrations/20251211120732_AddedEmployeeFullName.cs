using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employee_Directory_App.Migrations
{
    /// <inheritdoc />
    public partial class AddedEmployeeFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeeFullName",
                table: "EmployeesProjects",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeFullName",
                table: "EmployeesProjects");
        }
    }
}
