using Report_App_BlazorServ.Utils;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.SerializedParameters;
using System.Collections.Generic;
using System.Threading;

namespace ReportAppWASM.Server.Services.BackgroundWorker
{
    public class TaskJobParameters
    {
        public int TaskHeaderId { get; set; }
        public CancellationToken Cts { get; set; }
        public List<EmailRecipient> CustomEmails { get; set; } = null;
        public List<QueryCommandParameter> CustomQueryParameters { get; set; } = new();
        public bool GenerateFiles { get; set; } = false;
        public bool ManualRun { get; set; } = false;
        public string RunBy { get; set; } = "system";
    }
}
