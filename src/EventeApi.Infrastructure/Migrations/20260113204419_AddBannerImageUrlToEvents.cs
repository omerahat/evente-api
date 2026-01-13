using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventeApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerImageUrlToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "banner_image_url",
                table: "Events",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "banner_image_url",
                table: "Events");
        }
    }
}
