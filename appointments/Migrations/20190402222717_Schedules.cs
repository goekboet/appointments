using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace appointments.Migrations
{
    public partial class Schedules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    PrincipalId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ScheduleName = table.Column<string>(nullable: false),
                    Start = table.Column<long>(nullable: false),
                    Duration = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.UniqueConstraint("AK_Appointments_ScheduleName_Start", x => new { x.ScheduleName, x.Start });
                    table.ForeignKey(
                        name: "FK_Appointments_Schedules_ScheduleName",
                        column: x => x.ScheduleName,
                        principalTable: "Schedules",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Participant",
                columns: table => new
                {
                    SubjectId = table.Column<string>(nullable: false),
                    AppointmentId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participant", x => new { x.AppointmentId, x.SubjectId });
                    table.ForeignKey(
                        name: "FK_Participant_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Start",
                table: "Appointments",
                column: "Start");

            migrationBuilder.CreateIndex(
                name: "IX_Participant_Name",
                table: "Participant",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_PrincipalId",
                table: "Schedules",
                column: "PrincipalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Participant");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Schedules");
        }
    }
}
