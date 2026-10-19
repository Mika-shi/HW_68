using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace home_work_66.Migrations
{
    /// <inheritdoc />
    public partial class AddedCategoryRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Resumes");

            migrationBuilder.Sql("DELETE FROM \"Resumes\";");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Resumes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Resumes_CategoryId",
                table: "Resumes",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resumes_Categories_CategoryId",
                table: "Resumes",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resumes_Categories_CategoryId",
                table: "Resumes");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Resumes_CategoryId",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Resumes");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Resumes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
