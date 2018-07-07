using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SharkSync.PostgreSQL.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(nullable: true),
                    emailaddress = table.Column<string>(nullable: true),
                    avatarurl = table.Column<string>(nullable: true),
                    githubid = table.Column<string>(nullable: true),
                    googleid = table.Column<string>(nullable: true),
                    microsoftid = table.Column<string>(nullable: true),
                    balance = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "application",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    accesskey = table.Column<Guid>(nullable: false),
                    accountid = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "change",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    accountid = table.Column<Guid>(nullable: false),
                    applicationid = table.Column<Guid>(nullable: false),
                    groupid = table.Column<string>(nullable: true),
                    entity = table.Column<string>(nullable: true),
                    recordid = table.Column<Guid>(nullable: false),
                    property = table.Column<string>(nullable: true),
                    clientmodified = table.Column<long>(nullable: false),
                    recordvalue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_change", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "account",
                columns: new[] { "id", "avatarurl", "balance", "emailaddress", "githubid", "googleid", "microsoftid", "name" },
                values: new object[] { new Guid("250c6f28-4611-4c28-902c-8464fabc510b"), null, 0, null, null, null, null, "Integration Tests Account" });

            migrationBuilder.InsertData(
                table: "application",
                columns: new[] { "id", "accesskey", "accountid", "name" },
                values: new object[,]
                {
                    { new Guid("afd8db1e-73b8-4d5f-9cb1-6b49d205555a"), new Guid("3d65a27c-9d1d-48a3-a888-89cc0f7851d0"), new Guid("250c6f28-4611-4c28-902c-8464fabc510b"), "Integration Test App" },
                    { new Guid("59eadf1b-c4bf-4ded-8a2b-b80305b960fe"), new Guid("e7b40cf0-2781-4dc7-9545-91fd812fc506"), new Guid("250c6f28-4611-4c28-902c-8464fabc510b"), "Integration Test App 2" },
                    { new Guid("b858ceb1-00d0-4427-b45d-e9890b77da36"), new Guid("03172495-6158-44ae-b5b4-6ea5163f02d8"), new Guid("250c6f28-4611-4c28-902c-8464fabc510b"), "Integration Test App 3" }
                });

            //migrationBuilder.Sql("CREATE USER webuser WITH PASSWORD 'temp-password-will-be-rotated';");

            migrationBuilder.Sql("GRANT CONNECT ON DATABASE \"SharkSync\" TO webuser;");
            migrationBuilder.Sql("GRANT USAGE ON SCHEMA public TO webuser;");
            migrationBuilder.Sql("GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO webuser;");
            migrationBuilder.Sql("GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO webuser;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account");

            migrationBuilder.DropTable(
                name: "application");

            migrationBuilder.DropTable(
                name: "change");
        }
    }
}
