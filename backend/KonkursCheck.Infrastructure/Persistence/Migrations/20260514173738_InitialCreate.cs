using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonkursCheck.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    cvr_number = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    founded_date = table.Column<DateOnly>(type: "date", nullable: true),
                    bankruptcy_date = table.Column<DateOnly>(type: "date", nullable: true),
                    dissolution_date = table.Column<DateOnly>(type: "date", nullable: true),
                    industry_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.cvr_number);
                });

            migrationBuilder.CreateTable(
                name: "persons",
                columns: table => new
                {
                    person_cvr_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persons", x => x.person_cvr_id);
                });

            migrationBuilder.CreateTable(
                name: "bankruptcy_summaries",
                columns: table => new
                {
                    person_cvr_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total_bankruptcies = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    most_recent_date = table.Column<DateOnly>(type: "date", nullable: true),
                    company_names = table.Column<string[]>(type: "text[]", nullable: false),
                    last_calculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bankruptcy_summaries", x => x.person_cvr_id);
                    table.ForeignKey(
                        name: "FK_bankruptcy_summaries_persons_person_cvr_id",
                        column: x => x.person_cvr_id,
                        principalTable: "persons",
                        principalColumn: "person_cvr_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person_company_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    person_cvr_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cvr_number = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person_company_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_person_company_roles_companies_cvr_number",
                        column: x => x.cvr_number,
                        principalTable: "companies",
                        principalColumn: "cvr_number",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_person_company_roles_persons_person_cvr_id",
                        column: x => x.person_cvr_id,
                        principalTable: "persons",
                        principalColumn: "person_cvr_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_summary_bankruptcies",
                table: "bankruptcy_summaries",
                column: "total_bankruptcies",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_companies_name",
                table: "companies",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "idx_roles_company",
                table: "person_company_roles",
                column: "cvr_number");

            migrationBuilder.CreateIndex(
                name: "idx_roles_person",
                table: "person_company_roles",
                column: "person_cvr_id");

            migrationBuilder.CreateIndex(
                name: "idx_persons_name",
                table: "persons",
                column: "full_name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bankruptcy_summaries");

            migrationBuilder.DropTable(
                name: "person_company_roles");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "persons");
        }
    }
}
