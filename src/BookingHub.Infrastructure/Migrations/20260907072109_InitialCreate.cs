using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BookingHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: false),
                    WorkingHours = table.Column<string>(type: "jsonb", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CancellationDeadline = table.Column<TimeSpan>(type: "interval", nullable: false),
                    PendingConfirmationWindow = table.Column<TimeSpan>(type: "interval", nullable: false),
                    AutoCompleteWindow = table.Column<TimeSpan>(type: "interval", nullable: false),
                    WaitlistOfferWindow = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CanAdministratorsViewFinancials = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhotoUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    IsBookable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationMembers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    BufferBefore = table.Column<TimeSpan>(type: "interval", nullable: false),
                    BufferAfter = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    BasePriceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BasePriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeLocationAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLocationAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeLocationAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeLocationAssignments_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    RecurringSeriesId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "text", nullable: true),
                    ConfirmedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClientEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    ClientPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConfirmationToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PriceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TimeSlot_EndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeSlot_StartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocationServiceOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    OverridePriceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OverridePriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationServiceOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationServiceOverrides_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationServiceOverrides_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WaitlistEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    OfferedEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    OfferExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClientEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    ClientPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DesiredWindow_EndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DesiredWindow_StartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ManagementToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OfferedSlot_EndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OfferedSlot_StartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaitlistEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaitlistEntries_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WaitlistEntries_OfferedEmployee",
                        column: x => x.OfferedEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WaitlistEntries_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WaitlistEntries_RequestedEmployee",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WaitlistEntries_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecurringSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeLocationAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringSchedules_EmployeeLocationAssignments_EmployeeLoca~",
                        column: x => x.EmployeeLocationAssignmentId,
                        principalTable: "EmployeeLocationAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleExceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeLocationAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ModifiedStartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    ModifiedEndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleExceptions_EmployeeLocationAssignments_EmployeeLoca~",
                        column: x => x.EmployeeLocationAssignmentId,
                        principalTable: "EmployeeLocationAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ClientId",
                table: "Bookings",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EmployeeId_Status",
                table: "Bookings",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_LocationId",
                table: "Bookings",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_OrganizationId_ClientId",
                table: "Bookings",
                columns: new[] { "OrganizationId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_OrganizationId_LocationId",
                table: "Bookings",
                columns: new[] { "OrganizationId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RecurringSeriesId",
                table: "Bookings",
                column: "RecurringSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ServiceId",
                table: "Bookings",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_UserId",
                table: "Clients",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLocationAssignments_EmployeeId_LocationId",
                table: "EmployeeLocationAssignments",
                columns: new[] { "EmployeeId", "LocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLocationAssignments_LocationId",
                table: "EmployeeLocationAssignments",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_OrganizationId",
                table: "Employees",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_OrganizationId_UserId",
                table: "Employees",
                columns: new[] { "OrganizationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_OrganizationId",
                table: "Locations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationServiceOverrides_LocationId_ServiceId",
                table: "LocationServiceOverrides",
                columns: new[] { "LocationId", "ServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationServiceOverrides_ServiceId",
                table: "LocationServiceOverrides",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_OrganizationId_UserId",
                table: "OrganizationMembers",
                columns: new[] { "OrganizationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Slug",
                table: "Organizations",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringSchedules_EmployeeLocationAssignmentId_DayOfWeek",
                table: "RecurringSchedules",
                columns: new[] { "EmployeeLocationAssignmentId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookingId",
                table: "Reviews",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_EmployeeId",
                table: "Reviews",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_LocationId",
                table: "Reviews",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_OrganizationId_EmployeeId_IsHidden",
                table: "Reviews",
                columns: new[] { "OrganizationId", "EmployeeId", "IsHidden" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleExceptions_EmployeeLocationAssignmentId_Date",
                table: "ScheduleExceptions",
                columns: new[] { "EmployeeLocationAssignmentId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_OrganizationId",
                table: "Services",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_EmployeeId",
                table: "WaitlistEntries",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_LocationId",
                table: "WaitlistEntries",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_OfferedEmployeeId",
                table: "WaitlistEntries",
                column: "OfferedEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_OrganizationId_LocationId_ServiceId_Status",
                table: "WaitlistEntries",
                columns: new[] { "OrganizationId", "LocationId", "ServiceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_ServiceId",
                table: "WaitlistEntries",
                column: "ServiceId");
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql("""
                ALTER TABLE "Bookings" ADD CONSTRAINT "EX_Bookings_NoOverlapPerEmployee"
                EXCLUDE USING gist (
                    "EmployeeId" WITH =,
                    tstzrange("TimeSlot_StartUtc", "TimeSlot_EndUtc", '[)') WITH &&
                )
                WHERE ("Status" = 'Confirmed');
                """);

            migrationBuilder.Sql("""CREATE UNIQUE INDEX "IX_Clients_Phone" ON "Clients" ("Phone");""");

            migrationBuilder.Sql("""ALTER TABLE "Locations" ENABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""
                CREATE POLICY tenant_isolation ON "Locations"
                    USING ("OrganizationId" = current_setting('app.current_organization_id', true)::uuid);
                """);

            migrationBuilder.Sql("""ALTER TABLE "OrganizationMembers" ENABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""
                CREATE POLICY tenant_isolation ON "OrganizationMembers"
                    USING ("OrganizationId" = current_setting('app.current_organization_id', true)::uuid);
                """);

            migrationBuilder.Sql("""ALTER TABLE "Employees" ENABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""
                CREATE POLICY tenant_isolation ON "Employees"
                    USING ("OrganizationId" = current_setting('app.current_organization_id', true)::uuid);
                """);

            migrationBuilder.Sql("""ALTER TABLE "Services" ENABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""
                CREATE POLICY tenant_isolation ON "Services"
                    USING ("OrganizationId" = current_setting('app.current_organization_id', true)::uuid);
                """);

            migrationBuilder.Sql("""ALTER TABLE "Bookings" ENABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""
                CREATE POLICY tenant_isolation ON "Bookings"
                    USING ("OrganizationId" = current_setting('app.current_organization_id', true)::uuid);
                """);

            migrationBuilder.Sql("""ALTER TABLE "WaitlistEntries" ENABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""
                CREATE POLICY tenant_isolation ON "WaitlistEntries"
                    USING ("OrganizationId" = current_setting('app.current_organization_id', true)::uuid);
                """);

            migrationBuilder.Sql("""ALTER TABLE "Reviews" ENABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""
                CREATE POLICY tenant_isolation ON "Reviews"
                    USING ("OrganizationId" = current_setting('app.current_organization_id', true)::uuid);
                """);

            migrationBuilder.Sql("""ALTER TABLE "EmployeeLocationAssignments" ENABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""
                CREATE POLICY tenant_isolation ON "EmployeeLocationAssignments"
                    USING (EXISTS (
                        SELECT 1 FROM "Locations" l
                        WHERE l."Id" = "LocationId" AND l."OrganizationId" = current_setting('app.current_organization_id', true)::uuid
                    ));
                """);

            migrationBuilder.Sql("""ALTER TABLE "RecurringSchedules" ENABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""
                CREATE POLICY tenant_isolation ON "RecurringSchedules"
                    USING (EXISTS (
                        SELECT 1 FROM "EmployeeLocationAssignments" a
                        JOIN "Locations" l ON l."Id" = a."LocationId"
                        WHERE a."Id" = "EmployeeLocationAssignmentId" AND l."OrganizationId" = current_setting('app.current_organization_id', true)::uuid
                    ));
                """);

            migrationBuilder.Sql("""ALTER TABLE "ScheduleExceptions" ENABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""
                CREATE POLICY tenant_isolation ON "ScheduleExceptions"
                    USING (EXISTS (
                        SELECT 1 FROM "EmployeeLocationAssignments" a
                        JOIN "Locations" l ON l."Id" = a."LocationId"
                        WHERE a."Id" = "EmployeeLocationAssignmentId" AND l."OrganizationId" = current_setting('app.current_organization_id', true)::uuid
                    ));
                """);

            migrationBuilder.Sql("""ALTER TABLE "LocationServiceOverrides" ENABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""
                CREATE POLICY tenant_isolation ON "LocationServiceOverrides"
                    USING (EXISTS (
                        SELECT 1 FROM "Locations" l
                        WHERE l."Id" = "LocationId" AND l."OrganizationId" = current_setting('app.current_organization_id', true)::uuid
                    ));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP POLICY tenant_isolation ON "LocationServiceOverrides";""");
            migrationBuilder.Sql("""ALTER TABLE "LocationServiceOverrides" DISABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""DROP POLICY tenant_isolation ON "ScheduleExceptions";""");
            migrationBuilder.Sql("""ALTER TABLE "ScheduleExceptions" DISABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""DROP POLICY tenant_isolation ON "RecurringSchedules";""");
            migrationBuilder.Sql("""ALTER TABLE "RecurringSchedules" DISABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""DROP POLICY tenant_isolation ON "EmployeeLocationAssignments";""");
            migrationBuilder.Sql("""ALTER TABLE "EmployeeLocationAssignments" DISABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""DROP POLICY tenant_isolation ON "Reviews";""");
            migrationBuilder.Sql("""ALTER TABLE "Reviews" DISABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""DROP POLICY tenant_isolation ON "WaitlistEntries";""");
            migrationBuilder.Sql("""ALTER TABLE "WaitlistEntries" DISABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""DROP POLICY tenant_isolation ON "Bookings";""");
            migrationBuilder.Sql("""ALTER TABLE "Bookings" DISABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""DROP POLICY tenant_isolation ON "Services";""");
            migrationBuilder.Sql("""ALTER TABLE "Services" DISABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""DROP POLICY tenant_isolation ON "Employees";""");
            migrationBuilder.Sql("""ALTER TABLE "Employees" DISABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""DROP POLICY tenant_isolation ON "OrganizationMembers";""");
            migrationBuilder.Sql("""ALTER TABLE "OrganizationMembers" DISABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""DROP POLICY tenant_isolation ON "Locations";""");
            migrationBuilder.Sql("""ALTER TABLE "Locations" DISABLE ROW LEVEL SECURITY;""");
            migrationBuilder.Sql("""DROP INDEX "IX_Clients_Phone";""");
            migrationBuilder.Sql("""ALTER TABLE "Bookings" DROP CONSTRAINT "EX_Bookings_NoOverlapPerEmployee";""");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "LocationServiceOverrides");

            migrationBuilder.DropTable(
                name: "OrganizationMembers");

            migrationBuilder.DropTable(
                name: "RecurringSchedules");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "ScheduleExceptions");

            migrationBuilder.DropTable(
                name: "WaitlistEntries");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "EmployeeLocationAssignments");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
