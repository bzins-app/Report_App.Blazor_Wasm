using AutoMapper;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Shared.DTO;

namespace Report_App_WASM.Server.Utils
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            CreateMap<SftpConfiguration, SftpConfigurationDto>().ReverseMap();
            CreateMap<ApplicationParameters, ApplicationParametersDto>().ReverseMap();
            CreateMap<Activity, ActivityDto>().ReverseMap();
            CreateMap<ActivityDbConnection, ActivityDbConnectionDto>().ReverseMap();
            CreateMap<FileDepositPathConfiguration, FileDepositPathConfigurationDto>().ReverseMap();
            CreateMap<ServicesStatus, ServicesStatusDto>().ReverseMap();
            CreateMap<SmtpConfiguration, SmtpConfigurationDto>().ReverseMap();
            CreateMap<LdapConfiguration, LdapConfigurationDto>().ReverseMap();
            CreateMap<TaskDetail, TaskDetailDto>().ReverseMap();
            CreateMap<TaskEmailRecipient, TaskEmailRecipientDto>().ReverseMap();
            CreateMap<TaskHeader, TaskHeaderDto>().ReverseMap();
            CreateMap<ApplicationLogQueryExecution, ApplicationLogQueryExecutionDto>();
            CreateMap<ApplicationLogEmailSender, ApplicationLogEmailSenderDto>();
            CreateMap<ApplicationLogReportResult, ApplicationLogReportResultDto>();
            CreateMap<ApplicationLogTask, ApplicationLogTaskDto>();
            CreateMap<ApplicationLogSystem, ApplicationLogSystemDto>();
            CreateMap<ApplicationAuditTrail, ApplicationAuditTrailDto>();
            CreateMap<ApplicationLogTaskDetails, ApplicationLogTaskDetailsDto>();
            CreateMap<ApplicationUser, ApplicationUserDto>();
            CreateMap<DbTableDescriptions, DbTableDescriptionsDto>();
            CreateMap<QueryStore, QueryStoreDto>();
        }
    }
}
