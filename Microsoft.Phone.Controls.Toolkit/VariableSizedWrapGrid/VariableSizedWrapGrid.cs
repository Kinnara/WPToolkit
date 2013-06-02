using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides a grid-style layout panel where each tile/cell can be variable size based on content.
    /// </summary>
    public sealed class VariableSizedWrapGrid : Panel
    {
        /// <summary>
        /// A value indicating whether a dependency property change handler
        /// should ignore the next change notification.  This is used to reset
        /// the value of properties without performing any of the actions in
        /// their change handlers.
        /// </summary>
        private bool _ignorePropertyChange;

        private Dictionary<UIElement, Rect> _arrangeInfo = new Dictionary<UIElement, Rect>();

        /// <summary>
        /// Initializes a new instance of the VariableSizedWrapGrid class.
        /// </summary>
        public VariableSizedWrapGrid()
        {
        }

        #region ItemWidth

        /// <summary>
        /// Gets or sets the width of the layout area for each item that is
        /// contained in a VariableSizedWrapGrid.
        /// </summary>
        /// 
        /// <returns>
        /// The width of the layout area for each item that is
        /// contained in a VariableSizedWrapGrid. The default is Double.NaN.
        /// </returns>
        [TypeConverter(typeof(LengthConverter))]
        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemWidth dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the ItemWidth dependency property.
        /// </returns>
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
            "ItemWidth",
            typeof(double),
            typeof(VariableSizedWrapGrid),
            new PropertyMetadata(double.NaN, (d, e) => ((VariableSizedWrapGrid)d).OnItemWidthOrHeightChanged(e)));

        #endregion

        #region ItemHeight

        /// <summary>
        /// Gets or sets the height of the layout area for each item that is
        /// contained in a VariableSizedWrapGrid.
        /// </summary>
        /// 
        /// <returns>
        /// The height of the layout area for each item that is
        /// contained in a VariableSizedWrapGrid. The default is Double.NaN.
        /// </returns>
        [TypeConverter(typeof(LengthConverter))]
        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemHeight dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the ItemHeight dependency property.
        /// </returns>
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            "ItemHeight",
            typeof(double),
            typeof(VariableSizedWrapGrid),
            new PropertyMetadata(double.NaN, (d, e) => ((VariableSizedWrapGrid)d).OnItemWidthOrHeightChanged(e)));

        #endregion

        #region MaximumRowsOrColumns

        /// <summary>
        /// Gets or sets a value that influences the wrap point, also accounting for Orientation.
        /// </summary>
        /// 
        /// <returns>
        /// The maximum rows or columns that this VariableSizedWrapGrid
        /// should present before it introduces wrapping to the layout. The default is -1, which is a special value that indicates no maximum.
        /// </returns>
        public int MaximumRowsOrColumns
        {
            get { return (int)GetValue(MaximumRowsOrColumnsProperty); }
            set { SetValue(MaximumRowsOrColumnsProperty, value); }
        }

        /// <summary>
        /// Identifies the MaximumRowsOrColumns dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the MaximumRowsOrColumns dependency property.
        /// </returns>
        public static readonly DependencyProperty MaximumRowsOrColumnsProperty = DependencyProperty.Register(
            "MaximumRowsOrColumns",
            typeof(int),
            typeof(VariableSizedWrapGrid),
            new PropertyMetadata(-1, (d, e) => ((VariableSizedWrapGrid)d).InvalidateMeasure()));

        private void OnMaximumRowsOrColumnsChanged()
        {
            InvalidateMeasure();
        }

        #endregion

        #region Orientation

        /// <summary>
        /// Gets or sets the direction in which child elements are arranged.
        /// </summary>
        /// 
        /// <returns>
        /// A value of the enumeration. The default is Horizontal.
        /// </returns>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the Orientation dependency property.
        /// </returns>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(VariableSizedWrapGrid),
            new PropertyMetadata(Orientation.Horizontal, (d, e) => ((VariableSizedWrapGrid)d).OnOrientationChanged(e)));

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "Almost always set from the CLR property.")]
        private void OnOrientationChanged(DependencyPropertyChangedEventArgs e)
        {
            Orientation value = (Orientation)e.NewValue;

            // Ignore the change if requested
            if (_ignorePropertyChange)
            {
                _ignorePropertyChange = false;
                return;
            }

            // Validate the Orientation
            if ((value != Orientation.Horizontal) &&
                (value != Orientation.Vertical))
            {
                // Reset the property to its original state before throwing
                _ignorePropertyChange = true;
                SetValue(OrientationProperty, (Orientation)e.OldValue);

                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.WrapPanel_OnOrientationPropertyChanged_InvalidValue,
                    value);
                throw new ArgumentException(message, "value");
            }

            // Orientation affects measuring.
            InvalidateMeasure();
        }

        #endregion

        #region RowSpan

        /// <summary>
        /// Gets the value of the VariableSizedWrapGrid.RowSpan XAML attached property from a target element.
        /// </summary>
        /// 
        /// <returns>
        /// The obtained value.
        /// </returns>
        /// <param name="element">The target element.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        public static int GetRowSpan(UIElement element)
        {
            return (int)element.GetValue(RowSpanProperty);
        }

        /// <summary>
        /// Sets the value of the VariableSizedWrapGrid.RowSpan XAML attached property on a target element.
        /// </summary>
        /// <param name="element">The target element.</param>
        /// <param name="value">The value to set.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        public static void SetRowSpan(UIElement element, int value)
        {
            element.SetValue(RowSpanProperty, value);
        }

        /// <summary>
        /// Identifies the VariableSizedWrapGrid.RowSpan XAML attached property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the VariableSizedWrapGrid.RowSpan XAML attached property.
        /// </returns>
        public static readonly DependencyProperty RowSpanProperty = DependencyProperty.RegisterAttached(
            "RowSpan",
            typeof(int),
            typeof(VariableSizedWrapGrid),
            new PropertyMetadata(1));

        #endregion

        #region ColumnSpan

        /// <summary>
        /// Gets the value of the VariableSizedWrapGrid.ColumnSpan XAML attached property from a target element.
        /// </summary>
        /// 
        /// <returns>
        /// The obtained value.
        /// </returns>
        /// <param name="element">The target element.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        public static int GetColumnSpan(UIElement element)
        {
            return (int)element.GetValue(ColumnSpanProperty);
        }

        /// <summary>
        /// Sets the value of the VariableSizedWrapGrid.ColumnSpan XAML attached property on a target element.
        /// </summary>
        /// <param name="element">The target element.</param>
        /// <param name="value">The value to set.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        public static void SetColumnSpan(UIElement element, int value)
        {
            element.SetValue(ColumnSpanProperty, value);
        }

        /// <summary>
        /// Identifies the VariableSizedWrapGrid.ColumnSpan XAML attached property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the VariableSizedWrapGrid.ColumnSpan XAML attached property.
        /// </returns>
        public static readonly DependencyProperty ColumnSpanProperty = DependencyProperty.RegisterAttached(
            "ColumnSpan",
            typeof(int),
            typeof(VariableSizedWrapGrid),
            new PropertyMetadata(1));

        #endregion

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
            double itemWidth = ItemWidth;
            double itemHeight = ItemHeight;
            int maximumRowsOrColumns = MaximumRowsOrColumns;
            Orientation orientation = Orientation;

            if (double.IsNaN(itemWidth) || double.IsNaN(itemHeight))
            {
                throw new InvalidOperationException(Properties.Resources.VariableSizedWrapGrid_MeasureOverride_ItemWidthAndItemHeightMustBeSet);
            }

            int rows = double.IsPositiveInfinity(availableSize.Height) ? int.MaxValue : (int)(availableSize.Height / itemHeight);
            int columns = double.IsPositiveInfinity(availableSize.Width) ? int.MaxValue : (int)(availableSize.Width / itemWidth);

            if (maximumRowsOrColumns > 0)
            {
                if (orientation == Orientation.Horizontal)
                {
                    if (columns > maximumRowsOrColumns)
                    {
                        columns = maximumRowsOrColumns;
                    }
                }
                else
                {
                    if (rows > maximumRowsOrColumns)
                    {
                        rows = maximumRowsOrColumns;
                    }
                }
            }

            ICollection<Point> filledCells = new List<Point>();
            bool noCellsAvailable = false;

            int x = 0;
            int y = 0;

            double desiredWidth = 0;
            double desiredHeight = 0;

            _arrangeInfo.Clear();

            foreach (UIElement child in Children)
            {
                int rowSpan = Math.Max(GetRowSpan(child), 1);
                int columnSpan = Math.Max(GetColumnSpan(child), 1);

                child.Measure(new Size(itemWidth * columnSpan, itemHeight * rowSpan));

                if (noCellsAvailable)
                {
                    continue;
                }

                if (rowSpan > rows)
                {
                    rowSpan = rows;
                }

                if (columnSpan > columns)
                {
                    columnSpan = columns;
                }

                if (!TryComputeCellPosition(rows, columns, rowSpan, columnSpan, filledCells, ref x, ref y))
                {
                    noCellsAvailable = true;
                    continue;
                }

                if (y + rowSpan > rows)
                {
                    rowSpan = rows - y;
                }

                if (x + columnSpan > columns)
                {
                    columnSpan = columns - x;
                }

                Rect childRect = new Rect(x * itemWidth, y * itemHeight, itemWidth * columnSpan, itemHeight * rowSpan);
                _arrangeInfo[child] = childRect;

                if (desiredWidth < childRect.Right)
                {
                    desiredWidth = childRect.Right;
                }

                if (desiredHeight < childRect.Bottom)
                {
                    desiredHeight = childRect.Bottom;
                }

                for (int row = y; row < y + rowSpan; row++)
                {
                    for (int column = x; column < x + columnSpan; column++)
                    {
                        filledCells.Add(new Point(column, row));
                    }
                }

                if (orientation == Orientation.Horizontal)
                {
                    x += columnSpan;
                }
                else
                {
                    y += rowSpan;
                }
            }

            return new Size(desiredWidth, desiredHeight);
        }

        /// <summary>
        /// Handles the arrange pass for the control.
        /// </summary>
        /// 
        /// <returns>
        /// The render size.
        /// </returns>
        /// <param name="finalSize">The final size.</param>
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (KeyValuePair<UIElement, Rect> info in _arrangeInfo)
            {
                info.Key.Arrange(info.Value);
            }

            _arrangeInfo.Clear();

            return finalSize;
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "Almost always set from the CLR property.")]
        private void OnItemWidthOrHeightChanged(DependencyPropertyChangedEventArgs e)
        {
            double value = (double)e.NewValue;

            // Ignore the change if requested
            if (_ignorePropertyChange)
            {
                _ignorePropertyChange = false;
                return;
            }

            // Validate the length (which must be a positive,
            // finite number)
            if (((value <= 0.0) || double.IsPositiveInfinity(value)))
            {
                // Reset the property to its original state before throwing
                _ignorePropertyChange = true;
                SetValue(e.Property, (double)e.OldValue);

                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.WrapPanel_OnItemHeightOrWidthPropertyChanged_InvalidValue,
                    value);
                throw new ArgumentException(message, "value");
            }

            // The length properties affect measuring.
            InvalidateMeasure();
        }

        private bool TryComputeCellPosition(int rows, int columns, int rowSpan, int columnSpan, ICollection<Point> filledCells, ref int x, ref int y)
        {
            Orientation orientation = Orientation;

            int direct;
            int indirect;
            int directSpan;
            int indirectSpan;
            int maxDirect;
            int maxIndirect;

            if (orientation == Orientation.Horizontal)
            {
                direct = x;
                indirect = y;
                directSpan = columnSpan;
                indirectSpan = rowSpan;
                maxDirect = columns;
                maxIndirect = rows;
            }
            else
            {
                direct = y;
                indirect = x;
                directSpan = rowSpan;
                indirectSpan = columnSpan;
                maxDirect = rows;
                maxIndirect = columns;
            }

            for (; indirect + indirectSpan <= maxIndirect; indirect++)
            {
                for (; direct + directSpan <= maxDirect; direct++)
                {
                    Point cellPosition = orientation == Orientation.Horizontal ? new Point(direct, indirect) : new Point(indirect, direct);
                    if (!filledCells.Contains(cellPosition))
                    {
                        x = (int)cellPosition.X;
                        y = (int)cellPosition.Y;
                        return true;
                    }
                }

                direct = 0;
            }

            for (; indirect < maxIndirect; indirect++)
            {
                for (; direct < maxDirect; direct++)
                {
                    Point cellPosition = orientation == Orientation.Horizontal ? new Point(direct, indirect) : new Point(indirect, direct);
                    if (!filledCells.Contains(cellPosition))
                    {
                        x = (int)cellPosition.X;
                        y = (int)cellPosition.Y;
                        return true;
                    }
                }

                direct = 0;
            }

            return false;
        }
    }
}
