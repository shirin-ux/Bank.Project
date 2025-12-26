using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanGateway.Auth.Infrastructure.Migrations.Users
{
    /// <inheritdoc />
    public partial class UsersMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiftCardEligibleUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NationalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftCardEligibleUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtpCode",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Purpose = table.Column<byte>(type: "tinyint", nullable: false),
                    CodeHash = table.Column<byte[]>(type: "varbinary(64)", maxLength: 64, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsumedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    RequestIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpCode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<byte[]>(type: "varbinary(64)", maxLength: 64, nullable: false),
                    JwtId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReplacedByTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RotatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccessTokenExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    NationalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BirthDate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShenasnamehNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShenasnameSerial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShenasnameSeri = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsMobileVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsKycCompleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsProfileCompleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    KycLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    UidUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastLoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsKycVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    GiftStatus = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VerfiyMobileOwnerInquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    IsMatched = table.Column<bool>(type: "bit", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: true),
                    StatusMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RawResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerfiyMobileOwnerInquiry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KycVerification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShahkarStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    ShahkarMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalInfoStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    PersonalInfoMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankAccountStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    BankAccountMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    AddressMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EkycStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    EkycMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastShahkarAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPersonalInfoAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastBankAccountAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAddressAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastEkycAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastProviderPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KycVerification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KycVerification_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiftCardEligibleUsers_NationalCode",
                table: "GiftCardEligibleUsers",
                column: "NationalCode");

            migrationBuilder.CreateIndex(
                name: "IX_KycVerification_UserId",
                table: "KycVerification",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OtpCode_PhoneNumber",
                table: "OtpCode",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCode_PhoneNumber_Purpose_CreatedAtUtc",
                table: "OtpCode",
                columns: new[] { "PhoneNumber", "Purpose", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_JwtId",
                table: "RefreshTokens",
                column: "JwtId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_MobileNumber",
                table: "User",
                column: "MobileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_User_NationalCode",
                table: "User",
                column: "NationalCode");

            migrationBuilder.CreateIndex(
                name: "IX_VerfiyMobileOwnerInquiry_MobileNumber",
                table: "VerfiyMobileOwnerInquiry",
                column: "MobileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_VerfiyMobileOwnerInquiry_NationalId",
                table: "VerfiyMobileOwnerInquiry",
                column: "NationalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiftCardEligibleUsers");

            migrationBuilder.DropTable(
                name: "KycVerification");

            migrationBuilder.DropTable(
                name: "OtpCode");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "VerfiyMobileOwnerInquiry");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
