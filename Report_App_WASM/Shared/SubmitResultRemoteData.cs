﻿namespace Report_App_WASM.Shared;

public class SubmitResultRemoteData
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int TotalElements { get; set; }
    public List<Dictionary<string, object>>? Value { get; set; }
}