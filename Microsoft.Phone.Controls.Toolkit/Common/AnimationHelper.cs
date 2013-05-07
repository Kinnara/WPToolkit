using System;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    internal static class AnimationHelper
    {
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
    }
}
