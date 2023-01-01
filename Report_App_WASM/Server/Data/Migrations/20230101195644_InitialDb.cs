using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportAppWASM.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activity",
                columns: table => new
                {
                    ActivityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ActivityType = table.Column<int>(type: "int", nullable: false),
                    ActivityTypeName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActivated = table.Column<bool>(type: "bit", nullable: false),
                    Display = table.Column<bool>(type: "bit", nullable: false),
                    ActivityLogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActivityRoleId = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TableName = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectedColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryKey = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationAuditTrail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLogEmailSender",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationInSeconds = table.Column<int>(type: "int", nullable: false),
                    EmailTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Error = table.Column<bool>(type: "bit", nullable: false),
                    NbrOfRecipients = table.Column<int>(type: "int", nullable: false),
                    RecipientList = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    TypeDb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Database = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommandTimeOut = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    QueryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferBeginDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    SqlExcecutionDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    DownloadDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    NbrOfRows = table.Column<int>(type: "int", nullable: false),
                    RunBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    TaskHeaderId = table.Column<int>(type: "int", nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SubName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReportPath = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FileSizeInMb = table.Column<double>(type: "float", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Error = table.Column<bool>(type: "bit", nullable: false)
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
                    TimeStampAppHour = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Browser = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    FullVersion = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Host = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Path = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    User = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
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
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationInSeconds = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    JobDescription = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Error = table.Column<bool>(type: "bit", nullable: false),
                    RunBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Step = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    ApplicationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApplicationLogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoginScreenBackgroundImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminEmails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailPrefix = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ErrorEmailPrefix = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ErrorEMailMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WelcomeEMailMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlertEmailPrefix = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LogsRetentionInDays = table.Column<int>(type: "int", nullable: false),
                    ActivateTaskSchedulerModule = table.Column<bool>(type: "bit", nullable: false),
                    ActivateAdHocQueriesModule = table.Column<bool>(type: "bit", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
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
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsBaseUser = table.Column<bool>(type: "bit", nullable: false),
                    UserFirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserLastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApplicationTheme = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Culture = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", maxLength: 100, nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LdapConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigurationName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActivated = table.Column<bool>(type: "bit", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LdapConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicesStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailService = table.Column<bool>(type: "bit", nullable: false),
                    ReportService = table.Column<bool>(type: "bit", nullable: false),
                    AlertService = table.Column<bool>(type: "bit", nullable: false),
                    DataTransferService = table.Column<bool>(type: "bit", nullable: false),
                    CleanerService = table.Column<bool>(type: "bit", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicesStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SftpConfiguration",
                columns: table => new
                {
                    SftpConfigurationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UseFtpProtocol = table.Column<bool>(type: "bit", nullable: false),
                    ConfigurationName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Host = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Port = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SftpConfiguration", x => x.SftpConfigurationId);
                });

            migrationBuilder.CreateTable(
                name: "SmtpConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigurationName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SmtpUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SmtpPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmtpHost = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SmtpPort = table.Column<int>(type: "int", nullable: false),
                    SmtpSsl = table.Column<bool>(type: "bit", nullable: false),
                    FromEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FromFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActivated = table.Column<bool>(type: "bit", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmtpConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityDbConnection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConnectionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TypeDb = table.Column<int>(type: "int", nullable: false),
                    TypeDbName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ConnectionPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    ConnectionLogin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UseDbSchema = table.Column<bool>(type: "bit", nullable: false),
                    AdAuthentication = table.Column<bool>(type: "bit", nullable: false),
                    IntentReadOnly = table.Column<bool>(type: "bit", nullable: false),
                    DbSchema = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommandTimeOut = table.Column<int>(type: "int", nullable: false),
                    CommandFetchSize = table.Column<int>(type: "int", nullable: false),
                    DbConnectionParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UseTablesDescriptions = table.Column<bool>(type: "bit", nullable: false),
                    AdHocQueriesMaxNbrofRowsFetched = table.Column<int>(type: "int", nullable: false),
                    TaskSchedulerMaxNbrofRowsFetched = table.Column<int>(type: "int", nullable: false),
                    DataTransferMaxNbrofRowsFetched = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
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
                    IdActivity = table.Column<int>(type: "int", nullable: false),
                    QueryName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
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
                    TaskName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IdActivity = table.Column<int>(type: "int", nullable: false),
                    TaskNamePrefix = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TypeFile = table.Column<int>(type: "int", nullable: false),
                    TypeFileName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActivated = table.Column<bool>(type: "bit", nullable: false),
                    SendByEmail = table.Column<bool>(type: "bit", nullable: false),
                    ReportsRetentionInDays = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaskHeaderParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CronParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UseGlobalQueryParameters = table.Column<bool>(type: "bit", nullable: false),
                    QueryParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastRunDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FileDepositPathConfigurationId = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
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
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "FileDepositPathConfiguration",
                columns: table => new
                {
                    FileDepositPathConfigurationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigurationName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsReachable = table.Column<bool>(type: "bit", nullable: false),
                    TryToCreateFolder = table.Column<bool>(type: "bit", nullable: false),
                    UseSftpProtocol = table.Column<bool>(type: "bit", nullable: false),
                    SftpConfigurationId = table.Column<int>(type: "int", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
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
                name: "DbTableDescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TableDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColumnName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ColumnDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActivityDbConnectionId = table.Column<int>(type: "int", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
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
                    QueryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaskDetailParameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryParameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DetailSequence = table.Column<int>(type: "int", nullable: false),
                    LastRunDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NbrOfCumulativeOccurences = table.Column<int>(type: "int", nullable: false),
                    TaskHeaderId = table.Column<int>(type: "int", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
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
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaskHeaderId = table.Column<int>(type: "int", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationAuditTrail");

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
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DbTableDescriptions");

            migrationBuilder.DropTable(
                name: "FileDepositPathConfiguration");

            migrationBuilder.DropTable(
                name: "LdapConfiguration");

            migrationBuilder.DropTable(
                name: "QueryStore");

            migrationBuilder.DropTable(
                name: "ServicesStatus");

            migrationBuilder.DropTable(
                name: "SmtpConfiguration");

            migrationBuilder.DropTable(
                name: "TaskDetail");

            migrationBuilder.DropTable(
                name: "TaskEmailRecipient");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ActivityDbConnection");

            migrationBuilder.DropTable(
                name: "SftpConfiguration");

            migrationBuilder.DropTable(
                name: "TaskHeader");

            migrationBuilder.DropTable(
                name: "Activity");
        }
    }
}
