namespace Report_App_WASM.Shared.SerializedParameters
{
    public class QueryCommandParameter
    {
        public string? ParameterIdentifier { get; set; }
        public QueryCommandParameterValueType ValueType { get; set; }
        public CalulatedDateOption DateOption { get; set; }
        public bool Required { get; set; } = true;
        public string? value { get; set; }
    }


    public enum QueryCommandParameterValueType
    {
        String,
        Number,
        Date,
        DateTime
    }

    public enum CalulatedDateOption
    {
        LastRun,
        LastOpeningDay,
        Now,
        Yesterday,
        Tomorrow,
        StartOfThisWeek,
        EndOfThisWeek,
        StartOfLastWeek,
        EndOfLastWeek,
        StartOfThisMonth,
        EndOfThisMonth,
        StartOfLastMonth,
        EndOfLastMonth,
        StartOfThisQuarter,
        EndOfThisQuarter,
        StartOfThisYear,
        EndOfThisYear,
    }
}
