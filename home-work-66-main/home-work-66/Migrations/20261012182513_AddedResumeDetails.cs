using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace home_work_66.Migrations
{
    /// <inheritdoc />
    public partial class AddedResumeDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Resumes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Resumes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Facebook",
                table: "Resumes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LinkedIn",
                table: "Resumes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Resumes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "Resumes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telegram",
                table: "Resumes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "Facebook",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "LinkedIn",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "Telegram",
                table: "Resumes");
        }
    }
}
