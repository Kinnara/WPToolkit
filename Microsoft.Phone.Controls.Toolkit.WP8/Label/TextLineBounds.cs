using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Influences how a line box height is calculated
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
    public enum TextLineBounds
    {
        /// <summary>
        /// Uses normal line box height calculation, this is the default.
        /// </summary>
        Full,

        /// <summary>
        /// Top of line box height is the cap height from the font.
        /// </summary>
        TrimToCapHeight,

        /// <summary>
        /// Bottom of line box height is the text baseline.
        /// </summary>
        TrimToBaseline,

        /// <summary>
        /// Top of line box height is the cap height from the font, bottom of line box
        /// height is the text baseline.
        /// </summary>
        Tight,
    }
}
