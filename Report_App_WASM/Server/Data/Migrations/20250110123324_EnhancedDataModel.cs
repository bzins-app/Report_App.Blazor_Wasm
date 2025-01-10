using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportAppWASM.Server.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");



            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "UserTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "UserRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "UserLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "UserClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "RoleClaims");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "UserRoles",
                newName: "IX_UserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "UserLogins",
                newName: "IX_UserLogins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "UserClaims",
                newName: "IX_UserClaims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "RoleClaims",
                newName: "IX_RoleClaims_RoleId");

            migrationBuilder.AlterColumn<string>(
                name: "SmtpUserName",
                table: "SmtpConfiguration",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromFullName",
                table: "SmtpConfiguration",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FromEmail",
                table: "SmtpConfiguration",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ConfigurationName",
                table: "SmtpConfiguration",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AddColumn<string>(
                name: "MiscValue",
                table: "SmtpConfiguration",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConfigurationName",
                table: "SftpConfiguration",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AddColumn<string>(
                name: "MiscValue",
                table: "SftpConfiguration",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "LdapConfiguration",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConfigurationName",
                table: "LdapConfiguration",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AddColumn<string>(
                name: "MiscValue",
                table: "LdapConfiguration",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserLastName",
                table: "Users",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserFirstName",
                table: "Users",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModificationUser",
                table: "Users",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Culture",
                table: "Users",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUser",
                table: "Users",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTokens",
                table: "UserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserLogins",
                table: "UserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserClaims",
                table: "UserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleClaims",
                table: "RoleClaims",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AdHocQueryExecutionLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueryId = table.Column<int>(type: "int", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationInSeconds = table.Column<int>(type: "int", nullable: false),
                    DataProviderId = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    JobDescription = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    NbrOfRows = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Error = table.Column<bool>(type: "bit", nullable: false),
                    RunBy = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdHocQueryExecutionLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditTrail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TableName = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectedColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTrail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProvider",
                columns: table => new
                {
                    DataProviderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ProviderType = table.Column<int>(type: "int", nullable: false),
                    ProviderTypeName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    ProviderIcon = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProviderRoleId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MiscParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    MiscValue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProvider", x => x.DataProviderId);
                });

            migrationBuilder.CreateTable(
                name: "EmailLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationInSeconds = table.Column<int>(type: "int", nullable: false),
                    EmailTitle = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Error = table.Column<bool>(type: "bit", nullable: false),
                    NbrOfRecipients = table.Column<int>(type: "int", nullable: false),
                    RecipientList = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileStorageLocation",
                columns: table => new
                {
                    FileStorageLocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigurationName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsReachable = table.Column<bool>(type: "bit", nullable: false),
                    TryToCreateFolder = table.Column<bool>(type: "bit", nullable: false),
                    UseSftpProtocol = table.Column<bool>(type: "bit", nullable: false),
                    SftpConfigurationId = table.Column<int>(type: "int", nullable: true),
                    MiscValue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileStorageLocation", x => x.FileStorageLocationId);
                    table.ForeignKey(
                        name: "FK_FileStorageLocation_SftpConfiguration_SftpConfigurationId",
                        column: x => x.SftpConfigurationId,
                        principalTable: "SftpConfiguration",
                        principalColumn: "SftpConfigurationId");
                });

            migrationBuilder.CreateTable(
                name: "QueryExecutionLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeDb = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Database = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CommandTimeOut = table.Column<int>(type: "int", nullable: false),
                    DataProviderId = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TaskLogId = table.Column<int>(type: "int", nullable: false),
                    ScheduledTaskId = table.Column<int>(type: "int", nullable: false),
                    ScheduledTaskQueryId = table.Column<int>(type: "int", nullable: false),
                    QueryName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferBeginDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    SqlExcecutionDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    DownloadDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    RowsFetched = table.Column<int>(type: "int", nullable: false),
                    RunBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TypeJob = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryExecutionLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportGenerationLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DataProviderId = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TaskLogId = table.Column<int>(type: "int", nullable: false),
                    ScheduledTaskId = table.Column<int>(type: "int", nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    SubName = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    ReportPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FileSizeInMb = table.Column<double>(type: "float", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Error = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportGenerationLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeStampAppHour = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Browser = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FullVersion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Host = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Path = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    User = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ApplicationLogo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LoginScreenBackgroundImage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AdminEmails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmailPrefix = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ErrorEmailPrefix = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ErrorEMailMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    WelcomeEMailMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AlertEmailPrefix = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LogsRetentionInDays = table.Column<int>(type: "int", nullable: false),
                    ActivateTaskSchedulerModule = table.Column<bool>(type: "bit", nullable: false),
                    ActivateAdHocQueriesModule = table.Column<bool>(type: "bit", nullable: false),
                    MiscValue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemServicesStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailService = table.Column<bool>(type: "bit", nullable: false),
                    ReportService = table.Column<bool>(type: "bit", nullable: false),
                    AlertService = table.Column<bool>(type: "bit", nullable: false),
                    DataTransferService = table.Column<bool>(type: "bit", nullable: false),
                    CleanerService = table.Column<bool>(type: "bit", nullable: false),
                    MiscValue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemServicesStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemUniqueKey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemUniqueKey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskLog",
                columns: table => new
                {
                    TaskLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduledTaskId = table.Column<int>(type: "int", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationInSeconds = table.Column<int>(type: "int", nullable: false),
                    DataProviderId = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    JobDescription = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Error = table.Column<bool>(type: "bit", nullable: false),
                    HasSteps = table.Column<bool>(type: "bit", nullable: false),
                    MiscValue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RunBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskLog", x => x.TaskLogId);
                });

            migrationBuilder.CreateTable(
                name: "TaskStepLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskLogId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Step = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatedLogType = table.Column<int>(type: "int", nullable: false),
                    RelatedLogId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskStepLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SaveName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TypeConfiguration = table.Column<int>(type: "int", nullable: false),
                    IdIntConfiguration = table.Column<int>(type: "int", nullable: false),
                    IdStringConfiguration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SavedValues = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    MiscParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    MiscValue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseConnection",
                columns: table => new
                {
                    DatabaseConnectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConnectionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TypeDb = table.Column<int>(type: "int", nullable: false),
                    TypeDbName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DbConnectionParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ConnectionLogin = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CommandTimeOut = table.Column<int>(type: "int", nullable: false),
                    CommandFetchSize = table.Column<int>(type: "int", nullable: false),
                    UseTableMetaData = table.Column<bool>(type: "bit", nullable: false),
                    UseTableMetaDataFromAnotherProvider = table.Column<bool>(type: "bit", nullable: false),
                    IdTableMetaData = table.Column<int>(type: "int", nullable: false),
                    AdHocQueriesMaxNbrofRowsFetched = table.Column<int>(type: "int", nullable: false),
                    TaskSchedulerMaxNbrofRowsFetched = table.Column<int>(type: "int", nullable: false),
                    DataTransferMaxNbrofRowsFetched = table.Column<int>(type: "int", nullable: false),
                    RetryPatternParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    MiscParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DataProviderId = table.Column<int>(type: "int", nullable: false),
                    MiscValue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseConnection", x => x.DatabaseConnectionId);
                    table.ForeignKey(
                        name: "FK_DatabaseConnection_DataProvider_DataProviderId",
                        column: x => x.DataProviderId,
                        principalTable: "DataProvider",
                        principalColumn: "DataProviderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledTask",
                columns: table => new
                {
                    ScheduledTaskId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IdDataProvider = table.Column<int>(type: "int", nullable: false),
                    TaskNamePrefix = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TypeFile = table.Column<int>(type: "int", nullable: false),
                    TypeFileName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SendByEmail = table.Column<bool>(type: "bit", nullable: false),
                    ReportsRetentionInDays = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TaskParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CronParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    UseGlobalQueryParameters = table.Column<bool>(type: "bit", nullable: false),
                    GlobalQueryParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastRunDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FileStorageLocationId = table.Column<int>(type: "int", nullable: false),
                    MiscParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DataProviderId = table.Column<int>(type: "int", nullable: false),
                    MiscValue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTask", x => x.ScheduledTaskId);
                    table.ForeignKey(
                        name: "FK_ScheduledTask_DataProvider_DataProviderId",
                        column: x => x.DataProviderId,
                        principalTable: "DataProvider",
                        principalColumn: "DataProviderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoredQuery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdDataProvider = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    QueryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    QueryParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    MiscParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DataProviderId = table.Column<int>(type: "int", nullable: false),
                    MiscValue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredQuery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoredQuery_DataProvider_DataProviderId",
                        column: x => x.DataProviderId,
                        principalTable: "DataProvider",
                        principalColumn: "DataProviderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    TableDescription = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    ColumnName = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    ColumnDescription = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    IsSnippet = table.Column<bool>(type: "bit", nullable: false),
                    MiscParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DatabaseConnectionId = table.Column<int>(type: "int", nullable: false),
                    MiscValue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableMetadata_DatabaseConnection_DatabaseConnectionId",
                        column: x => x.DatabaseConnectionId,
                        principalTable: "DatabaseConnection",
                        principalColumn: "DatabaseConnectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledTaskDistributionList",
                columns: table => new
                {
                    ScheduledTaskDistributionListId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Recipients = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    EmailMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ScheduledTaskId = table.Column<int>(type: "int", nullable: false),
                    MiscValue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTaskDistributionList", x => x.ScheduledTaskDistributionListId);
                    table.ForeignKey(
                        name: "FK_ScheduledTaskDistributionList_ScheduledTask_ScheduledTaskId",
                        column: x => x.ScheduledTaskId,
                        principalTable: "ScheduledTask",
                        principalColumn: "ScheduledTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledTaskQuery",
                columns: table => new
                {
                    ScheduledTaskQueryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QueryParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ExecutionParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ExecutionOrder = table.Column<int>(type: "int", nullable: false),
                    LastRunDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutionCount = table.Column<int>(type: "int", nullable: false),
                    ScheduledTaskId = table.Column<int>(type: "int", nullable: false),
                    MiscValue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTaskQuery", x => x.ScheduledTaskQueryId);
                    table.ForeignKey(
                        name: "FK_ScheduledTaskQuery_ScheduledTask_ScheduledTaskId",
                        column: x => x.ScheduledTaskId,
                        principalTable: "ScheduledTask",
                        principalColumn: "ScheduledTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"INSERT INTO [dbo].[SystemUniqueKey]
           ([Id])    
           SELECT [Id]
  FROM [dbo].[ApplicationUniqueKey]
");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[SystemServicesStatus]
           ([EmailService]
           ,[ReportService]
           ,[AlertService]
           ,[DataTransferService]
           ,[CleanerService]
           ,[CreateDateTime]
           ,[CreateUser]
           ,[ModDateTime]
           ,[ModificationUser])
SELECT [EmailService]
      ,[ReportService]
      ,[AlertService]
      ,[DataTransferService]
      ,[CleanerService]
      ,[CreateDateTime]
      ,[CreateUser]
      ,[ModDateTime]
      ,[ModificationUser]
  FROM [dbo].[ServicesStatus]");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[SystemParameters]
           ([ApplicationName]
           ,[ApplicationLogo]
           ,[LoginScreenBackgroundImage]
           ,[AdminEmails]
           ,[EmailPrefix]
           ,[ErrorEmailPrefix]
           ,[ErrorEMailMessage]
           ,[WelcomeEMailMessage]
           ,[AlertEmailPrefix]
           ,[LogsRetentionInDays]
           ,[ActivateTaskSchedulerModule]
           ,[ActivateAdHocQueriesModule]
           ,[CreateDateTime]
           ,[CreateUser]
           ,[ModDateTime]
           ,[ModificationUser])
SELECT [ApplicationName]
      ,[ApplicationLogo]
      ,[LoginScreenBackgroundImage]
      ,[AdminEmails]
      ,[EmailPrefix]
      ,[ErrorEmailPrefix]
      ,[ErrorEMailMessage]
      ,[WelcomeEMailMessage]
      ,[AlertEmailPrefix]
      ,[LogsRetentionInDays]
      ,[ActivateTaskSchedulerModule]
      ,[ActivateAdHocQueriesModule]
      ,[CreateDateTime]
      ,[CreateUser]
      ,[ModDateTime]
      ,[ModificationUser]
  FROM [dbo].[ApplicationParameters]
");
            migrationBuilder.Sql(@"
INSERT INTO [dbo].[SystemLog]
           ([TimeStampAppHour]
           ,[Browser]
           ,[Platform]
           ,[FullVersion]
           ,[Host]
           ,[Path]
           ,[User]
           ,[EventId]
           ,[Level]
           ,[Message]
           ,[Name]
           ,[TimeStamp])
SELECT 
      [TimeStampAppHour]
      ,[Browser]
      ,[Platform]
      ,[FullVersion]
      ,[Host]
      ,[Path]
      ,[User]
      ,[EventId]
      ,[Level]
      ,[Message]
      ,[Name]
      ,[TimeStamp]
  FROM [dbo].[ApplicationLogSystem]
");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[AuditTrail]
           ([UserId]
           ,[Type]
           ,[TableName]
           ,[DateTime]
           ,[OldValues]
           ,[NewValues]
           ,[AffectedColumns]
           ,[PrimaryKey])
SELECT [UserId]
      ,[Type]
      ,[TableName]
      ,[DateTime]
      ,[OldValues]
      ,[NewValues]
      ,[AffectedColumns]
      ,[PrimaryKey]
  FROM [dbo].[ApplicationAuditTrail]");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[EmailLog]
           ([StartDateTime]
           ,[EndDateTime]
           ,[DurationInSeconds]
           ,[EmailTitle]
           ,[Result]
           ,[Error]
           ,[NbrOfRecipients]
           ,[RecipientList])
SELECT [StartDateTime]
      ,[EndDateTime]
      ,[DurationInSeconds]
      ,[EmailTitle]
      ,[Result]
      ,[Error]
      ,[NbrOfRecipients]
      ,[RecipientList]
  FROM [dbo].[ApplicationLogEmailSender]");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[UserPreferences]
           ([UserId]
           ,[SaveName]
           ,[Parameters]
           ,[TypeConfiguration]
           ,[IdIntConfiguration]
           ,[IdStringConfiguration]
           ,[SavedValues]
           ,[CreateDateTime]
           ,[CreateUser]
           ,[ModDateTime]
           ,[ModificationUser])
SELECT [UserId]
      ,[SaveName]
      ,[Parameters]
      ,[TypeConfiguration]
      ,[IdIntConfiguration]
      ,[IdStringConfiguration]
      ,[SavedValues]
      ,[CreateDateTime]
      ,[CreateUser]
      ,[ModDateTime]
      ,[ModificationUser]
  FROM [dbo].[UserSavedConfiguration]
");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[FileStorageLocation]
           ([ConfigurationName]
           ,[FilePath]
           ,[IsReachable]
           ,[TryToCreateFolder]
           ,[UseSftpProtocol]
           ,[SftpConfigurationId]
           ,[MiscValue]
           ,[CreateDateTime]
           ,[CreateUser]
           ,[ModDateTime]
           ,[ModificationUser])
SELECT [ConfigurationName]
      ,[FilePath]
      ,[IsReachable]
      ,[TryToCreateFolder]
      ,[UseSftpProtocol]
      ,[SftpConfigurationId]
	  ,[FileDepositPathConfigurationId]
      ,[CreateDateTime]
      ,[CreateUser]
      ,[ModDateTime]
      ,[ModificationUser]
  FROM [dbo].[FileDepositPathConfiguration]");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[DataProvider]
           ([ProviderName]
           ,[ProviderType]
           ,[ProviderTypeName]
           ,[IsEnabled]
           ,[IsVisible]
           ,[ProviderIcon]
           ,[ProviderRoleId]
           ,[MiscValue]
           ,[CreateDateTime]
           ,[CreateUser]
           ,[ModDateTime]
           ,[ModificationUser])
SELECT [ActivityName]
      ,[ActivityType]
      ,[ActivityTypeName]
      ,[IsActivated]
      ,[Display]
      ,[ActivityLogo]
      ,[ActivityRoleId]
	  ,[ActivityId]
      ,[CreateDateTime]
      ,[CreateUser]
      ,[ModDateTime]
      ,[ModificationUser]
  FROM [dbo].[Activity]
");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[DatabaseConnection]
           ([ConnectionType]
           ,[TypeDb]
           ,[TypeDbName]
           ,[DbConnectionParameters]
           ,[ConnectionLogin]
           ,[Password]
           ,[CommandTimeOut]
           ,[CommandFetchSize]
		  ,[UseTableMetaData]
		  ,[UseTableMetaDataFromAnotherProvider]
		  ,[IdTableMetaData]
           ,[AdHocQueriesMaxNbrofRowsFetched]
           ,[TaskSchedulerMaxNbrofRowsFetched]
           ,[DataTransferMaxNbrofRowsFetched]
           ,[DataProviderId]
           ,[MiscValue]
           ,[CreateDateTime]
           ,[CreateUser]
           ,[ModDateTime]
           ,[ModificationUser])
SELECT adc.[ConnectionType]
      ,adc.[TypeDb]
      ,adc.[TypeDbName]
	  ,adc.[DbConnectionParameters]
      ,adc.[ConnectionLogin]
      ,adc.[Password]
      ,adc.[CommandTimeOut]
      ,adc.[CommandFetchSize]    
      ,adc.[UseTablesDescriptions]
	  ,adc.[UseDescriptionsFromAnotherActivity]
	  , coalesce (dpvm.[Id],0)  
      ,adc.[AdHocQueriesMaxNbrofRowsFetched]
      ,adc.[TaskSchedulerMaxNbrofRowsFetched]
      ,adc.[DataTransferMaxNbrofRowsFetched]
      ,dpv.DataProviderId
	  ,adc.[Id]
      ,adc.[CreateDateTime]
      ,adc.[CreateUser]
      ,adc.[ModDateTime]
      ,adc.[ModificationUser]         
  FROM [dbo].[ActivityDbConnection] adc
  join [dbo].[DataProvider] dpv on dpv.MiscValue=adc.[ActivityId]
  left join [dbo].[ActivityDbConnection] dpvm on dpvm.[Id]=adc.[IdDescriptions]");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[ScheduledTask]
           ([TaskName]
           ,[ProviderName]
           ,[IdDataProvider]
           ,[TaskNamePrefix]
           ,[Type]
           ,[TypeName]
           ,[TypeFile]
           ,[TypeFileName]
           ,[IsEnabled]
           ,[SendByEmail]
           ,[ReportsRetentionInDays]
           ,[Comment]
           ,[Tags]
           ,[TaskParameters]
           ,[CronParameters]
           ,[UseGlobalQueryParameters]
           ,[GlobalQueryParameters]
           ,[LastRunDateTime]
           ,[FileStorageLocationId]
           ,[DataProviderId]
           ,[MiscValue]
           ,[CreateDateTime]
           ,[CreateUser]
           ,[ModDateTime]
           ,[ModificationUser])
SELECT th.[TaskName]
      ,th.[ActivityName]
      ,dpv.[DataProviderId]
      ,th.[TaskNamePrefix]
      ,th.[Type]
      ,th.[TypeName]
      ,th.[TypeFile]
      ,th.[TypeFileName]
      ,th.[IsActivated]
      ,th.[SendByEmail]
      ,th.[ReportsRetentionInDays]
      ,th.[Comment]
	  ,th.[Tags]
      ,th.[TaskHeaderParameters]
      ,th.[CronParameters]
      ,th.[UseGlobalQueryParameters]
      ,th.[QueryParameters]
      ,th.[LastRunDateTime]
      ,fsl.FileStorageLocationId
      ,dpv.[DataProviderId]
	  ,th.[TaskHeaderId]
      ,th.[CreateDateTime]
      ,th.[CreateUser]
      ,th.[ModDateTime]
      ,th.[ModificationUser]   
  FROM [dbo].[TaskHeader] th
  join [dbo].[DataProvider] dpv on dpv.MiscValue=th.[ActivityId]
  left join  [dbo].[FileStorageLocation] fsl on fsl.MiscValue=th.[FileDepositPathConfigurationId]
");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[ScheduledTaskDistributionList]
           ([Recipients]
           ,[EmailMessage]
           ,[ScheduledTaskId]
           ,[MiscValue]
           ,[CreateDateTime]
           ,[CreateUser]
           ,[ModDateTime]
           ,[ModificationUser])
SELECT 
     dl.[Email]
      ,dl.[Message]
      ,th.[ScheduledTaskId]
	  ,dl.[TaskEmailRecipientId]
      ,dl.[CreateDateTime]
      ,dl.[CreateUser]
      ,dl.[ModDateTime]
      ,dl.[ModificationUser]
  FROM [dbo].[TaskEmailRecipient] dl
  join [dbo].[ScheduledTask] th on th.MiscValue=dl.[TaskHeaderId]");
            migrationBuilder.Sql(@"  INSERT INTO [dbo].[ScheduledTaskQuery]
           ([QueryName]
           ,[Query]
           ,[QueryParameters]
           ,[ExecutionParameters]
           ,[ExecutionOrder]
           ,[LastRunDateTime]
           ,[ExecutionCount]
           ,[ScheduledTaskId]
           ,[MiscValue]
           ,[CreateDateTime]
           ,[CreateUser]
           ,[ModDateTime]
           ,[ModificationUser])
SELECT td.[QueryName]
      ,td.[Query]
	  ,td.[QueryParameters]
      ,td.[TaskDetailParameters]
      ,td.[DetailSequence]
      ,td.[LastRunDateTime]
      ,td.[NbrOfCumulativeOccurences]
      ,th.[ScheduledTaskId]  
      ,td.[TaskDetailId]
      ,td.[CreateDateTime]
      ,td.[CreateUser]
      ,td.[ModDateTime]
      ,td.[ModificationUser]
  FROM [dbo].[TaskDetail] td
   join [dbo].[ScheduledTask] th on th.MiscValue=td.[TaskHeaderId]
");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[StoredQuery]
           ([IdDataProvider]
           ,[ProviderName]
           ,[Comment]
           ,[Tags]
           ,[QueryName]
           ,[Query]
           ,[Parameters]
           ,[QueryParameters]
           ,[DataProviderId]
           ,[MiscValue]
           ,[CreateDateTime]
           ,[CreateUser]
           ,[ModDateTime]
           ,[ModificationUser])
SELECT 
      dpv.DataProviderId
	  ,qs.[ActivityName]
	  ,qs.[Comment]
      ,qs.[Tags]
      ,qs.[QueryName]
      ,qs.[Query]
	  ,qs.[Parameters]  
      ,qs.[QueryParameters]
      ,dpv.DataProviderId
	  ,qs.[Id]
      ,qs.[CreateDateTime]
      ,qs.[CreateUser]
      ,qs.[ModDateTime]
      ,qs.[ModificationUser]        
  FROM [dbo].[QueryStore] qs
  join [dbo].[DataProvider] dpv on dpv.MiscValue=qs.[ActivityId]");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[TableMetadata]
           ([TableName]
           ,[TableDescription]
           ,[ColumnName]
           ,[ColumnDescription]
           ,[IsSnippet]
           ,[DatabaseConnectionId]
           ,[MiscValue]
           ,[CreateDateTime]
           ,[CreateUser]
           ,[ModDateTime]
           ,[ModificationUser])
SELECT td.[TableName]
      ,td.[TableDescription]
      ,td.[ColumnName]
      ,td.[ColumnDescription]
	  ,td.[IsSnippet]
      ,dpvm.[DatabaseConnectionId]  
	  ,td.[Id]
      ,td.[CreateDateTime]
      ,td.[CreateUser]
      ,td.[ModDateTime]
      ,td.[ModificationUser]    
  FROM [dbo].[DbTableDescriptions] td
	left join [dbo].[DatabaseConnection] dpvm on dpvm.MiscValue=td.[ActivityDbConnectionId]
");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[TaskLog]
           ([ScheduledTaskId]
           ,[StartDateTime]
           ,[EndDateTime]
           ,[DurationInSeconds]
           ,[DataProviderId]
           ,[ProviderName]
           ,[JobDescription]
           ,[Type]
           ,[Result]
           ,[Error]
           ,[HasSteps]
           ,[MiscValue]
           ,[RunBy])
SELECT 
      coalesce(th.ScheduledTaskId,0)
      ,lt.[StartDateTime]
      ,lt.[EndDateTime]
      ,lt.[DurationInSeconds]
      ,coalesce(dpv.[DataProviderId],0)
      ,lt.[ActivityName]
      ,lt.[JobDescription]
      ,lt.[Type]
      ,lt.[Result]
      ,lt.[Error]
	  , case when lt.[TaskId]=0 then 0 else 1 end
	  ,lt.[Id]
      ,lt.[RunBy]
  FROM [dbo].[ApplicationLogTask] lt
  left join [dbo].[ScheduledTask] th on th.MiscValue=lt.[TaskId]
  left join [dbo].[DataProvider] dpv on dpv.MiscValue=lt.[ActivityId]");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[TaskStepLog]
           ([TaskLogId]
           ,[TimeStamp]
           ,[Step]
           ,[Info]
           ,[RelatedLogType]
           ,[RelatedLogId])
SELECT  coalesce(tl.TaskLogId,0)
      ,atd.[TimeStamp]
      ,atd.[Step]
      ,atd.[Info]
	  ,0,0
  FROM [dbo].[ApplicationLogTaskDetails] atd
  left join [dbo].[TaskLog] tl on tl.[MiscValue]=atd.TaskId");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[AdHocQueryExecutionLog]
           ([QueryId]
           ,[StartDateTime]
           ,[EndDateTime]
           ,[DurationInSeconds]
           ,[DataProviderId]
           ,[ProviderName]
           ,[JobDescription]
           ,[Type]
           ,[NbrOfRows]
           ,[Result]
           ,[Error]
           ,[RunBy])
SELECT 
      qs.[Id]
      ,aql.[StartDateTime]
      ,aql.[EndDateTime]
      ,aql.[DurationInSeconds]
      ,dpv.DataProviderId
      ,aql.[ActivityName]
      ,aql.[JobDescription]
      ,aql.[Type]
      ,aql.[NbrOfRows]
      ,aql.[Result]
      ,aql.[Error]
      ,aql.[RunBy]
  FROM [dbo].[ApplicationLogAdHocQueries] aql
	join [dbo].[DataProvider] dpv on dpv.MiscValue=aql.[ActivityId]
	join [dbo].[StoredQuery] qs on qs.MiscValue=aql.[QueryId]
");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[QueryExecutionLog]
           ([TypeDb]
           ,[Database]
           ,[CommandTimeOut]
           ,[DataProviderId]
           ,[ProviderName]
           ,[TaskLogId]
           ,[ScheduledTaskId]
           ,[ScheduledTaskQueryId]
           ,[QueryName]
           ,[Query]
           ,[StartDateTime]
           ,[TransferBeginDateTime]
           ,[EndDateTime]
           ,[TotalDuration]
           ,[SqlExcecutionDuration]
           ,[DownloadDuration]
           ,[RowsFetched]
           ,[RunBy]
           ,[TypeJob])
SELECT qel.[TypeDb]
      ,qel.[Database]
      ,qel.[CommandTimeOut]
      ,dpv.DataProviderId
      ,qel.[ActivityName]
	  ,0,0,0
      ,qel.[QueryName]
      ,qel.[Query]
      ,qel.[StartDateTime]
      ,qel.[TransferBeginDateTime]
      ,qel.[EndDateTime]
      ,qel.[TotalDuration]
      ,qel.[SqlExcecutionDuration]
      ,qel.[DownloadDuration]
      ,qel.[NbrOfRows]
      ,qel.[RunBy]
      ,qel.[TypeJob]
  FROM [dbo].[ApplicationLogQueryExecution] qel
  join [dbo].[DataProvider] dpv on dpv.MiscValue=qel.[ActivityId]");
            migrationBuilder.Sql(@"INSERT INTO [dbo].[ReportGenerationLog]
           ([CreatedAt]
           ,[CreatedBy]
           ,[DataProviderId]
           ,[ProviderName]
           ,[TaskLogId]
           ,[ScheduledTaskId]
           ,[ReportName]
           ,[SubName]
           ,[FileType]
           ,[FileName]
           ,[ReportPath]
           ,[FileSizeInMb]
           ,[IsAvailable]
           ,[Result]
           ,[Error])
SELECT rlr.[CreatedAt]
      ,rlr.[CreatedBy]
      ,dpv.DataProviderId
      ,rlr.[ActivityName]
	  ,0
      ,th.ScheduledTaskId
      ,rlr.[ReportName]
      ,rlr.[SubName]
      ,rlr.[FileType]
      ,rlr.[FileName]
      ,rlr.[ReportPath]
      ,rlr.[FileSizeInMb]
      ,rlr.[IsAvailable]
      ,rlr.[Result]
      ,rlr.[Error]
  FROM [dbo].[ApplicationLogReportResult] rlr
join [dbo].[DataProvider] dpv on dpv.MiscValue=rlr.[ActivityId]
left join [dbo].[ScheduledTask] th on th.MiscValue=rlr.[TaskHeaderId]");


            migrationBuilder.CreateIndex(
                name: "IX_AdHocQueryExecutionLog_DataProviderId_QueryId_JobDescription",
                table: "AdHocQueryExecutionLog",
                columns: new[] { "DataProviderId", "QueryId", "JobDescription" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrail_DateTime",
                table: "AuditTrail",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrail_Type_TableName",
                table: "AuditTrail",
                columns: new[] { "Type", "TableName" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrail_UserId",
                table: "AuditTrail",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseConnection_DataProviderId",
                table: "DatabaseConnection",
                column: "DataProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLog_EndDateTime",
                table: "EmailLog",
                column: "EndDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLog_Error_Result",
                table: "EmailLog",
                columns: new[] { "Error", "Result" });

            migrationBuilder.CreateIndex(
                name: "IX_FileStorageLocation_SftpConfigurationId",
                table: "FileStorageLocation",
                column: "SftpConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportGenerationLog_CreatedAt",
                table: "ReportGenerationLog",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReportGenerationLog_DataProviderId_ReportPath",
                table: "ReportGenerationLog",
                columns: new[] { "DataProviderId", "ReportPath" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportGenerationLog_IsAvailable",
                table: "ReportGenerationLog",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_ReportGenerationLog_ScheduledTaskId",
                table: "ReportGenerationLog",
                column: "ScheduledTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTask_DataProviderId",
                table: "ScheduledTask",
                column: "DataProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTaskDistributionList_ScheduledTaskId",
                table: "ScheduledTaskDistributionList",
                column: "ScheduledTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTaskQuery_ScheduledTaskId",
                table: "ScheduledTaskQuery",
                column: "ScheduledTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredQuery_DataProviderId",
                table: "StoredQuery",
                column: "DataProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredQuery_QueryName",
                table: "StoredQuery",
                column: "QueryName");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLog_EventId",
                table: "SystemLog",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLog_Level",
                table: "SystemLog",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLog_TimeStampAppHour",
                table: "SystemLog",
                column: "TimeStampAppHour");

            migrationBuilder.CreateIndex(
                name: "IX_TableMetadata_DatabaseConnectionId",
                table: "TableMetadata",
                column: "DatabaseConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TableMetadata_TableName_ColumnName",
                table: "TableMetadata",
                columns: new[] { "TableName", "ColumnName" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskLog_EndDateTime",
                table: "TaskLog",
                column: "EndDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_TaskLog_Error_Result",
                table: "TaskLog",
                columns: new[] { "Error", "Result" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskStepLog_Id_TimeStamp",
                table: "TaskStepLog",
                columns: new[] { "Id", "TimeStamp" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskStepLog_TaskLogId_Id",
                table: "TaskStepLog",
                columns: new[] { "TaskLogId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskStepLog_TaskLogId_TimeStamp",
                table: "TaskStepLog",
                columns: new[] { "TaskLogId", "TimeStamp" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId_TypeConfiguration_IdIntConfiguration",
                table: "UserPreferences",
                columns: new[] { "UserId", "TypeConfiguration", "IdIntConfiguration" });

            migrationBuilder.AddForeignKey(
                name: "FK_RoleClaims_Roles_RoleId",
                table: "RoleClaims",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserClaims_Users_UserId",
                table: "UserClaims",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLogins_Users_UserId",
                table: "UserLogins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);


            migrationBuilder.DropTable(
                name: "ApplicationAuditTrail");

            migrationBuilder.DropTable(
                name: "ApplicationLogAdHocQueries");

            migrationBuilder.DropTable(
                name: "ApplicationLogEmailSender");

            migrationBuilder.DropTable(
                name: "ApplicationLogQueryExecution");

            migrationBuilder.DropTable(
                name: "ApplicationLogReportResult");

            migrationBuilder.DropTable(
                name: "ApplicationLogSystem");

            migrationBuilder.DropTable(
                name: "ApplicationLogTask");

            migrationBuilder.DropTable(
                name: "ApplicationLogTaskDetails");

            migrationBuilder.DropTable(
                name: "ApplicationParameters");

            migrationBuilder.DropTable(
                name: "ApplicationUniqueKey");

            migrationBuilder.DropTable(
                name: "DbTableDescriptions");

            migrationBuilder.DropTable(
                name: "FileDepositPathConfiguration");

            migrationBuilder.DropTable(
                name: "QueryStore");

            migrationBuilder.DropTable(
                name: "ServicesStatus");

            migrationBuilder.DropTable(
                name: "TaskDetail");

            migrationBuilder.DropTable(
                name: "TaskEmailRecipient");

            migrationBuilder.DropTable(
                name: "UserSavedConfiguration");

            migrationBuilder.DropTable(
                name: "ActivityDbConnection");

            migrationBuilder.DropTable(
                name: "TaskHeader");

            migrationBuilder.DropTable(
                name: "Activity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleClaims_Roles_RoleId",
                table: "RoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_UserClaims_Users_UserId",
                table: "UserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLogins_Users_UserId",
                table: "UserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens");

            migrationBuilder.DropTable(
                name: "AdHocQueryExecutionLog");

            migrationBuilder.DropTable(
                name: "AuditTrail");

            migrationBuilder.DropTable(
                name: "EmailLog");

            migrationBuilder.DropTable(
                name: "FileStorageLocation");

            migrationBuilder.DropTable(
                name: "QueryExecutionLog");

            migrationBuilder.DropTable(
                name: "ReportGenerationLog");

            migrationBuilder.DropTable(
                name: "ScheduledTaskDistributionList");

            migrationBuilder.DropTable(
                name: "ScheduledTaskQuery");

            migrationBuilder.DropTable(
                name: "StoredQuery");

            migrationBuilder.DropTable(
                name: "SystemLog");

            migrationBuilder.DropTable(
                name: "SystemParameters");

            migrationBuilder.DropTable(
                name: "SystemServicesStatus");

            migrationBuilder.DropTable(
                name: "SystemUniqueKey");

            migrationBuilder.DropTable(
                name: "TableMetadata");

            migrationBuilder.DropTable(
                name: "TaskLog");

            migrationBuilder.DropTable(
                name: "TaskStepLog");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "ScheduledTask");

            migrationBuilder.DropTable(
                name: "DatabaseConnection");

            migrationBuilder.DropTable(
                name: "DataProvider");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTokens",
                table: "UserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserLogins",
                table: "UserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserClaims",
                table: "UserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleClaims",
                table: "RoleClaims");

            migrationBuilder.DropColumn(
                name: "MiscValue",
                table: "SmtpConfiguration");

            migrationBuilder.DropColumn(
                name: "MiscValue",
                table: "SftpConfiguration");

            migrationBuilder.DropColumn(
                name: "MiscValue",
                table: "LdapConfiguration");

            migrationBuilder.RenameTable(
                name: "UserTokens",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "UserLogins",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "UserClaims",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "RoleClaims",
                newName: "AspNetRoleClaims");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_RoleId",
                table: "AspNetUserRoles",
                newName: "IX_AspNetUserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_UserLogins_UserId",
                table: "AspNetUserLogins",
                newName: "IX_AspNetUserLogins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserClaims_UserId",
                table: "AspNetUserClaims",
                newName: "IX_AspNetUserClaims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RoleClaims_RoleId",
                table: "AspNetRoleClaims",
                newName: "IX_AspNetRoleClaims_RoleId");

            migrationBuilder.AlterColumn<string>(
                name: "SmtpUserName",
                table: "SmtpConfiguration",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromFullName",
                table: "SmtpConfiguration",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "FromEmail",
                table: "SmtpConfiguration",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "ConfigurationName",
                table: "SmtpConfiguration",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "ConfigurationName",
                table: "SftpConfiguration",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "LdapConfiguration",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConfigurationName",
                table: "LdapConfiguration",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "UserLastName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserFirstName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModificationUser",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Culture",
                table: "AspNetUsers",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUser",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Activity",
                columns: table => new
                {
                    ActivityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityLogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActivityName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ActivityRoleId = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    ActivityType = table.Column<int>(type: "int", nullable: false),
                    ActivityTypeName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Display = table.Column<bool>(type: "bit", nullable: false),
                    IsActivated = table.Column<bool>(type: "bit", nullable: false),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activity", x => x.ActivityId);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationAuditTrail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AffectedColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TableName = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationAuditTrail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLogAdHocQueries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    DurationInSeconds = table.Column<int>(type: "int", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Error = table.Column<bool>(type: "bit", nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    NbrOfRows = table.Column<int>(type: "int", nullable: false),
                    QueryId = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RunBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogAdHocQueries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLogEmailSender",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DurationInSeconds = table.Column<int>(type: "int", nullable: false),
                    EmailTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Error = table.Column<bool>(type: "bit", nullable: false),
                    NbrOfRecipients = table.Column<int>(type: "int", nullable: false),
                    RecipientList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogEmailSender", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLogQueryExecution",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CommandTimeOut = table.Column<int>(type: "int", nullable: false),
                    Database = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DownloadDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NbrOfRows = table.Column<int>(type: "int", nullable: false),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RunBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SqlExcecutionDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    TransferBeginDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TypeDb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeJob = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogQueryExecution", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLogReportResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Error = table.Column<bool>(type: "bit", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSizeInMb = table.Column<double>(type: "float", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReportPath = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TaskHeaderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogReportResult", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLogSystem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Browser = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    FullVersion = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Host = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    TimeStamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TimeStampAppHour = table.Column<DateTime>(type: "datetime2", nullable: false),
                    User = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogSystem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLogTask",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    DurationInSeconds = table.Column<int>(type: "int", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Error = table.Column<bool>(type: "bit", nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RunBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogTask", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLogTaskDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Step = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogTaskDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivateAdHocQueriesModule = table.Column<bool>(type: "bit", nullable: false),
                    ActivateTaskSchedulerModule = table.Column<bool>(type: "bit", nullable: false),
                    AdminEmails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlertEmailPrefix = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApplicationLogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmailPrefix = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ErrorEMailMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorEmailPrefix = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LoginScreenBackgroundImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogsRetentionInDays = table.Column<int>(type: "int", nullable: false),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WelcomeEMailMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUniqueKey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUniqueKey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileDepositPathConfiguration",
                columns: table => new
                {
                    FileDepositPathConfigurationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SftpConfigurationId = table.Column<int>(type: "int", nullable: true),
                    ConfigurationName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsReachable = table.Column<bool>(type: "bit", nullable: false),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TryToCreateFolder = table.Column<bool>(type: "bit", nullable: false),
                    UseSftpProtocol = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDepositPathConfiguration", x => x.FileDepositPathConfigurationId);
                    table.ForeignKey(
                        name: "FK_FileDepositPathConfiguration_SftpConfiguration_SftpConfigurationId",
                        column: x => x.SftpConfigurationId,
                        principalTable: "SftpConfiguration",
                        principalColumn: "SftpConfigurationId");
                });

            migrationBuilder.CreateTable(
                name: "ServicesStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlertService = table.Column<bool>(type: "bit", nullable: false),
                    CleanerService = table.Column<bool>(type: "bit", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DataTransferService = table.Column<bool>(type: "bit", nullable: false),
                    EmailService = table.Column<bool>(type: "bit", nullable: false),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReportService = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicesStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSavedConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdIntConfiguration = table.Column<int>(type: "int", nullable: false),
                    IdStringConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaveName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SavedValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeConfiguration = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSavedConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityDbConnection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    AdAuthentication = table.Column<bool>(type: "bit", nullable: false),
                    AdHocQueriesMaxNbrofRowsFetched = table.Column<int>(type: "int", nullable: false),
                    CommandFetchSize = table.Column<int>(type: "int", nullable: false),
                    CommandTimeOut = table.Column<int>(type: "int", nullable: false),
                    ConnectionLogin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConnectionPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConnectionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DataTransferMaxNbrofRowsFetched = table.Column<int>(type: "int", nullable: false),
                    DbConnectionParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DbSchema = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdDescriptions = table.Column<int>(type: "int", nullable: false),
                    IntentReadOnly = table.Column<bool>(type: "bit", nullable: false),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Port = table.Column<int>(type: "int", nullable: false),
                    TaskSchedulerMaxNbrofRowsFetched = table.Column<int>(type: "int", nullable: false),
                    TypeDb = table.Column<int>(type: "int", nullable: false),
                    TypeDbName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UseDbSchema = table.Column<bool>(type: "bit", nullable: false),
                    UseDescriptionsFromAnotherActivity = table.Column<bool>(type: "bit", nullable: false),
                    UseTablesDescriptions = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityDbConnection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityDbConnection_Activity_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activity",
                        principalColumn: "ActivityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QueryStore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdActivity = table.Column<int>(type: "int", nullable: false),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    QueryParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryStore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueryStore_Activity_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activity",
                        principalColumn: "ActivityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskHeader",
                columns: table => new
                {
                    TaskHeaderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CronParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileDepositPathConfigurationId = table.Column<int>(type: "int", nullable: false),
                    IdActivity = table.Column<int>(type: "int", nullable: false),
                    IsActivated = table.Column<bool>(type: "bit", nullable: false),
                    LastRunDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    QueryParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportsRetentionInDays = table.Column<int>(type: "int", nullable: false),
                    SendByEmail = table.Column<bool>(type: "bit", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaskHeaderParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TaskNamePrefix = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TypeFile = table.Column<int>(type: "int", nullable: false),
                    TypeFileName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TypeName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UseGlobalQueryParameters = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskHeader", x => x.TaskHeaderId);
                    table.ForeignKey(
                        name: "FK_TaskHeader_Activity_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activity",
                        principalColumn: "ActivityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DbTableDescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityDbConnectionId = table.Column<int>(type: "int", nullable: false),
                    ColumnDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColumnName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsSnippet = table.Column<bool>(type: "bit", nullable: false),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TableDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TableName = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbTableDescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbTableDescriptions_ActivityDbConnection_ActivityDbConnectionId",
                        column: x => x.ActivityDbConnectionId,
                        principalTable: "ActivityDbConnection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskDetail",
                columns: table => new
                {
                    TaskDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskHeaderId = table.Column<int>(type: "int", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DetailSequence = table.Column<int>(type: "int", nullable: false),
                    LastRunDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NbrOfCumulativeOccurences = table.Column<int>(type: "int", nullable: false),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QueryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QueryParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaskDetailParameters = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskDetail", x => x.TaskDetailId);
                    table.ForeignKey(
                        name: "FK_TaskDetail_TaskHeader_TaskHeaderId",
                        column: x => x.TaskHeaderId,
                        principalTable: "TaskHeader",
                        principalColumn: "TaskHeaderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskEmailRecipient",
                columns: table => new
                {
                    TaskEmailRecipientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskHeaderId = table.Column<int>(type: "int", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskEmailRecipient", x => x.TaskEmailRecipientId);
                    table.ForeignKey(
                        name: "FK_TaskEmailRecipient_TaskHeader_TaskHeaderId",
                        column: x => x.TaskHeaderId,
                        principalTable: "TaskHeader",
                        principalColumn: "TaskHeaderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityDbConnection_ActivityId",
                table: "ActivityDbConnection",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAuditTrail_DateTime",
                table: "ApplicationAuditTrail",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAuditTrail_Type_TableName",
                table: "ApplicationAuditTrail",
                columns: new[] { "Type", "TableName" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAuditTrail_UserId",
                table: "ApplicationAuditTrail",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogAdHocQueries_ActivityId_QueryId_JobDescription",
                table: "ApplicationLogAdHocQueries",
                columns: new[] { "ActivityId", "QueryId", "JobDescription" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogEmailSender_EndDateTime",
                table: "ApplicationLogEmailSender",
                column: "EndDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogEmailSender_Error_Result",
                table: "ApplicationLogEmailSender",
                columns: new[] { "Error", "Result" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogReportResult_ActivityId_ReportPath",
                table: "ApplicationLogReportResult",
                columns: new[] { "ActivityId", "ReportPath" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogReportResult_CreatedAt",
                table: "ApplicationLogReportResult",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogReportResult_IsAvailable",
                table: "ApplicationLogReportResult",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogReportResult_TaskHeaderId",
                table: "ApplicationLogReportResult",
                column: "TaskHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogSystem_EventId",
                table: "ApplicationLogSystem",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogSystem_Level",
                table: "ApplicationLogSystem",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogSystem_TimeStampAppHour",
                table: "ApplicationLogSystem",
                column: "TimeStampAppHour");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogTask_EndDateTime",
                table: "ApplicationLogTask",
                column: "EndDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogTask_Error_Result",
                table: "ApplicationLogTask",
                columns: new[] { "Error", "Result" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogTaskDetails_TaskId_Id",
                table: "ApplicationLogTaskDetails",
                columns: new[] { "TaskId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_DbTableDescriptions_ActivityDbConnectionId",
                table: "DbTableDescriptions",
                column: "ActivityDbConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DbTableDescriptions_TableName_ColumnName",
                table: "DbTableDescriptions",
                columns: new[] { "TableName", "ColumnName" });

            migrationBuilder.CreateIndex(
                name: "IX_FileDepositPathConfiguration_SftpConfigurationId",
                table: "FileDepositPathConfiguration",
                column: "SftpConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryStore_ActivityId",
                table: "QueryStore",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryStore_QueryName",
                table: "QueryStore",
                column: "QueryName");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDetail_TaskHeaderId",
                table: "TaskDetail",
                column: "TaskHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskEmailRecipient_TaskHeaderId",
                table: "TaskEmailRecipient",
                column: "TaskHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskHeader_ActivityId",
                table: "TaskHeader",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSavedConfiguration_UserId_TypeConfiguration_IdIntConfiguration",
                table: "UserSavedConfiguration",
                columns: new[] { "UserId", "TypeConfiguration", "IdIntConfiguration" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
