using System.Text.Json;
using Report_App_WASM.Server.Services.EmailSender;
using Report_App_WASM.Server.Services.FilesManagement;
using Report_App_WASM.Server.Services.RemoteDb;
using Report_App_WASM.Server.Utils.BackgroundWorker;
using Report_App_WASM.Server.Utils.FIles;

namespace Report_App_WASM.Server.Services.BackgroundWorker
{
    public abstract class ScheduledTaskHandler : IDisposable
    {
        protected ApplicationDbContext _context;
        protected IRemoteDatabaseActionsHandler _dbReader;
        protected IEmailSender _emailSender;
        protected LocalFilesService _fileDeposit;
        protected IWebHostEnvironment _hostingEnvironment;
        protected IMapper _mapper;
        protected List<EmailRecipient> _emails = new();

        protected Dictionary<ScheduledTaskQuery, DataTable> _fetchedData = new();
        protected List<MemoryFileContainer> _fileResults = new();
        protected ScheduledTask _header = null!;
        protected TaskJobParameters _jobParameters = null!;

        protected JsonSerializerOptions _jsonOpt = new()
        {
            PropertyNameCaseInsensitive = true
        };

        protected long _taskId;
        protected TaskLog _logTask;

        protected ScheduledTaskHandler(ApplicationDbContext context, IEmailSender emailSender,
            IRemoteDatabaseActionsHandler dbReader, LocalFilesService fileDeposit, IMapper mapper,
            IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _emailSender = emailSender;
            _dbReader = dbReader;
            _fileDeposit = fileDeposit;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }


        protected async Task<ScheduledTask> GetScheduledTaskAsync(long scheduledTaskId)
        {
            return await _context.ScheduledTask
                       .Where(a => a.ScheduledTaskId == scheduledTaskId)
                       .Include(a => a.DataProvider)
                       .Include(a => a.TaskQueries)
                       .Include(a => a.DistributionLists)
                       .FirstOrDefaultAsync() ??
                   throw new InvalidOperationException($"ScheduledTask {scheduledTaskId} cannnot be retrieved");
        }

        protected async Task<DatabaseConnection> GetDatabaseConnectionAsync(long dataProviderId)
        {
            return await _context.DatabaseConnection
                       .Where(a => a.DataProvider.DataProviderId == dataProviderId)
                       .FirstOrDefaultAsync() ??
                   throw new InvalidOperationException($"DataProvider {dataProviderId} cannnot be retrieved");
        }

        protected TaskLog CreateTaskLog(TaskJobParameters parameters)
        {
            return new TaskLog
            {
                DataProviderId = _header.DataProvider.DataProviderId,
                ProviderName = _header.ProviderName,
                StartDateTime = DateTime.Now,
                JobDescription = _header.TaskName,
                Type = _header.Type.ToString(),
                Result = "Running",
                ScheduledTaskId = parameters.ScheduledTaskId,
                RunBy = _jobParameters.RunBy,
                EndDateTime = DateTime.Now
            };
        }

        protected async Task InsertLogTaskAsync(TaskLog logTask)
        {
            await _context.AddAsync(logTask);
            await _context.SaveChangesAsync("backgroundworker");
        }

        protected async Task HandleNoQueriesAsync(TaskLog logTask)
        {
            logTask.EndDateTime = DateTime.Now;
            logTask.Result = "No query to run";
            logTask.Error = true;
            logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
            await _context.AddAsync(logTask);
            await _context.SaveChangesAsync("backgroundworker");
        }

        protected async Task InsertLogTaskStepAsync(string step, string info, bool error)
        {
            _logTask.HasSteps = true;
            await _context.AddAsync(new TaskStepLog { TaskLogId = _taskId, Step = step, Info = info, Error = error });
            await _context.SaveChangesAsync("backgroundworker");
        }

        protected async Task FinalizeTaskAsync(TaskLog logTask, bool generateFiles, string _result = "Ok")
        {
            logTask.Error = false;
            logTask.Result = _result;
            logTask.EndDateTime = DateTime.Now;
            logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
            if (generateFiles)
            {
                _header.LastRunDateTime = DateTime.Now;
                _context.Entry(_header).State = EntityState.Modified;
            }

            await InsertLogTaskStepAsync("Job end", $"Total duration {logTask.DurationInSeconds} seconds", false);
        }

        protected async Task HandleTaskErrorAsync(TaskLog logTask, Exception ex)
        {
            logTask.Result = new string(ex.Message.Take(449).ToArray());
            logTask.Error = true;
            logTask.EndDateTime = DateTime.Now;
            logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
            logTask.Result = ex.Message.Length > 440 ? ex.Message.Substring(0, 440) : ex.Message;
            await _emailSender.GenerateErrorEmailAsync(ex.Message, _header.ProviderName + ": " + _header.TaskName);
            await InsertLogTaskStepAsync("Error", logTask.Result, true);
            _fetchedData.Clear();
        }

        protected async Task UpdateTaskLogAsync(TaskLog logTask)
        {
            _context.Update(logTask);
            await _context.SaveChangesAsync("backgroundworker");
        }

        protected async ValueTask FetchData(ScheduledTaskQuery detail, int maxRows = 100000)
        {
            using var remoteDb = new RemoteDatabaseActionsHandler(_context, _mapper);
            var detailParam =
                JsonSerializer.Deserialize<ScheduledTaskQueryParameters>(detail.ExecutionParameters!, _jsonOpt);
            List<QueryCommandParameter>? param = new();
            if (_jobParameters.QueryCommandParameters!.Any())
                param = _jobParameters.QueryCommandParameters;
            else if (_header.UseGlobalQueryParameters && _header.GlobalQueryParameters != "[]" &&
                     !string.IsNullOrEmpty(_header.GlobalQueryParameters))
                param = JsonSerializer.Deserialize<List<QueryCommandParameter>>(_header.GlobalQueryParameters,
                    _jsonOpt);

            if (detail.QueryParameters != "[]" && !string.IsNullOrEmpty(detail.QueryParameters))
            {
                var desParam =
                    JsonSerializer.Deserialize<List<QueryCommandParameter>>(detail.QueryParameters, _jsonOpt);
                foreach (var value in desParam!)
                    if (param!.All(a => a.ParameterIdentifier?.ToLower() != value.ParameterIdentifier?.ToLower()))
                        param?.Add(value);
            }

            var table = await remoteDb.RemoteDbToDatableAsync(
                new RemoteDbCommandParameters
                {
                    DataProviderId = _header.DataProvider.DataProviderId,
                    ScheduledTaskId = _header.ScheduledTaskId,
                    ScheduledTaskQueryId = detail.ScheduledTaskQueryId,
                    QueryToRun = detail.Query,
                    QueryInfo = detail.QueryName,
                    PaginatedResult = true,
                    LastRunDateTime = detail.LastRunDateTime ?? DateTime.Now,
                    QueryCommandParameters = param,
                    MaxSize = maxRows
                }, _jobParameters.Cts, _taskId);
            _logTask.HasSteps = true;

            if (detailParam!.GenerateIfEmpty || table.Rows.Count > 0) _fetchedData.Add(detail, table);

            if (_jobParameters.GenerateFiles)
            {
                detail.LastRunDateTime = DateTime.Now;
                _context.Entry(detail).State = EntityState.Modified;
            }
        }
    }
}