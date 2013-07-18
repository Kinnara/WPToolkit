using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Arranges child elements into a single line that can be oriented horizontally or vertically.
    /// Supports UI virtualization. Can only be used to display items in an ItemsControl.
    /// </summary>
    public class ItemsStackPanel : VirtualizingStackPanel
    {
        #region public Thickness Padding

        /// <summary>
        /// Gets or sets the distance between the panel and its children.
        /// </summary>
        /// 
        /// <returns>
        /// The dimensions of the space between the panel and its children as a <see cref="T:System.Windows.Thickness"/> value.
        /// The <see cref="T:System.Windows.Thickness"/> values are in pixels.
        /// </returns>
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.ItemsStackPanel.Padding"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.ItemsStackPanel.Padding"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            "Padding",
            typeof(Thickness),
            typeof(ItemsStackPanel),
            new PropertyMetadata(OnPaddingChanged));

        private static void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ItemsStackPanel)d).ApplyPadding();
        }

        #endregion

        /// <summary>
        /// Handles the measure pass for the control.
        /// </summary>
        /// 
        /// <returns>
        /// The desired size.
        /// </returns>
        /// <param name="constraint">The available size.</param>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#", Justification = "Compat with WPF.")]
        protected override System.Windows.Size MeasureOverride(Size constraint)
        {
            Size desiredSize = base.MeasureOverride(constraint);
            ApplyPadding();
            return desiredSize;
        }

        private void ApplyPadding()
        {
            if (IsItemsHost)
            {
                ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
                if (itemsControl != null)
                {
                    int itemsCount = itemsControl.Items.Count;
                    if (itemsCount > 0)
                    {
                        Thickness padding = Padding;
                        bool isVertical = Orientation == Orientation.Vertical;

                        FrameworkElement firstContainer = itemsControl.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
                        FrameworkElement lastContainer = itemsControl.ItemContainerGenerator.ContainerFromIndex(itemsCount - 1) as FrameworkElement;

                        if (firstContainer != null || lastContainer != null)
                        {
                            if (firstContainer != lastContainer)
                            {
                                if (firstContainer != null)
                                {
                                    Thickness margin = new Thickness();
                                    if (isVertical)
                                    {
                                        margin.Top = padding.Top;
                                    }
                                    else
                                    {
                                        margin.Left = padding.Left;
                                    }

                                    firstContainer.Margin = margin;
                                }

                                if (lastContainer != null)
                                {
                                    Thickness margin = new Thickness();
                                    if (isVertical)
                                    {
                                        margin.Bottom = padding.Bottom;
                                    }
                                    else
                                    {
                                        margin.Right = padding.Right;
                                    }

                                    lastContainer.Margin = margin;
                                }
                            }
                            else
                            {
                                Thickness margin = new Thickness();
                                if (isVertical)
                                {
                                    margin.Top = padding.Top;
                                    margin.Bottom = padding.Bottom;
                                }
                                else
                                {
                                    margin.Left = padding.Left;
                                    margin.Right = padding.Right;
                                }

                                firstContainer.Margin = margin;
                            }
                        }

                        for (int i = 1; i < itemsCount - 1; i++)
                        {
                            FrameworkElement container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                            if (container != null)
                            {
                                container.ClearValue(FrameworkElement.MarginProperty);
                            }
                        }
                    }
                }
            }
        }
    }
}
