using System.Globalization;
using System.Text;

namespace Report_App_WASM.Shared.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveSpecialCharacters(this string? str)
        {
            StringBuilder sb = new();

            foreach (var c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == 'é' || c == 'è' || c == 'à' || c == 'ù')
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static string RemoveSpecialExceptSpaceCharacters(this string? str)
        {
            StringBuilder sb = new();

            foreach (var c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == 'é' || c == 'è' || c == 'à' || c == 'ù' || c == ' ' || c == '-')
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static string RemoveDigits(this string str)
        {
            StringBuilder sb = new();
            foreach (var c in str)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string RemoveDiacritics(this String s)
        {
            String normalizedString = s.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                Char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }
    }

}
