using BlazorDownloadFile;
using static System.Net.WebRequestMethods;
using System;
using System.Net.Http.Json;
using Report_App_WASM.Shared.ApiResponse;

namespace Report_App_WASM.Client.Services
{
    public class DataEntityService
    {
        private readonly HttpClient _httpClient;
        private readonly IBlazorDownloadFileService BlazorDownloadFileService;

        public DataEntityService(HttpClient httpClient, IBlazorDownloadFileService blazorDownloadFileService)
        {
            _httpClient = httpClient;
            BlazorDownloadFileService = blazorDownloadFileService;
        }
        public async Task ExtractGridLogs(ODataExtractPayload Values)
        {
            try
            {
                string url = "odata/ExtractLogs";
                var response = await _httpClient.PostAsJsonAsync(url, Values);
                if(response.IsSuccessStatusCode)
                {
                    var downloadresult = await BlazorDownloadFileService.DownloadFile(Values.FileName + " " + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".xlsx", await response.Content.ReadAsByteArrayAsync(), contentType: "application/octet-stream");
                    if (downloadresult.Succeeded)
                    {
                        response.Dispose();
                    }
                }
            }
            catch
            {
                // Unfortunately this HTTP API returns a 404 if there were no results, so we have to handle that separately
                //return null;
            }
        }

    }
}
