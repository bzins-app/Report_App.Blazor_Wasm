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
            CreateMap<SFTPConfiguration, SFTPConfigurationDTO>().ReverseMap();
            CreateMap<ApplicationParameters, ApplicationParametersDTO>().ReverseMap();
            CreateMap<Activity, ActivityDTO>().ReverseMap();
            CreateMap<ActivityDbConnection, ActivityDbConnectionDTO>().ReverseMap();
            CreateMap<FileDepositPathConfiguration, FileDepositPathConfigurationDTO>().ReverseMap();
            CreateMap<ServicesStatus, ServicesStatusDTO>().ReverseMap();
            CreateMap<SMTPConfiguration, SMTPConfigurationDTO>().ReverseMap();
            CreateMap<LDAPConfiguration, LDAPConfigurationDTO>().ReverseMap();
            CreateMap<TaskDetail, TaskDetailDTO>().ReverseMap();
            CreateMap<TaskEmailRecipient, TaskEmailRecipientDTO>().ReverseMap();
            CreateMap<TaskHeader, TaskHeaderDTO>().ReverseMap();
            CreateMap<ApplicationLogQueryExecution, ApplicationLogQueryExecutionDTO>();
            CreateMap<ApplicationLogEmailSender, ApplicationLogEmailSenderDTO>();
            CreateMap<ApplicationLogReportResult, ApplicationLogReportResultDTO>();
            CreateMap<ApplicationLogTask, ApplicationLogTaskDTO>();
            CreateMap<ApplicationLogSystem, ApplicationLogSystemDTO>();
            CreateMap<ApplicationAuditTrail, ApplicationAuditTrailDTO>();
            CreateMap<ApplicationLogTaskDetails, ApplicationLogTaskDetailsDTO>();
        }
    }
}
