namespace Report_App_WASM.Shared
{
    public class UserInfo
    {
        public bool IsAuthenticated { get; set; }
        public string? UserName { get; set; }
        public List<ClaimsValue>? ExposedClaims { get; set; }
    }

    public class ClaimsValue
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
