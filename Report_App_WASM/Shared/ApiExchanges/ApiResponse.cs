using System.Text.Json.Serialization;

namespace Report_App_WASM.Shared.ApiExchanges
{
    public class ApiResponse<T> where T : class
    {
        [JsonPropertyName("@odata.count")]
        public int Count { get; set; }
        [JsonPropertyName("value")]
        public List<T>? Value { get; set; }
    }
}
