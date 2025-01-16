namespace Report_App_WASM.Shared.DatabasesConnectionParameters
{
    public class DatabaseTaskRetryPattern
    {
        public List<RetryOptions> Pattern { get; set; } =
        [
            new() { RetryAttempt = 1, DelayBetweenRetriesInSeconds = 10 },
            new() { RetryAttempt = 2, DelayBetweenRetriesInSeconds = 60 },
            new() { RetryAttempt = 3, DelayBetweenRetriesInSeconds = 300 }
        ];
    }

    public class RetryOptions
    {
        public int RetryAttempt { get; set; }
        public int DelayBetweenRetriesInSeconds { get; set; }
    }
}