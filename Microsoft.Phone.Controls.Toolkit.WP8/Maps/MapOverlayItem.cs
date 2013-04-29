// ---------------------------------------------------------------------------
// <copyright file="MapOverlayItem.cs" company="Microsoft">
//     (c) Copyright Microsoft Corporation.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Microsoft.Phone.Maps.Toolkit
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Phone.Maps.Controls;

    /// <summary>
    /// MapOverlayItem class
    /// This class helps with the task to create the target item to be presented when a template is provided.
    /// This target item will have bindings to the MapOverlay. 
    /// When the item has been resolved (from template + content), this class will take care of creating the bindings.
    /// When there is no template and the content is the UI, it will follow the same pattern
    /// to wait until item is visible before binding the dependency properties.
    /// </summary>
    internal class MapOverlayItem : ContentPresenter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapOverlayItem"/> class
        /// </summary>
        /// <param name="content">Content to be used</param>
        /// <param name="contentTemplate">Content template</param>
        /// <param name="mapOverlay">MapOverlay that will be used to bind the dependency properties when content becomes visible</param>
        public MapOverlayItem(object content, DataTemplate contentTemplate, MapOverlay mapOverlay)
        {
            this.ContentTemplate = contentTemplate;
            this.Content = content;
            this.MapOverlay = mapOverlay;
        }

        /// <summary>
        /// Gets or sets the MapOverlay that will be used to bind the properties
        /// </summary>
        private MapOverlay MapOverlay { get; set; }

        /// <summary>
        /// OnApplyTemplate override.
        /// Will take care of binding the dependency properties.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            MapChild.BindMapOverlayProperties(this.MapOverlay);
        }
    }
}
