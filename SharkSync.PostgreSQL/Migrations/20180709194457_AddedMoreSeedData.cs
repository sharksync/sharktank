using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SharkSync.PostgreSQL.Migrations
{
    public partial class AddedMoreSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE FUNCTION public.delete_replaced_changes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF 
AS $BODY$

BEGIN
	DELETE FROM public.change WHERE Id IN (SELECT Id FROM public.change WHERE ApplicationId = NEW.ApplicationId AND Entity = NEW.Entity AND RecordId = NEW.RecordId AND Property = NEW.Property ORDER BY ClientModified DESC LIMIT 9999999 OFFSET 1); 
	DELETE FROM public.change WHERE ApplicationId = NEW.ApplicationId AND Entity = NEW.Entity AND RecordId = NEW.RecordId AND strpos(NEW.Property, '__delete__') > 0 AND strpos(Property, '__delete__') = 0;
	RETURN NULL;
END;

$BODY$;

CREATE TRIGGER trigger_delete_replaced_changes AFTER INSERT ON public.change
    FOR EACH ROW EXECUTE PROCEDURE delete_replaced_changes();
");

            migrationBuilder.InsertData(
                table: "application",
                columns: new[] { "id", "accesskey", "accountid", "name" },
                values: new object[] { new Guid("19d8856c-a439-46ae-9932-c81fd0fe5556"), new Guid("0f458ce8-1a0e-450c-a2c4-2b50b3c4f41d"), new Guid("250c6f28-4611-4c28-902c-8464fabc510b"), "Integration Test App 4" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION public.delete_replaced_changes();");

            migrationBuilder.DeleteData(
                table: "application",
                keyColumn: "id",
                keyValue: new Guid("19d8856c-a439-46ae-9932-c81fd0fe5556"));
        }
    }
}
