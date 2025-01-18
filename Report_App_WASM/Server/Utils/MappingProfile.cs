using Report_App_WASM.Shared.DTO;

namespace Report_App_WASM.Server.Utils;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Add as many of these lines as you need to map your objects
        CreateMap<SftpConfiguration, SftpConfigurationDto>().ReverseMap();
        CreateMap<SystemParameters, SystemParametersDto>().ReverseMap();
        CreateMap<DataProvider, DataProviderDto>().ReverseMap();
        CreateMap<DatabaseConnection, DatabaseConnectionDto>().ReverseMap();
        CreateMap<FileStorageLocation, FileStorageLocationDto>().ReverseMap();
        CreateMap<SystemServicesStatus, SystemServicesStatusDto>().ReverseMap();
        CreateMap<SmtpConfiguration, SmtpConfigurationDto>().ReverseMap();
        CreateMap<LdapConfiguration, LdapConfigurationDto>().ReverseMap();
        CreateMap<ScheduledTaskQuery, ScheduledTaskQueryDto>().ReverseMap();
        CreateMap<ScheduledTaskDistributionList, ScheduledTaskDistributionListDto>().ReverseMap();
        CreateMap<ScheduledTask, ScheduledTaskDto>().ReverseMap();
        CreateMap<QueryExecutionLog, QueryExecutionLogDto>();
        CreateMap<EmailLog, EmailLogDto>();
        CreateMap<ReportGenerationLog, ReportGenerationLogDto>();
        CreateMap<TaskLog, TaskLogDto>();
        CreateMap<SystemLog, SystemLogDto>();
        CreateMap<AuditTrail, AuditTrailDto>();
        CreateMap<TaskStepLog, TaskStepLogDto>();
        CreateMap<ApplicationUser, ApplicationUserDto>();
        CreateMap<TableMetadata, TableMetadataDto>();
        CreateMap<StoredQuery, StoredQueryDto>();
    }
}