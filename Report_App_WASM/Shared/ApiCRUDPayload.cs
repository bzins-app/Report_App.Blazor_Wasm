namespace Report_App_WASM.Shared
{
    public class ApiCRUDPayload<T> where T : class
    {
        public string UserName { get; set; } = "system";
        public T EntityValue { get; set; }
    }

    public class ApiBackgrounWorkerdPayload
    {
        public bool Activate { get; set; } 
        public int Value { get; set; }
    }
}
