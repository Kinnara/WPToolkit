using System;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    internal static class AnimationHelper
    {
        private static readonly CacheMode BitmapCacheMode = new BitmapCache();

        public static void InvokeOnSecondRendering(Action a)
        {
            int frameCount = 0;
            EventHandler handler = null;
            handler = (sender, e) =>
            {
                if (++frameCount == 2)
                {
                    CompositionTarget.Rendering -= handler;
                    a();
                }
            };
            CompositionTarget.Rendering += handler;
        }

        public static void PrepareForCompositor(UIElement element)
        {
            if (!(element.CacheMode is BitmapCache))
            {
                element.CacheMode = BitmapCacheMode;
            }
        }
    }
}
