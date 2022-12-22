namespace Report_App_WASM.Shared.SerializedParameters
{
    public class DataTransferParameters
    {
        public string? DataTransferTargetTableName { get; set; }
        public bool DataTransferCreateTable { get; set; } = false;
        public bool DataTransferUsePK { get; set; } = false;
        public string? DataTransferCommandBehaviour { get; set; }
        public List<string> DataTransferPK { get; set; } = new();
    }
}
