using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fcs.Donations.Infrastructure.SqlServer.Migrations;

[ExcludeFromCodeCoverage]
[Migration("20260718000000_AddOutboxTraceContext")]
public partial class AddOutboxTraceContext : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TraceParent",
            table: "OutboxMessages",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TraceState",
            table: "OutboxMessages",
            type: "nvarchar(512)",
            maxLength: 512,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TraceParent",
            table: "OutboxMessages");

        migrationBuilder.DropColumn(
            name: "TraceState",
            table: "OutboxMessages");
    }
}
