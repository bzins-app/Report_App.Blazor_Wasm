using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared.ApiExchanges
{
    public class UserConfigurationSave
    {
        public string? SaveName { get; set; }
        public string? Parameters { get; set; }
        public TypeConfiguration TypeConfiguration { get; set; }
        public int IdIntConfiguration { get; set; }
        public string? IdStringConfiguration { get; set; }
        public string? SavedValues { get; set; }
    }

    public class UserConfigurations
    {
        public int Id { get; set; }
        public string? SaveName { get; set; }
        public string? Parameters { get; set; }
        public TypeConfiguration TypeConfiguration { get; set; }
        public int IdIntConfiguration { get; set; }
        public string? IdStringConfiguration { get; set; }
        public string? SavedValues { get; set; }
    }

    public class UserConfigurationDelete
    {
        public int Id { get; set; }
    }

    public class UserConfigurationGetById
    {
        public int IdIntConfiguration { get; set; }
    }
}
