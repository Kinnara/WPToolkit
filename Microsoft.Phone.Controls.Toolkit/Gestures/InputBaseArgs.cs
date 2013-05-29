using System.Windows;

namespace Microsoft.Phone.Gestures
{
    internal class InputBaseArgs
    {
        protected InputBaseArgs(UIElement source, Point origin)
        {
            Source = source;
            Origin = origin;
        }

        public UIElement Source { get; private set; }

        public Point Origin { get; private set; }
    }
}
