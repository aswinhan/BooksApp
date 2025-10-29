using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Blog.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialBlogSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "blog_category_id",
                schema: "blog",
                table: "posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "blog_categories",
                schema: "blog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "blog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "post_tags",
                schema: "blog",
                columns: table => new
                {
                    posts_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tags_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_tags", x => new { x.posts_id, x.tags_id });
                    table.ForeignKey(
                        name: "fk_post_tags_posts_posts_id",
                        column: x => x.posts_id,
                        principalSchema: "blog",
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_tags_tags_tags_id",
                        column: x => x.tags_id,
                        principalSchema: "blog",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_posts_blog_category_id",
                schema: "blog",
                table: "posts",
                column: "blog_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_blog_categories_slug",
                schema: "blog",
                table: "blog_categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_tags_tags_id",
                schema: "blog",
                table: "post_tags",
                column: "tags_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_slug",
                schema: "blog",
                table: "tags",
                column: "slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_posts_blog_categories_blog_category_id",
                schema: "blog",
                table: "posts",
                column: "blog_category_id",
                principalSchema: "blog",
                principalTable: "blog_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_posts_blog_categories_blog_category_id",
                schema: "blog",
                table: "posts");

            migrationBuilder.DropTable(
                name: "blog_categories",
                schema: "blog");

            migrationBuilder.DropTable(
                name: "post_tags",
                schema: "blog");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "blog");

            migrationBuilder.DropIndex(
                name: "ix_posts_blog_category_id",
                schema: "blog",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "blog_category_id",
                schema: "blog",
                table: "posts");
        }
    }
}
