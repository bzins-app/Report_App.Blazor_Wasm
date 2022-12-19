namespace Report_App_WASM.Shared
{
    public class SelectItemActivitiesInfo
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string LogoPath { get; set; }
        public bool HasALogo { get; set; }
        public bool IsVisible { get; set; }
        public bool IsActivated { get; set; }
    }
}
