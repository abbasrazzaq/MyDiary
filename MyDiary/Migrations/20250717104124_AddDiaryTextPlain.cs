using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyDiary.Migrations
{
    /// <inheritdoc />
    public partial class AddDiaryTextPlain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiaryTextPlain",
                table: "DiaryEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaryTextPlain",
                table: "DiaryEntries");
        }
    }
}
