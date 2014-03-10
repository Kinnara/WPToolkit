using System.Globalization;

namespace Microsoft.Phone.Controls
{
    internal static class DateTimePickerHelper
    {
        /// <summary>
        /// Date should flow from right to left for arabic and persian.
        /// </summary>
        internal static bool DateShouldFlowRTL()
        {
            string lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            return lang == "ar" || lang == "fa";
        }

        /// <summary>
        /// Returns true if the current language is RTL.
        /// </summary>
        internal static bool IsRTLLanguage()
        {
            // Currently supported RTL languages are arabic, hebrew and persian.
            string lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            return lang == "ar" || lang == "he" || lang == "fa";
        }
    }
}
