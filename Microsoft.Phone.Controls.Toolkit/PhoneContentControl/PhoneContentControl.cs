using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// An extended ContentControl.
    /// </summary>
    public class PhoneContentControl : ContentControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Phone.Controls.PhoneContentControl"/> class.
        /// </summary>
        public PhoneContentControl()
        {
            DefaultStyleKey = typeof(PhoneContentControl);

            SetBinding(ContentTemplateShadowProperty, new Binding("ContentTemplate") { Source = this });
        }

        #region public DataTemplateSelector ContentTemplateSelector

        /// <summary>
        /// Gets or sets a selection object that changes the DataTemplate to apply for content, based on processing information about the content item or its container at run time.
        /// </summary>
        /// 
        /// <returns>
        /// A selection object that changes the DataTemplate to apply for content.
        /// </returns>
        public DataTemplateSelector ContentTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ContentTemplateSelectorProperty); }
            set { SetValue(ContentTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the ContentTemplateSelector dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the ContentTemplateSelector dependency property.
        /// </returns>
        public static readonly DependencyProperty ContentTemplateSelectorProperty = DependencyProperty.Register(
            "ContentTemplateSelector",
            typeof(DataTemplateSelector),
            typeof(PhoneContentControl),
            new PropertyMetadata(null, (d, e) => ((PhoneContentControl)d).OnContentTemplateSelectorChanged(e)));

        private void OnContentTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ContentTemplate == null)
            {
                EnsureTemplate();
            }
        }

        #endregion

        /// <summary>
        /// Identifies the ActualContentTemplate dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the ActualContentTemplate dependency property.
        /// </returns>
        public static readonly DependencyProperty ActualContentTemplateProperty = DependencyProperty.Register(
            "ActualContentTemplate",
            typeof(DataTemplate),
            typeof(PhoneContentControl),
            null);

        private static readonly DependencyProperty ContentTemplateShadowProperty = DependencyProperty.Register(
            "ContentTemplateShadow",
            typeof(DataTemplate),
            typeof(PhoneContentControl),
            new PropertyMetadata(null, (d, e) => ((PhoneContentControl)d).OnContentTemplateShadowChanged(e)));

        private void OnContentTemplateShadowChanged(DependencyPropertyChangedEventArgs e)
        {
            EnsureTemplate();
        }

        /// <summary>
        /// Called when the value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property changes.
        /// </summary>
        /// <param name="oldContent">The old value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property.</param>
        /// <param name="newContent">The new value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property.</param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (ContentTemplate == null)
            {
                EnsureTemplate();
            }
        }

        private void EnsureTemplate()
        {
            DataTemplate template = ContentTemplate;

            if (template == null)
            {
                if (ContentTemplateSelector != null)
                {
                    template = ContentTemplateSelector.SelectTemplate(Content, this);
                }
            }

            SetValue(ActualContentTemplateProperty, template);
        }
    }
}
