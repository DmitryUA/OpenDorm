using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenDorm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dormitories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "идентификатор общежития"),
                    address_city = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "город"),
                    address_street = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "улица"),
                    address_house = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "номер дома"),
                    floor_count = table.Column<int>(type: "integer", nullable: false, comment: "общее количество этажей в здании")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dormitories", x => x.id);
                    table.CheckConstraint("CK_Address_City_MinLength", "LENGTH(address_city) >= 3");
                    table.CheckConstraint("CK_Address_Street_MinLength", "LENGTH(address_street) >= 3");
                },
                comment: "общежития");

            migrationBuilder.CreateTable(
                name: "occupants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "идентификатор жильца"),
                    last_name_encrypted = table.Column<byte[]>(type: "bytea", nullable: false, comment: "зашифрованная фамилия"),
                    first_name_encrypted = table.Column<byte[]>(type: "bytea", nullable: false, comment: "зашифрованное имя"),
                    patronymic_encrypted = table.Column<byte[]>(type: "bytea", nullable: true, comment: "зашифрованное отчество"),
                    gender = table.Column<char>(type: "character(1)", nullable: false, comment: "гендер (m - мужчина, f - женщина)"),
                    birth_date_encrypted = table.Column<byte[]>(type: "bytea", nullable: false, comment: "зашифрованная дата рождения"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "активен ли проживающий, true - да, false - нет")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_occupants", x => x.id);
                },
                comment: "жильцы");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "идентификатор роли"),
                    name = table.Column<int>(type: "integer", nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                },
                comment: "роли");

            migrationBuilder.CreateTable(
                name: "system_flags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "идентификатор флага"),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    value = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_flags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "идентификатор пользователя"),
                    last_name_encrypted = table.Column<byte[]>(type: "bytea", nullable: false, comment: "зашифрованная фамилия"),
                    first_name_encrypted = table.Column<byte[]>(type: "bytea", nullable: false, comment: "зашифрованное имя"),
                    patronymic_encrypted = table.Column<byte[]>(type: "bytea", nullable: true, comment: "зашифрованное отчество"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    login = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("CK_Login_MinLength", "LENGTH(login) >= 4");
                },
                comment: "пользователи");

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "идентификатор комнаты"),
                    gender = table.Column<char>(type: "character(1)", nullable: false, comment: "гендер, m - мужская, f - женская"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "активна ли комната"),
                    dormitory_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "идентификатор общежития"),
                    capacity = table.Column<int>(type: "integer", nullable: false, comment: "вместимость"),
                    floor_number = table.Column<int>(type: "integer", nullable: false, comment: "номер этажа"),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "номер комнаты")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.id);
                    table.CheckConstraint("CK_Name_MinLength", "LENGTH(name) >= 1");
                    table.ForeignKey(
                        name: "FK_rooms_dormitories_dormitory_id",
                        column: x => x.dormitory_id,
                        principalTable: "dormitories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "комнаты в общежитиях");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "идентификатор токена"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "токены обновления");

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "идентификатор роли пользователя"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_roles_roles",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "роли пользователя");

            migrationBuilder.CreateTable(
                name: "accommodations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "идентификатор заселения"),
                    room_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "идентификатор комнаты"),
                    check_in_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "дата и время заселения"),
                    check_out_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "дата и время выселения"),
                    occupant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "идентификатор жильца")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accommodations", x => x.id);
                    table.ForeignKey(
                        name: "FK_accommodations_occupants_occupant_id",
                        column: x => x.occupant_id,
                        principalTable: "occupants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_accommodations_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "заселения");

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "name", "normalized_name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), 1, "ADMINISTRATOR" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), 2, "MANAGER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_accommodations_room_id",
                table: "accommodations",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "UX_Accommodations_OccupantId_Active",
                table: "accommodations",
                column: "occupant_id",
                filter: "check_out_date IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_token",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rooms_dormitory_id",
                table: "rooms",
                column: "dormitory_id");

            migrationBuilder.CreateIndex(
                name: "IX_system_flags_name",
                table: "system_flags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_user_id_role_id",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accommodations");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "system_flags");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "occupants");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "dormitories");
        }
    }
}
