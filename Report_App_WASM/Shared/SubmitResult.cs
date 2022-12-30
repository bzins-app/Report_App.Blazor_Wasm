namespace Report_App_WASM.Shared
{
    public class SubmitResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Value { get; set; }
    }

    public class SubmitResultRemoteData
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<Dictionary<string, object>>? Value { get; set; }
    }
}
