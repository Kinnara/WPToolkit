// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace System.Windows.Controls
{
    /// <summary>
    /// Common TypeConverter functionality.
    /// </summary>
    internal static partial class TypeConverters
    {
        /// <summary>
        /// Determines whether conversion is possible to a specified type.
        /// </summary>
        /// <typeparam name="T">Expected type of the converter.</typeparam>
        /// <param name="destinationType">
        /// Identifies the data type to evaluate for conversion.
        /// </param>
        /// <returns>
        /// A value indicating whether conversion is possible.
        /// </returns>
        internal static bool CanConvertTo<T>(Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            return (destinationType == typeof(string)) ||
                destinationType.IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Attempts to convert a specified object to an instance of the
        /// desired type.
        /// </summary>
        /// <param name="converter">TypeConverter instance.</param>
        /// <param name="value">The object being converted.</param>
        /// <param name="destinationType">
        /// The type to convert the value to.
        /// </param>
        /// <returns>
        /// The value of the conversion to the specified type.
        /// </returns>
        internal static object ConvertTo(TypeConverter converter, object value, Type destinationType)
        {
            Debug.Assert(converter != null, "converter should not be null!");

            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            // Just return the value if it is already an instance of the
            // destination type
            if (value == null && !destinationType.IsValueType)
            {
                return null;
            }
            else if (value != null && destinationType.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            // Otherwise throw an error
            throw new NotSupportedException(string.Format(
                CultureInfo.CurrentCulture,
                Microsoft.Phone.Controls.Properties.
                    Resources.TypeConverters_Convert_CannotConvert,
                converter.GetType().Name,
                value != null ? value.GetType().FullName : "(null)",
                destinationType.GetType().Name));
        }
    }
}