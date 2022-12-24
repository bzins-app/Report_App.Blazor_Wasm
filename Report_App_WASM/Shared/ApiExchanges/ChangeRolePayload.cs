namespace Report_App_WASM.Shared.ApiExchanges
{
    public class ChangeRolePayload
    {
        public string UserName { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }

    public class UserPayload
    {
        public string UserName { get; set; }
        public string? UserMail { get; set; }
        public string Password { get; set; }
    }
}
