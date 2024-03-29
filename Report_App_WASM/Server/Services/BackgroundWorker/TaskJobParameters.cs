﻿namespace Report_App_WASM.Server.Services.BackgroundWorker;

public class TaskJobParameters
{
    public int TaskHeaderId { get; init; }
    public CancellationToken Cts { get; set; }
    public List<EmailRecipient>? CustomEmails { get; init; } = null;
    public List<QueryCommandParameter>? CustomQueryParameters { get; init; } = new();
    public bool GenerateFiles { get; init; } = false;
    public bool ManualRun { get; init; } = false;
    public string? RunBy { get; init; } = "system";
}