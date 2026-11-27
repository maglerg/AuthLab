using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthLab.Provider.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldOAuthClientRedirectUri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedirectUri",
                table: "OAuthClients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RedirectUri",
                table: "OAuthClients",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
