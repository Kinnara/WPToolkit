// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// The GestureListener class raises events similar to those provided by the XNA TouchPanel, but it is designed for
    /// XAML's event-driven model, rather than XNA's loop/polling model, and it also takes care of the hit testing
    /// and event routing.
    /// </summary>
    [System.Obsolete("GestureListener is obsolete beginning in Windows Phone 8, as the built-in manipulation and gesture events on UIElement now have functional parity with it.")]
    public partial class GestureListener
    {
        private enum GestureState
        {
            None,
            Hold,
            Undetermined,
            Drag,
            Pinch,
        }

        private static bool _isInitialized = false;
        private static Point _gestureOrigin;
        private static bool _gestureChanged = false;
        private static List<UIElement> _elements;
        private static GestureState _state = GestureState.None;
        private static ManipulationDeltaEventArgs _previousDelta = null;


        internal static void Initialize(DependencyObject o)
        {
            var gestureSensitiveElement = o as FrameworkElement;
            if (!_isInitialized && gestureSensitiveElement != null)
            {
                // We need to add event handlers to the application root visual, but it may not
                // be available yet. If that's the case, defer initialization until the visual
                // tree is loaded.
                if (Application.Current.RootVisual == null)
                {
                    gestureSensitiveElement.Loaded += OnGestureSensitiveElementLoaded;
                }
                else
                {
                    Initialize();
                }
            }
        }

        private static void OnGestureSensitiveElementLoaded(object sender, RoutedEventArgs e)
        {
            Initialize();
            (sender as FrameworkElement).Loaded -= OnGestureSensitiveElementLoaded;
        }

        private static void Initialize()
        {
            var root = Application.Current.RootVisual;
            System.Diagnostics.Debug.Assert(root != null);
            root.AddHandler(UIElement.TapEvent, new EventHandler<System.Windows.Input.GestureEventArgs>(OnTap), true);
            root.AddHandler(UIElement.DoubleTapEvent, new EventHandler<System.Windows.Input.GestureEventArgs>(OnDoubleTap), true);
            root.AddHandler(UIElement.HoldEvent, new EventHandler<System.Windows.Input.GestureEventArgs>(OnHold), true);
            root.AddHandler(UIElement.ManipulationStartedEvent, new EventHandler<System.Windows.Input.ManipulationStartedEventArgs>(OnManipulationStarted), true);
            root.AddHandler(UIElement.ManipulationDeltaEvent, new EventHandler<System.Windows.Input.ManipulationDeltaEventArgs>(OnManipulationDelta), true);
            root.AddHandler(UIElement.ManipulationCompletedEvent, new EventHandler<System.Windows.Input.ManipulationCompletedEventArgs>(OnManipulationCompleted), true);
            _isInitialized = true;
        }

        private static void OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var touchPoint = e.GetPosition(Application.Current.RootVisual);
            touchPoint = GetInverseTransform(true).Transform(touchPoint);
            RaiseGestureEvent((handler) => handler.Tap, () => new Microsoft.Phone.Controls.GestureEventArgs(_gestureOrigin, touchPoint), false);
            OnGestureComplete(touchPoint, touchPoint);
        }

        private static void OnDoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var touchPoint = e.GetPosition(Application.Current.RootVisual);
            touchPoint = GetInverseTransform(true).Transform(touchPoint);
            RaiseGestureEvent((handler) => handler.DoubleTap, () => new Microsoft.Phone.Controls.GestureEventArgs(_gestureOrigin, touchPoint), false);
            OnGestureComplete(touchPoint, touchPoint);
        }

        private static void OnHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            _state = GestureState.Hold;
            var touchPoint = e.GetPosition(Application.Current.RootVisual);
            touchPoint = GetInverseTransform(true).Transform(touchPoint);
            RaiseGestureEvent((handler) => handler.Hold, () => new Microsoft.Phone.Controls.GestureEventArgs(_gestureOrigin, touchPoint), false);
        }

        private static void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            
            OnGestureBegin(GetInverseTransform(true, e.ManipulationContainer).Transform(e.ManipulationOrigin));
        }

        private static void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            var oldState = _state;
            var wasGestureChanged = _gestureChanged;
            _state = e.PinchManipulation == null ? GestureState.Drag : GestureState.Pinch;

            var transformToRoot = GetInverseTransform(true, e.ManipulationContainer);

            if (_state == GestureState.Drag)
            {
                var deltaTransform = GetInverseTransform(false, e.ManipulationContainer);
                var totalTranslation = deltaTransform.Transform(e.CumulativeManipulation.Translation);
                var deltaTranslation = deltaTransform.Transform(e.DeltaManipulation.Translation);
                Orientation orientation = GetOrientation(totalTranslation.X, totalTranslation.Y);

                if (wasGestureChanged)
                {
                    OnGestureBegin(transformToRoot.Transform(e.ManipulationOrigin));

                    // Fire the deferred DragStarted
                    RaiseGestureEvent(
                        (handler) => handler.DragStarted,
                        () => new Microsoft.Phone.Controls.DragStartedGestureEventArgs(_gestureOrigin, orientation),
                        true);
                }

                if (oldState == GestureState.Drag)
                {
                    // Continue the drag
                    var currentPoint = new Point(_gestureOrigin.X + totalTranslation.X, _gestureOrigin.Y + totalTranslation.Y);
                    RaiseGestureEvent(
                        (handler) => handler.DragDelta,
                        () => new Microsoft.Phone.Controls.DragDeltaGestureEventArgs(_gestureOrigin, currentPoint, deltaTranslation, orientation),
                        false);
                }

                else
                {
                    if (oldState == GestureState.Pinch)
                    {
                        // End the pinch
                        OnPinchCompleted(_previousDelta);
                        // Transitioning from a pinch to a drag. 
                        _gestureChanged = true;

                        // Note that we are deferring the DragStarted event until the next 
                        // ManipulationDelta is received. 
                        // We want to fire a DragStarted event now, but the ManipulationOrigin
                        // for the first ManipulationDelta after a pinch is completed always corresponds 
                        // to the primary touch point from the previous pinch, even if it was the 
                        // secondary touch point that remains on the screen.
                        // Since the pinch is now completed, but we haven't been able to determine
                        // the drag origin, we'll go back to the Undetermined state.
                        _state = GestureState.Undetermined;
                    }
                    else
                    {
                        RaiseGestureEvent(
                           (handler) => handler.DragStarted,
                           () => new Microsoft.Phone.Controls.DragStartedGestureEventArgs(_gestureOrigin, orientation),
                           true);
                    }
                    
                }
            }
            else // _state is GestureState.Pinch
            {
                var originalPrimary = transformToRoot.Transform(e.PinchManipulation.Original.PrimaryContact);
                var originalSecondary = transformToRoot.Transform(e.PinchManipulation.Original.SecondaryContact);
                var currentPrimary = transformToRoot.Transform(e.PinchManipulation.Current.PrimaryContact);
                var currentSecondary = transformToRoot.Transform(e.PinchManipulation.Current.SecondaryContact);

                if (wasGestureChanged)
                {
                    OnGestureBegin(originalPrimary);
                }

                if (oldState == GestureState.Pinch)
                {
                    // Continue the pinch
                    RaiseGestureEvent(
                        (handler) => handler.PinchDelta,
                        () => new Microsoft.Phone.Controls.PinchGestureEventArgs(originalPrimary, originalSecondary, currentPrimary, currentSecondary),
                        false);
                }
                else
                {
                    if (oldState == GestureState.Drag)
                    {
                        // End the drag
                        OnDragCompleted(_previousDelta);
                        // Transitioning from a drag to a pinch
                        _gestureChanged = true;
                    }

                    // Start the pinch
                    RaiseGestureEvent(
                        (handler) => handler.PinchStarted,
                        () => new Microsoft.Phone.Controls.PinchStartedGestureEventArgs(originalPrimary, originalSecondary, currentPrimary, currentSecondary),
                        true);
                }
            }

            _previousDelta = e;
        }

        private static void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (_state == GestureState.Drag)
            {
                OnDragCompleted(_previousDelta, e);
            }
            else if (_state == GestureState.Pinch)
            {
                System.Diagnostics.Debug.Assert(_previousDelta.PinchManipulation != null);
                OnPinchCompleted(_previousDelta, true);
            }
            else if (_state == GestureState.Hold)
            {
                var deltaTransform = GetInverseTransform(false, e.ManipulationContainer);
                var totalTranslation = deltaTransform.Transform(e.TotalManipulation.Translation);
                var currentPoint = new Point(_gestureOrigin.X + totalTranslation.X, _gestureOrigin.Y + totalTranslation.Y);
                OnGestureComplete(_gestureOrigin, currentPoint);
            }
        }

        private static void OnPinchCompleted(ManipulationDeltaEventArgs lastPinchInfo, bool gestureCompleted = false)
        {
            var transformToRoot = GetInverseTransform(true, lastPinchInfo.ManipulationContainer);
            var originalPrimary = transformToRoot.Transform(_previousDelta.PinchManipulation.Original.PrimaryContact);
            var currentPrimary = transformToRoot.Transform(_previousDelta.PinchManipulation.Current.PrimaryContact);
            RaiseGestureEvent(
                    (handler) => handler.PinchCompleted,
                    () => new Microsoft.Phone.Controls.PinchGestureEventArgs(
                        originalPrimary,
                        transformToRoot.Transform(_previousDelta.PinchManipulation.Original.SecondaryContact),
                        currentPrimary,
                        transformToRoot.Transform(_previousDelta.PinchManipulation.Current.SecondaryContact)),
                    false);

            if (gestureCompleted)
            {
                OnGestureComplete(originalPrimary, currentPrimary);
            }
        }

        private static void OnDragCompleted(ManipulationDeltaEventArgs lastDragInfo, ManipulationCompletedEventArgs completedInfo = null)
        {
            GeneralTransform deltaTransform = null;
            Point releasePoint;
            Point totalTranslation;
            Point finalVelocity;
            Orientation orientation = Orientation.Horizontal;
            bool gestureCompleted = false;

            releasePoint = totalTranslation = finalVelocity = new Point();

            if (completedInfo != null)
            {
                gestureCompleted = true;
                deltaTransform = GetInverseTransform(false, completedInfo.ManipulationContainer);
                totalTranslation = deltaTransform.Transform(completedInfo.TotalManipulation.Translation);
                finalVelocity = deltaTransform.Transform(completedInfo.FinalVelocities.LinearVelocity);

                if (completedInfo.IsInertial)
                {
                    RaiseGestureEvent(
                        (handler) => handler.Flick,
                        () => new Microsoft.Phone.Controls.FlickGestureEventArgs(
                            _gestureOrigin,
                            finalVelocity),
                        true);
                }
            }
            else
            {
                deltaTransform = GetInverseTransform(false, lastDragInfo.ManipulationContainer);
                totalTranslation = deltaTransform.Transform(lastDragInfo.CumulativeManipulation.Translation);
                finalVelocity = deltaTransform.Transform(lastDragInfo.Velocities.LinearVelocity);
            }

            releasePoint = new Point(_gestureOrigin.X + totalTranslation.X, _gestureOrigin.Y + totalTranslation.Y);
            orientation = GetOrientation(totalTranslation.X, totalTranslation.Y);


            RaiseGestureEvent(
                (handler) => handler.DragCompleted,
                () => new Microsoft.Phone.Controls.DragCompletedGestureEventArgs(
                    _gestureOrigin,
                    releasePoint,
                    totalTranslation,
                    orientation,
                    finalVelocity),
                false);

            if (gestureCompleted)
            {
                OnGestureComplete(_gestureOrigin, releasePoint);
            }
        }

        private static void OnGestureBegin(Point touchPoint)
        {
            _gestureChanged = false;
            _gestureOrigin = touchPoint;
            _elements = new List<UIElement>(VisualTreeHelper.FindElementsInHostCoordinates(touchPoint, Application.Current.RootVisual));

            if (_state == GestureState.None)
            {
                // The GestureBegin event should only be raised if there were no fingers on the screen previously.
                _state = GestureState.Undetermined;
                RaiseGestureEvent(
                    (handler) => handler.GestureBegin, 
                    () => new Microsoft.Phone.Controls.GestureEventArgs(touchPoint, touchPoint), 
                    false);
            }
        }

        private static void OnGestureComplete(Point gestureOrigin, Point releasePoint)
        {
            _state = GestureState.None;
            _previousDelta = null;

            RaiseGestureEvent((handler) => handler.GestureCompleted, () => new Microsoft.Phone.Controls.GestureEventArgs(gestureOrigin, releasePoint), false);
        }

        private static GeneralTransform GetInverseTransform(bool includeOffset, UIElement target = null)
        {
            GeneralTransform transform = Application.Current.RootVisual.TransformToVisual(target).Inverse;

            if (!includeOffset)
            {
                MatrixTransform matrixTransform = transform as MatrixTransform;
                if (matrixTransform != null)
                {
                    Matrix matrix = matrixTransform.Matrix;
                    matrix.OffsetX = matrix.OffsetY = 0;
                    matrixTransform.Matrix = matrix;
                }
            }

            return transform;
        }

        private static Orientation GetOrientation(double x, double y)
        {
            return Math.Abs(x) >= Math.Abs(y) ? 
                System.Windows.Controls.Orientation.Horizontal : System.Windows.Controls.Orientation.Vertical;
        }

        /// <summary>
        /// This method does all the necessary work to raise a gesture event. It sets the orginal source, does the routing,
        /// handles Handled, and only creates the event args if they are needed.
        /// </summary>
        /// <typeparam name="T">This is the type of event args that will be raised.</typeparam>
        /// <param name="eventGetter">Gets the specific event to raise.</param>
        /// <param name="argsGetter">Lazy creator function for the event args.</param>
        /// <param name="releaseMouseCapture">Indicates whether the mouse capture should be released </param>
        private static void RaiseGestureEvent<T>(
            Func<GestureListener, EventHandler<T>> eventGetter, 
            Func<T> argsGetter, bool releaseMouseCapture) 
                where T : GestureEventArgs
        {
            T args = null;

            FrameworkElement originalSource = null;
            bool handled = false;

            foreach (FrameworkElement element in _elements)
            {
                if (releaseMouseCapture)
                {
                    element.ReleaseMouseCapture();
                }

                if (!handled)
                {
                    if (originalSource == null)
                    {
                        originalSource = element;
                    }

                    GestureListener helper = GestureService.GetGestureListenerInternal(element, false);
                    if (helper != null)
                    {
                        SafeRaise.Raise(eventGetter(helper), element, () =>
                        {
                            if (args == null)
                            {
                                args = argsGetter();
                                args.OriginalSource = originalSource;
                            }
                            return args;
                        });
                    }

                    if (args != null && args.Handled == true)
                    {
                        handled = true;
                    }
                }
            }
        }
    }
}