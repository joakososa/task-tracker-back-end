using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTracker.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// ADR 0009: the owner becomes a ProjectMembers row with Role = 'Owner' and Projects.OwnerId goes away.
    /// Hand-edited from the scaffold so existing owners are moved, not lost, in both directions.
    /// </summary>
    public partial class ProjectMemberRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nullable first: existing rows get their role below, then the column becomes NOT NULL.
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "ProjectMembers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql("UPDATE [ProjectMembers] SET [Role] = N'Member';");

            migrationBuilder.Sql(
                """
                INSERT INTO [ProjectMembers] ([ProjectId], [UserId], [Role], [JoinedAt])
                SELECT [Id], [OwnerId], N'Owner', [CreatedAt]
                FROM [Projects];
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "ProjectMembers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_OneOwnerPerProject",
                table: "ProjectMembers",
                column: "ProjectId",
                unique: true,
                filter: "[Role] = 'Owner'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectMembers_Role",
                table: "ProjectMembers",
                sql: "[Role] IN ('Owner', 'Member')");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_OwnerId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_OwnerId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Projects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE p SET p.[OwnerId] = m.[UserId]
                FROM [Projects] p
                JOIN [ProjectMembers] m ON m.[ProjectId] = p.[Id] AND m.[Role] = N'Owner';
                """);

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Projects",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OwnerId",
                table: "Projects",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_OwnerId",
                table: "Projects",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("DELETE FROM [ProjectMembers] WHERE [Role] = N'Owner';");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMembers_OneOwnerPerProject",
                table: "ProjectMembers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectMembers_Role",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ProjectMembers");
        }
    }
}
