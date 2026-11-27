using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthLab.Provider.Migrations
{
    /// <inheritdoc />
    public partial class AddOAuthRedirectUris : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OAuthRedirectUris",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OAuthClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    RedirectUri = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthRedirectUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OAuthRedirectUris_OAuthClients_OAuthClientId",
                        column: x => x.OAuthClientId,
                        principalTable: "OAuthClients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.Sql("""
    INSERT INTO "OAuthRedirectUris" ("Id", "OAuthClientId", "RedirectUri")
    SELECT gen_random_uuid(), "Id", "RedirectUri"
    FROM "OAuthClients"
    WHERE "RedirectUri" IS NOT NULL
      AND "RedirectUri" <> '';
    """);
            migrationBuilder.CreateIndex(
                name: "IX_OAuthRedirectUris_OAuthClientId_RedirectUri",
                table: "OAuthRedirectUris",
                columns: new[] { "OAuthClientId", "RedirectUri" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OAuthRedirectUris");
        }
    }
}
