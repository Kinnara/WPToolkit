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
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PhoneContentControl.ContentTemplateSelector"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PhoneContentControl.ContentTemplateSelector"/> dependency property.
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

            OnContentTemplateSelectorChanged((DataTemplateSelector)e.OldValue, (DataTemplateSelector)e.NewValue);
        }

        #endregion

        /// <summary>
        /// Gets the actual data template that is used to display the content of the <see cref="T:Microsoft.Phone.Controls.PhoneContentControl"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The actual data template that is used to display the content of the <see cref="T:Microsoft.Phone.Controls.PhoneContentControl"/>.
        /// </returns>
        public DataTemplate ActualContentTemplate
        {
            get { return (DataTemplate)GetValue(ActualContentTemplateProperty); }
            private set { SetValue(ActualContentTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.PhoneContentControl.ActualContentTemplate"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.PhoneContentControl.ActualContentTemplate"/> dependency property.
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

            OnContentTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
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

        /// <summary>
        /// Called when the <see cref="P:System.Windows.Controls.ContentControl.ContentTemplate"/> property changes.
        /// </summary>
        /// <param name="oldContentTemplate">The old value of the <see cref="P:System.Windows.Controls.ContentControl.ContentTemplate"/> property.</param>
        /// <param name="newContentTemplate">The new value of the <see cref="P:System.Windows.Controls.ContentControl.ContentTemplate"/> property.</param>
        protected virtual void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
        {
        }

        /// <summary>
        /// Called when the <see cref="P:Microsoft.Phone.Controls.PhoneContentControl.ContentTemplateSelector"/> property changes.
        /// </summary>
        /// <param name="oldContentTemplateSelector">The old value of the <see cref="P:Microsoft.Phone.Controls.PhoneContentControl.ContentTemplateSelector"/> property.</param>
        /// <param name="newContentTemplateSelector">The new value of the <see cref="P:Microsoft.Phone.Controls.PhoneContentControl.ContentTemplateSelector"/> property.</param>
        protected virtual void OnContentTemplateSelectorChanged(DataTemplateSelector oldContentTemplateSelector, DataTemplateSelector newContentTemplateSelector)
        {
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

            ActualContentTemplate = template;
        }
    }
}
