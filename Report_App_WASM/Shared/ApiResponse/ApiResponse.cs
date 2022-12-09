using Report_App_WASM.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared
{
    public class ApiResponse<T> where T:class
    {
        [JsonPropertyName("@odata.count")]
        public int Count { get; set; }
        [JsonPropertyName("value")]
        public List<T> Value { get; set; }
    }
}
