using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aloha.PostService.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "PostServiceDB");

            migrationBuilder.CreateTable(
                name: "Posts",
                schema: "PostServiceDB",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CategoryPath = table.Column<string>(type: "jsonb", nullable: false),
                    ProvinceCode = table.Column<int>(type: "integer", nullable: false),
                    ProvinceText = table.Column<string>(type: "text", nullable: true),
                    DistrictCode = table.Column<int>(type: "integer", nullable: false),
                    DistrictText = table.Column<string>(type: "text", nullable: true),
                    WardCode = table.Column<int>(type: "integer", nullable: false),
                    WardText = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsViolation = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PushedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Attributes = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    IsLocationValid = table.Column<bool>(type: "boolean", nullable: false),
                    IsCategoryValid = table.Column<bool>(type: "boolean", nullable: false),
                    IsUserPlanValid = table.Column<bool>(type: "boolean", nullable: false),
                    LocationValidationReceived = table.Column<bool>(type: "boolean", nullable: false),
                    CategoryValidationReceived = table.Column<bool>(type: "boolean", nullable: false),
                    UserPlanValidationReceived = table.Column<bool>(type: "boolean", nullable: false),
                    UserPlanWasConsumed = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    LocationValidationMessage = table.Column<string>(type: "text", nullable: true),
                    CategoryValidationMessage = table.Column<string>(type: "text", nullable: true),
                    UserPlanValidationMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostImages",
                schema: "PostServiceDB",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostImages_Posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "PostServiceDB",
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostImages_PostId",
                schema: "PostServiceDB",
                table: "PostImages",
                column: "PostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostImages",
                schema: "PostServiceDB");

            migrationBuilder.DropTable(
                name: "Posts",
                schema: "PostServiceDB");
        }
    }
}
