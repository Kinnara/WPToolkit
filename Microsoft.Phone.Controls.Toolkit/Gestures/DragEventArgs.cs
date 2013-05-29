using System.Windows;

namespace Microsoft.Phone.Gestures
{
    internal class DragEventArgs : GestureEventArgs
    {
        public DragEventArgs()
        {
        }

        public DragEventArgs(InputDeltaArgs args)
        {
            if (args != null)
            {
                CumulativeDistance = args.CumulativeTranslation;
                DeltaDistance = args.DeltaTranslation;
            }
        }

        public bool IsTouchComplete { get; private set; }

        public Point DeltaDistance { get; private set; }

        public Point CumulativeDistance { get; internal set; }

        public void MarkAsFinalTouchManipulation()
        {
            IsTouchComplete = true;
        }
    }
}
