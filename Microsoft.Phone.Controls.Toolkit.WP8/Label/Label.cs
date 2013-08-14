using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides a control for displaying small amounts of text.
    /// </summary>
    [ContentProperty("Inlines")]
    [TemplatePart(Name = ElementTextBlockHostName, Type = typeof(Border))]
    public class Label : Control
    {
        private const string ElementTextBlockHostName = "TextBlockHost";

        private const double FontSmallSize = 13.333;
        private const double FontMediumSize = 20;
        private const double FontLargeSize = 22.667;
        private const double FontExtraLargeSize = 32;
        private const double FontExtraLargePlusSize = 37.333;
        private const double FontXXLargeSize = 42.667;
        private const double ListGroupHeaderFontSize = 29.333;

        private static FontMetrics DefaultFontMetrics = new FontMetrics
        {
            Height = 1.330078125,
            Baseline = 1.0791015625,
            CapsHeight = 0.7001953125
        };

        private static IDictionary<double, double> DefaultTopMargins = new Dictionary<double, double>
        {
            { FontSmallSize, -5 },
            { FontMediumSize, -8 },
            { FontLargeSize, -9 },
            { FontExtraLargeSize, -12 },
            { FontExtraLargePlusSize, -14 },
            { FontXXLargeSize, -16 },
            { ListGroupHeaderFontSize, -11 },
        };

        private static IDictionary<double, double> DefaultBottomMargins = new Dictionary<double, double>
        {
            { FontSmallSize, -3 },
            { FontMediumSize, -4 },
            { FontLargeSize, -6 },
            { FontExtraLargeSize, -8 },
            { FontExtraLargePlusSize, -9 },
            { FontXXLargeSize, -11 },
            { ListGroupHeaderFontSize, -8 },
        };

        /// <summary>
        /// A value indicating whether a dependency property change handler
        /// should ignore the next change notification.  This is used to reset
        /// the value of properties without performing any of the actions in
        /// their change handlers.
        /// </summary>
        private bool _ignorePropertyChange;

        private TextBlock _textBlock;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Microsoft.Phone.Controls.Label" /> class.
        /// </summary>
        public Label()
        {
            DefaultStyleKey = typeof(Label);

            _textBlock = new TextBlock
            {
                IsHitTestVisible = false
            };
        }

        #region TextDecorations

        /// <summary>
        /// Gets or sets a value that specifies the text decorations that are applied to the content in a <see cref="T:Microsoft.Phone.Controls.Label"/> element.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Windows.TextDecorationCollection"/>, or null if no text decorations are applied.
        /// </returns>
        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
            set { SetValue(TextDecorationsProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Label.TextDecorations"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Label.TextDecorations"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register(
            "TextDecorations",
            typeof(TextDecorationCollection),
            typeof(Label),
            new PropertyMetadata(GetDefaultValue(TextBlock.TextDecorationsProperty, typeof(TextBlock)), (d, e) => ((Label)d).OnTextDecorationsChanged()));

        private void OnTextDecorationsChanged()
        {
            _textBlock.TextDecorations = TextDecorations;
        }

        #endregion

        #region Inlines

        /// <summary>
        /// Gets the collection of inline text elements within a <see cref="T:Microsoft.Phone.Controls.Label"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A collection that holds all inline text elements from the <see cref="T:Microsoft.Phone.Controls.Label"/>.The default is an empty collection.
        /// </returns>
        public InlineCollection Inlines
        {
            get { return _textBlock.Inlines; }
        }

        #endregion

        #region FontSource

        /// <summary>
        /// Gets or sets the font source that is applied to the text for rendering content.
        /// </summary>
        /// 
        /// <returns>
        /// The font source that is used to render content in the text box.  The default is null.
        /// </returns>
        public FontSource FontSource
        {
            get { return _textBlock.FontSource; }
            set { _textBlock.FontSource = value; }
        }

        #endregion

        #region BaselineOffset

        /// <summary>
        /// Returns a value by which each line of text is offset from a baseline.
        /// </summary>
        /// 
        /// <returns>
        /// The amount by which each line of text is offset from the baseline, in device independent pixels. . <see cref="F:System.Double.NaN"/> indicates that an optimal baseline offset is automatically calculated from the current font characteristics. The default is <see cref="F:System.Double.NaN"/>.
        /// </returns>
        public double BaselineOffset
        {
            get { return _textBlock.BaselineOffset; }
        }

        #endregion

        #region TextWrapping

        /// <summary>
        /// Gets or sets how the <see cref="T:Microsoft.Phone.Controls.Label"/> wraps text.
        /// </summary>
        /// 
        /// <returns>
        /// A value that indicates how the <see cref="T:Microsoft.Phone.Controls.Label"/> wraps text. The default is <see cref="F:System.Windows.TextWrapping.NoWrap"/>.
        /// </returns>
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Label.TextWrapping"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier of the <see cref="P:Microsoft.Phone.Controls.Label.TextWrapping"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
            "TextWrapping",
            typeof(TextWrapping),
            typeof(Label),
            new PropertyMetadata(GetDefaultValue(TextBlock.TextWrappingProperty, typeof(TextBlock)), (d, e) => ((Label)d).OnTextWrappingChanged()));

        private void OnTextWrappingChanged()
        {
            _textBlock.TextWrapping = TextWrapping;
        }

        #endregion

        #region TextTrimming

        /// <summary>
        /// Gets or sets the text trimming behavior to employ when content overflows the content area.
        /// </summary>
        /// 
        /// <returns>
        /// One of the <see cref="T:System.Windows.TextTrimming"/> values that specifies the text trimming behavior to employ. The default is <see cref="F:System.Windows.TextTrimming.None"/>.
        /// </returns>
        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Label.TextTrimming"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier of the <see cref="P:Microsoft.Phone.Controls.Label.TextTrimming"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
            "TextTrimming",
            typeof(TextTrimming),
            typeof(Label),
            new PropertyMetadata(GetDefaultValue(TextBlock.TextTrimmingProperty, typeof(TextBlock)), (d, e) => ((Label)d).OnTextTrimmingChanged()));

        private void OnTextTrimmingChanged()
        {
            _textBlock.TextTrimming = TextTrimming;
        }

        #endregion

        #region TextAlignment

        /// <summary>
        /// Gets or sets a value that indicates the horizontal alignment of text content.
        /// </summary>
        /// 
        /// <returns>
        /// The text alignment. The default is <see cref="F:System.Windows.TextAlignment.Left"/>.
        /// </returns>
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Label.TextAlignment"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Label.TextAlignment"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
            "TextAlignment",
            typeof(TextAlignment),
            typeof(Label),
            new PropertyMetadata(GetDefaultValue(TextBlock.TextAlignmentProperty, typeof(TextBlock)), (d, e) => ((Label)d).OnTextAlignmentChanged()));

        private void OnTextAlignmentChanged()
        {
            _textBlock.TextAlignment = TextAlignment;
        }

        #endregion

        #region Text

        /// <summary>
        /// Gets or sets the text contents of a <see cref="T:Microsoft.Phone.Controls.Label"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A string that specifies the text contents of this <see cref="T:Microsoft.Phone.Controls.Label"/>. The default is an empty string.
        /// </returns>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Label.Text"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier of the <see cref="P:Microsoft.Phone.Controls.Label.Text"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(Label),
            new PropertyMetadata(GetDefaultValue(TextBlock.TextProperty, typeof(TextBlock)), (d, e) => ((Label)d).OnTextChanged()));

        private void OnTextChanged()
        {
            _textBlock.Text = Text;
        }

        #endregion

        #region LineHeight

        /// <summary>
        /// Gets or sets the height of each line of content.
        /// </summary>
        /// 
        /// <returns>
        /// The height of each line in pixels. A value of 0 indicates that the line height is determined automatically from the current font characteristics. The default is 0.
        /// </returns>
        /// <exception cref="T:System.ArgumentException"><see cref="P:Microsoft.Phone.Controls.Label.LineHeight"/> is set to a non-positive value.</exception>
        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Label.LineHeight"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Label.LineHeight"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register(
            "LineHeight",
            typeof(double),
            typeof(Label),
            new PropertyMetadata(GetDefaultValue(TextBlock.LineHeightProperty, typeof(TextBlock)), (d, e) => ((Label)d).OnLineHeightChanged()));

        private void OnLineHeightChanged()
        {
            _textBlock.LineHeight = LineHeight;
        }

        #endregion

        #region LineStackingStrategy

        /// <summary>
        /// Gets or sets a value that indicates how a line box is determined for each line of text in the <see cref="T:Microsoft.Phone.Controls.Label"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A value that indicates how a line box is determined for each line of text in the <see cref="T:Microsoft.Phone.Controls.Label"/>. The default is <see cref="F:System.Windows.LineStackingStrategy.MaxHeight"/>.
        /// </returns>
        public LineStackingStrategy LineStackingStrategy
        {
            get { return (LineStackingStrategy)GetValue(LineStackingStrategyProperty); }
            set { SetValue(LineStackingStrategyProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Label.LineStackingStrategy"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Label.LineStackingStrategy"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty LineStackingStrategyProperty = DependencyProperty.Register(
            "LineStackingStrategy",
            typeof(LineStackingStrategy),
            typeof(Label),
            new PropertyMetadata(GetDefaultValue(TextBlock.LineStackingStrategyProperty, typeof(TextBlock)), (d, e) => ((Label)d).OnLineStackingStrategyChanged()));

        private void OnLineStackingStrategyChanged()
        {
            _textBlock.LineStackingStrategy = LineStackingStrategy;
        }

        #endregion

        #region TextLineBounds

        /// <summary>
        /// Gets or sets a value that indicates how the line box height is determined for each line of text in the <see cref="T:Microsoft.Phone.Controls.Label"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A value that indicates how the line box height is determined for each line of text in the <see cref="T:Microsoft.Phone.Controls.Label"/>. The default is Full.
        /// </returns>
        public TextLineBounds TextLineBounds
        {
            get { return (TextLineBounds)GetValue(TextLineBoundsProperty); }
            set { SetValue(TextLineBoundsProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Label.TextLineBounds"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Label.TextLineBounds"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty TextLineBoundsProperty = DependencyProperty.Register(
            "TextLineBounds",
            typeof(TextLineBounds),
            typeof(Label),
            new PropertyMetadata(TextLineBounds.Full, (d, e) => ((Label)d).OnTextLineBoundsChanged()));

        private void OnTextLineBoundsChanged()
        {
            InvalidateMeasure();
        }

        private void ApplyTextLineBounds()
        {
            TextLineBounds textLineBounds = TextLineBounds;

            if (textLineBounds == TextLineBounds.Full)
            {
                _textBlock.ClearValue(FrameworkElement.MarginProperty);
                _textBlock.ClearValue(UIElement.RenderTransformProperty);
            }
            else
            {
                double fontSize = _textBlock.FontSize;
                FontMetrics fontMetrics = FontMetrics ?? DefaultFontMetrics;

                if (_textBlock.LineStackingStrategy == LineStackingStrategy.BlockLineHeight)
                {
                    if (textLineBounds == TextLineBounds.Tight)
                    {
                        textLineBounds = TextLineBounds.TrimToBaseline;
                    }

                    _textBlock.ClearValue(FrameworkElement.MarginProperty);

                    double translateY = CalculateTranslateY(textLineBounds, fontSize, _textBlock.LineHeight, fontMetrics);
                    TextBlockTransform.Y = translateY;
                    _textBlock.RenderTransform = TextBlockTransform;
                }
                else
                {
                    double top = 0, bottom = 0;

                    switch (TextLineBounds)
                    {
                        case TextLineBounds.TrimToCapHeight:
                            top = CalculateTopMargin(fontSize, fontMetrics);
                            break;
                        case TextLineBounds.TrimToBaseline:
                            bottom = CalculateBottomMargin(fontSize, fontMetrics);
                            break;
                        case TextLineBounds.Tight:
                            top = CalculateTopMargin(fontSize, fontMetrics);
                            bottom = CalculateBottomMargin(fontSize, fontMetrics);
                            break;
                    }

                    _textBlock.Margin = new Thickness(0, top, 0, bottom);
                    _textBlock.ClearValue(UIElement.RenderTransformProperty);
                }
            }
        }

        #endregion

        #region MaxLines

        /// <summary>
        /// Gets or sets the maximum lines of text shown in the <see cref="T:Microsoft.Phone.Controls.Label"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The maximum lines of text shown in the <see cref="T:Microsoft.Phone.Controls.Label"/>. The default is 0, which is a special value that represents Auto behavior. The value cannot be negative.
        /// </returns>
        public int MaxLines
        {
            get { return (int)GetValue(MaxLinesProperty); }
            set { SetValue(MaxLinesProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Label.MaxLines"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Label.MaxLines"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.Register(
            "MaxLines",
            typeof(int),
            typeof(Label),
            new PropertyMetadata((d, e) => ((Label)d).OnMaxLinesChanged(e)));

        private void OnMaxLinesChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_ignorePropertyChange)
            {
                _ignorePropertyChange = false;
                return;
            }

            if ((int)e.NewValue < 0)
            {
                _ignorePropertyChange = true;
                MaxLines = (int)e.OldValue;
                throw new ArgumentOutOfRangeException("MaxLines");
            }

            InvalidateMeasure();
        }

        private void ApplyMaxLines()
        {
            if (MaxLines > 0)
            {
                TextBlock measureTextBlock = new TextBlock
                {
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    FontSource = FontSource,
                    LineHeight = LineHeight,
                    LineStackingStrategy = LineStackingStrategy,
                    Padding = Padding,
                    Text = "Xg"
                };

                for (int i = 1; i < MaxLines; i++)
                {
                    measureTextBlock.Text += Environment.NewLine;
                }

                measureTextBlock.UpdateLayout();

                _textBlock.MaxHeight = Math.Ceiling(measureTextBlock.ActualHeight);
            }
            else
            {
                _textBlock.ClearValue(FrameworkElement.MaxHeightProperty);
            }
        }

        #endregion

        #region FontMetrics

        /// <summary>
        /// Gets or sets the font metrics object that is used for layout.
        /// </summary>
        /// 
        /// <returns>
        /// The font metrics object that is used for layout. The default is null.
        /// </returns>
        public FontMetrics FontMetrics
        {
            get { return (FontMetrics)GetValue(FontMetricsProperty); }
            set { SetValue(FontMetricsProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Label.FontMetrics"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Label.FontMetrics"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty FontMetricsProperty = DependencyProperty.Register(
            "FontMetrics",
            typeof(FontMetrics),
            typeof(Label),
            new PropertyMetadata((d, e) => ((Label)d).OnFontMetricsChanged(e)));

        private void OnFontMetricsChanged(DependencyPropertyChangedEventArgs e)
        {
            InvalidateMeasure();
        }

        #endregion

        private Border ElementTextBlockHost { get; set; }

        private TranslateTransform _textBlockTransform;
        private TranslateTransform TextBlockTransform
        {
            get { return _textBlockTransform ?? (_textBlockTransform = new TranslateTransform()); }
        }

        /// <summary>
        /// Builds the visual tree for the
        /// <see cref="T:Microsoft.Phone.Controls.Label" /> control
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ElementTextBlockHost != null)
            {
                ElementTextBlockHost.Child = null;
            }

            ElementTextBlockHost = GetTemplateChild(ElementTextBlockHostName) as Border;

            if (ElementTextBlockHost != null)
            {
                ElementTextBlockHost.Child = _textBlock;
            }
        }

        /// <summary>
        /// Handles the measure pass for the control.
        /// </summary>
        /// 
        /// <returns>
        /// The desired size.
        /// </returns>
        /// <param name="availableSize">The available size.</param>
        protected override Size MeasureOverride(Size availableSize)
        {
            ApplyTextLineBounds();
            ApplyMaxLines();

            return base.MeasureOverride(availableSize);
        }

        private static double CalculateTopMargin(double fontSize, FontMetrics fontMetrics)
        {
            if (fontMetrics == DefaultFontMetrics)
            {
                double top;
                if (DefaultTopMargins.TryGetValue(Math.Round(fontSize, 3), out top))
                {
                    return top;
                }
            }

            return ((fontSize * (fontMetrics.CapsHeight - fontMetrics.Baseline)));
        }

        private static double CalculateBottomMargin(double fontSize, FontMetrics fontMetrics)
        {
            if (fontMetrics == DefaultFontMetrics)
            {
                double bottom;
                if (DefaultBottomMargins.TryGetValue(Math.Round(fontSize, 3), out bottom))
                {
                    return bottom;
                }
            }

            return ((fontSize * (fontMetrics.Baseline - fontMetrics.Height)));
        }

        private static double CalculateTranslateY(TextLineBounds textLineBounds, double fontSize, double lineHeight, FontMetrics fontMetrics)
        {
            if (textLineBounds == TextLineBounds.TrimToCapHeight)
            {
                double defaultLineHeight = fontSize * fontMetrics.Height;
                double lineHeightDiff = lineHeight - defaultLineHeight;
                double topDiff = lineHeightDiff * fontMetrics.Baseline / fontMetrics.Height;
                double bottomDiff = lineHeightDiff - topDiff;
                return fontSize * (fontMetrics.CapsHeight - fontMetrics.Baseline) - topDiff;
            }
            else if (textLineBounds == TextLineBounds.TrimToBaseline)
            {
                double defaultLineHeight = fontSize * fontMetrics.Height;
                double lineHeightDiff = lineHeight - defaultLineHeight;
                double topDiff = lineHeightDiff * fontMetrics.Baseline / fontMetrics.Height;
                double bottomDiff = lineHeightDiff - topDiff;
                return fontSize * (fontMetrics.Height - fontMetrics.Baseline) + bottomDiff;
            }
            else
            {
                throw new ArgumentOutOfRangeException("textLineBounds");
            }
        }

        private static object GetDefaultValue(DependencyProperty dp, Type forType)
        {
            return dp.GetMetadata(forType).DefaultValue;
        }
    }
}
