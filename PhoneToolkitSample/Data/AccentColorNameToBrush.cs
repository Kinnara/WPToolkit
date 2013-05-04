// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PhoneToolkitSample.Data
{
    /// <summary>
    /// A converter that takes a name of an accent color and returns a SolidColorBrush.
    /// </summary>
    public class AccentColorNameToBrush : IValueConverter
    {
        private static Dictionary<string, SolidColorBrush> ColorNameToBrush = new Dictionary<string, SolidColorBrush>()
        {
            { "lime",    0xFFA4C400.ToSolidColorBrush() },
            { "green",   0xFF60A917.ToSolidColorBrush() },
            { "emerald", 0xFF008A00.ToSolidColorBrush() },
            { "teal",    0xFF00ABA9.ToSolidColorBrush() },
            { "cyan",    0xFF1BA1E2.ToSolidColorBrush() },
            { "cobalt",  0xFF0050EF.ToSolidColorBrush() },
            { "indigo",  0xFF6A00FF.ToSolidColorBrush() },
            { "violet",  0xFFAA00FF.ToSolidColorBrush() },
            { "pink",    0xFFF472D0.ToSolidColorBrush() },
            { "magenta", 0xFFD80073.ToSolidColorBrush() },
            { "crimson", 0xFFA20025.ToSolidColorBrush() },
            { "red",     0xFFE51400.ToSolidColorBrush() },
            { "orange",  0xFFFA6800.ToSolidColorBrush() },
            { "amber",   0xFFF0A30A.ToSolidColorBrush() },
            { "yellow",  0xFFD8C100.ToSolidColorBrush() },
            { "brown",   0xFF825A2C.ToSolidColorBrush() },
            { "olive",   0xFF6D8764.ToSolidColorBrush() },
            { "steel",   0xFF647687.ToSolidColorBrush() },
            { "mauve",   0xFF76608A.ToSolidColorBrush() },
            { "taupe",   0xFF7A3B3F.ToSolidColorBrush() },
        };

        /// <summary>
        /// Converts a name of an accent color to a SolidColorBrush.
        /// </summary>
        /// <param name="value">The accent color as a string.</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">The parameter</param>
        /// <param name="culture">The culture</param>
        /// <returns>A SolidColorBrush representing the accent color.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "By design")]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string v = value as string;
            if (null == v)
            {
                throw new ArgumentNullException("value");
            }

            SolidColorBrush brush = null;
            if (ColorNameToBrush.TryGetValue(v.ToLowerInvariant(), out brush))
            {
                return brush;
            }

            return null;
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}