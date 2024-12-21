namespace Report_App_WASM.Shared.SerializedParameters;

public class QueryCommandParameter
{
    public string DisplayName { get; set; } = string.Empty;
    public int DisplaySequence { get; set; } = 99;
    public string ParameterIdentifier { get; set; } = string.Empty;
    public QueryCommandParameterValueType ValueType { get; set; }
    public CalulatedDateOption DateOption { get; set; }
    public bool Required { get; set; } = true;
    public bool HideParameter { get; set; } = false;
    public string Value { get; set; } = string.Empty;
}