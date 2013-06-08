// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Samples
{
    public partial class GestureSample : PhoneApplicationPage
    {
        SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush normalBrush;

        bool isPinch;
        bool isDrag;
        double initialAngle;
        bool isGestureOnTarget;

        public GestureSample()
        {
            InitializeComponent();

            normalBrush = (SolidColorBrush) Resources["PhoneAccentBrush"];
            isPinch = false;
            isDrag = false;
            isGestureOnTarget = false;

            MessageBox.Show(
@"The GestureListener is now obsolete in Windows Phone 8, as the built-in manipulation and gesture events now have functional parity with it.

This sample and the sample code demonstrates how to use the manipulation and gesture events for purposes for which one previously would have used the GestureListener.");
        }

#region UIElement touch event handlers

        // UIElement.ManipulationStarted indicates the beginning of a touch interaction. It tells us
        // that we went from having no fingers on the screen to having at least one finger on the screen.
        // It doesn't tell us what gesture this is going to become, but it can be useful for 
        // initializing your gesture handling code.
        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
        }

        // UIElement.Tap is used in place of GestureListener.Tap.
        private void OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            transform.TranslateX = transform.TranslateY = 0;
        }

        // UIElement.DoubleTap is used in place of GestureListener.DoubleTap.
        private void OnDoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            transform.ScaleX = transform.ScaleY = 1;
        }

        // UIElement.Hold is used in place of GestureListener.Hold.
        private void OnHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            transform.TranslateX = transform.TranslateY = 0;
            transform.ScaleX = transform.ScaleY = 1;
            transform.Rotation = 0;
        }

        // UIElement.ManipulationDelta represents either a drag or a pinch.
        // If PinchManipulation == null, then we have a drag, corresponding to GestureListener.DragStarted, 
        // GestureListener.DragDelta, or GestureListener.DragCompleted.
        // If PinchManipulation != null, then we have a pinch, corresponding to GestureListener.PinchStarted, 
        // GestureListener.PinchDelta, or GestureListener.PinchCompleted.
        // 
        // In this sample we track drag and pinch state to illustrate how to manage transitions between 
        // pinching and dragging, but commonly only the pinch or drag deltas will be of interest, in which 
        // case determining when pinches and drags begin and end is not necessary.
        //
        // Note that the exact APIs for the event args are not quite the same as the ones in GestureListener.
        // Comments inside methods called from here will note where they diverge.
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            bool oldIsPinch = isPinch;
            bool oldIsDrag = isDrag;
            isPinch = e.PinchManipulation != null;

            // The origin of the first manipulation after a pinch is completed always corresponds to the
            // primary touch point from the pinch, even if the secondary touch point is the one that 
            // remains active. In this sample we only want a drag to affect the rectangle if the finger
            // on the screen falls inside the rectangle's bounds, so if we've just finished a pinch,
            // we have to defer until the next ManipulationDelta to determine whether or not a new 
            // drag has started.
            isDrag = e.PinchManipulation == null && !oldIsPinch;
  
            // check for ending gestures
            if (oldIsDrag && !isDrag)
            {
                this.OnDragCompleted();
            }
            if (oldIsPinch && !isPinch)
            {
                this.OnPinchCompleted();
            }

            // check for continuing gestures
            if (oldIsDrag && isDrag)
            {
                this.OnDragDelta(sender, e);
            }
            if (oldIsPinch && isPinch)
            {
                this.OnPinchDelta(sender, e);
            }

            // check for starting gestures
            if (!oldIsDrag && isDrag)
            {
                // Once a manipulation has started on the UIElement, that element will continue to fire ManipulationDelta
                // events until all fingers have left the screen and we get a ManipulationCompleted. In this sample
                // however, we treat each transition between pinch and drag as a new gesture, and we only want to 
                // apply effects to our border control if the the gesture begins within the bounds of the border.
                isGestureOnTarget = e.ManipulationContainer == border && 
                        new Rect(0, 0, border.ActualWidth, border.ActualHeight).Contains(e.ManipulationOrigin);
                this.OnDragStarted();
            }
            if (!oldIsPinch && isPinch)
            {
                isGestureOnTarget = e.ManipulationContainer == border && 
                        new Rect(0, 0, border.ActualWidth, border.ActualHeight).Contains(e.PinchManipulation.Original.PrimaryContact);
                this.OnPinchStarted(sender, e);
            }
        }

        // UIElement.ManipulationCompleted indicates the end of a touch interaction. It tells us that
        // we went from having at least one finger on the screen to having no fingers on the screen.
        // If e.IsInertial is true, then it's also the same thing as GestureListener.Flick,
        // although the event args API for the flick case are different, as will be noted inside that method.
        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (isDrag)
            {
                isDrag = false;
                this.OnDragCompleted();

                if (e.IsInertial)
                {
                    this.OnFlick(sender, e);
                }
            }

            if (isPinch)
            {
                isPinch = false;
                this.OnPinchCompleted();
            }
        }

#endregion

