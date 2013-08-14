namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Defines the physical characteristics of a font.
    /// </summary>
    public sealed class FontMetrics
    {
        /// <summary>
        /// Gets or sets the height of the character cell relative to the em size.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.Double"/> value that represents the height of the character cell.
        /// </returns>
        public double Height { get; set; }

        /// <summary>
        /// Gets or sets the baseline value for the <see cref="T:Microsoft.Phone.Controls.FontMetrics"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A value of type <see cref="T:System.Double"/> that represents the baseline.
        /// </returns>
        public double Baseline { get; set; }

        /// <summary>
        /// Gets or sets the distance from the baseline to the top of an English capital, relative to em size, for the <see cref="T:Microsoft.Phone.Controls.FontMetrics"/> object.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Double"/> that indicates the distance from the baseline to the top of an English capital letter, expressed as a fraction of the font em size.
        /// </returns>
        public double CapsHeight { get; set; }
    }
}
