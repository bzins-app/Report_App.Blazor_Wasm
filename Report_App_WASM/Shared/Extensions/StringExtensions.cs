namespace Report_App_WASM.Shared.Extensions;

public static class StringExtensions
{
    public static string RemoveSpecialCharacters(this string? str)
    {
        StringBuilder sb = new();

        foreach (var c in str)
            if (c is >= '0' and <= '9' || c is >= 'A' and <= 'Z' || c is >= 'a' and <= 'z' || c == '.' || c == '_' ||
                c == 'é' || c == 'è' || c == 'à' || c == 'ù')
                sb.Append(c);

        return sb.ToString();
    }

    public static string RemoveSpecialExceptSpaceCharacters(this string? str)
    {
        StringBuilder sb = new();

        foreach (var c in str)
            if (c is >= '0' and <= '9' || c is >= 'A' and <= 'Z' || c is >= 'a' and <= 'z' || c == '.' || c == '_' ||
                c == 'é' || c == 'è' || c == 'à' || c == 'ù' || c == ' ' || c == '-')
                sb.Append(c);

        return sb.ToString();
    }

    public static string RemoveDigits(this string str)
    {
        StringBuilder sb = new();
        foreach (var c in str)
            if (c is >= 'A' and <= 'Z' || c is >= 'a' and <= 'z' || c == '.' || c == '_')
                sb.Append(c);
        return sb.ToString();
    }

    public static string RemoveDiacritics(this string s)
    {
        var normalizedString = s.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        for (var i = 0; i < normalizedString.Length; i++)
        {
            var c = normalizedString[i];
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString();
    }
}