#region Gesture events inferred from UIElement.Manipulation* touch events

        private void OnDragStarted()
        {
            if (isGestureOnTarget)
            {
                border.Background = greenBrush;
            }
        }

        private void OnDragDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (isGestureOnTarget)
            {
                // HorizontalChange and VerticalChange from DragDeltaGestureEventArgs are now
                // DeltaManipulation.Translation.X and DeltaManipulation.Translation.Y.
                
                // The translation is given in the coordinate space of e.ManipulationContainer, which in
                // this case is the border control that we're applying transforms to. We need to apply 
                // the the current rotation and scale transforms to the deltas to get back to screen coordinates.
                // Note that if other ancestors of the border control had transforms applied as well, we would
                // need to use UIElement.TransformToVisual to get the aggregate transform between
                // the border control and Application.Current.RootVisual. See GestureListenerStatic.cs in the 
                // WP8 toolkit source for a detailed look at how this can be done.
                Point transformedTranslation = GetTransformNoTranslation(transform).Transform(e.DeltaManipulation.Translation);

                transform.TranslateX += transformedTranslation.X;
                transform.TranslateY += transformedTranslation.Y;
            }
        }

        private void OnDragCompleted()
        {
            if (isGestureOnTarget)
            {
                border.Background = normalBrush;
            }
        }

        private void OnPinchStarted(object sender, ManipulationDeltaEventArgs e)
        {
            if (isGestureOnTarget)
            {
                border.Background = redBrush;
                initialAngle = transform.Rotation;
            }
        }

        private void OnPinchDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (isGestureOnTarget)
            {
                // Rather than providing the rotation, the event args now just provide
                // the raw points of contact for the pinch manipulation.
                // However, calculating the rotation from these two points is fairly trivial;
                // the utility method used here illustrates how that's done.
                // Note that we don't have to apply a transform because the angle delta is the
                // same in any non-skewed reference frame.
                double angleDelta = this.GetAngle(e.PinchManipulation.Current) - this.GetAngle(e.PinchManipulation.Original);

                transform.Rotation = initialAngle + angleDelta;

                // DistanceRatio from PinchGestureEventArgs is now replaced by
                // PinchManipulation.DeltaScale and PinchManipulation.CumulativeScale,
                // which expose the scale from the pinch directly.
                // Note that we don't have to apply a transform because the distance ratio is the
                // same in any reference frame.
                transform.ScaleX *= e.PinchManipulation.DeltaScale;
                transform.ScaleY *= e.PinchManipulation.DeltaScale;
            }
        }

        private void OnPinchCompleted()
        {
            if (isGestureOnTarget)
            {
                border.Background = normalBrush;
            }
        }

        private void OnFlick(object sender, ManipulationCompletedEventArgs e)
        {
            if (isGestureOnTarget)
            {
                // All of the properties on FlickGestureEventArgs have been replaced by the single property
                // FinalVelocities.LinearVelocity.  This method shows how to retrieve from FinalVelocities.LinearVelocity
                // the properties that used to be in FlickGestureEventArgs. Also, note that while the GestureListener
                // provided fairly precise directional information, small linear velocities here are rounded
                // to 0, resulting in flick vectors that are often snapped to one axis.

                Point transformedVelocity = GetTransformNoTranslation(transform).Transform(e.FinalVelocities.LinearVelocity);

                double horizontalVelocity = transformedVelocity.X;
                double verticalVelocity = transformedVelocity.Y;

                flickData.Text = string.Format("{0} Flick: Angle {1} Velocity {2},{3}",
                    this.GetDirection(horizontalVelocity, verticalVelocity), Math.Round(this.GetAngle(horizontalVelocity, verticalVelocity)), horizontalVelocity, verticalVelocity);
            }
        }
#endregion

#region Helpers
        private GeneralTransform GetTransformNoTranslation(CompositeTransform transform)
        {
            CompositeTransform newTransform = new CompositeTransform();
            newTransform.Rotation = transform.Rotation;
            newTransform.ScaleX = transform.ScaleX;
            newTransform.ScaleY = transform.ScaleY;
            newTransform.CenterX = transform.CenterX;
            newTransform.CenterY = transform.CenterY;
            newTransform.TranslateX = 0;
            newTransform.TranslateY = 0;

            return newTransform;
        }

        private double GetAngle(PinchContactPoints points)
        {
            Point directionVector = new Point(points.SecondaryContact.X - points.PrimaryContact.X, points.SecondaryContact.Y - points.PrimaryContact.Y);
            return GetAngle(directionVector.X, directionVector.Y);
        }

        private Orientation GetDirection(double x, double y)
        {
            return Math.Abs(x) >= Math.Abs(y) ? System.Windows.Controls.Orientation.Horizontal : System.Windows.Controls.Orientation.Vertical;
        }

        private double GetAngle(double x, double y)
        {
            // Note that this function works in xaml coordinates, where positive y is down, and the
            // angle is computed clockwise from the x-axis. 
            double angle = Math.Atan2(y, x);

            // Atan2() returns values between pi and -pi.  We want a value between
            // 0 and 2 pi.  In order to compensate for this, we'll add 2 pi to the angle
            // if it's less than 0, and then multiply by 180 / pi to get the angle
            // in degrees rather than radians, which are the expected units in XAML.
            if (angle < 0)
            {
                angle += 2 * Math.PI;
            }

            return angle * 180 / Math.PI;
        }
#endregion
    }
}