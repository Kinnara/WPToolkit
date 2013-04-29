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
        double initialAngle;

        public GestureSample()
        {
            InitializeComponent();

            normalBrush = (SolidColorBrush) Resources["PhoneAccentBrush"];
            isPinch = false;

            MessageBox.Show(
@"The GestureListener is now obsolete in Windows Phone 8, as the built-in manipulation and gesture events now have functional parity with it.

This sample and the sample code demonstrates how to use the manipulation and gesture events for purposes for which one previously would have used the GestureListener.");
        }

        // UIElement.Tap is the same thing as GestureListener.Tap.
        private void OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            transform.TranslateX = transform.TranslateY = 0;
        }

        // UIElement.DoubleTap is the same thing as GestureListener.DoubleTap.
        private void OnDoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            transform.ScaleX = transform.ScaleY = 1;
        }

        // UIElement.Hold is the same thing as GestureListener.Hold.
        private void OnHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            transform.TranslateX = transform.TranslateY = 0;
            transform.ScaleX = transform.ScaleY = 1;
            transform.Rotation = 0;
        }

        // UIElement.ManipulationStarted is the same thing as GestureListener.DragStarted.
        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            border.Background = greenBrush;
            isPinch = false;
        }

        // UIElement.ManipulationDelta represents either a drag or a pinch.
        // If PinchManipulation == null, then we have a drag, corresponding to GestureListener.DragDelta.
        // If PinchManipulation != null, then we have a pinch.
        // This can correspond to GestureListener.PinchStarted, GestureListener.PinchDelta, or GestureListener.PinchCompleted.
        // In order to know which is the case (if it matters - typically you'll only be interested in PinchDelta),
        // you'll need to make use of a boolean value that says whether or not you're currently pinching.
        // If PinchManipulation != null and the value is false, then that's the same as GestureListener.PinchStarted.
        // If PinchManipulation != null and the value is true, then that's the same as GestureListener.PinchDelta.
        // If PinchManipulation == null and the value is true, then that's the same as GestureListener.PinchCompleted.
        // Of course, if you're only interested in PinchDelta, then you can just use PinchManipulation when PinchManipulation != null.
        // Note that the exact APIs for the event args are not quite the same as the ones in GestureListener.
        // Comments inside methods called from here will note where they diverge.
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            bool oldIsPinch = isPinch;
            isPinch = e.PinchManipulation != null;

            if (oldIsPinch == false && isPinch == false)
            {
                this.OnDragDelta(sender, e);
            }
            else if (oldIsPinch == false && isPinch == true)
            {
                this.OnPinchStarted(sender, e);
            }
            else if (oldIsPinch == true && isPinch == true)
            {
                this.OnPinchDelta(sender, e);
            }
            else if (oldIsPinch == true && isPinch == false)
            {
                this.OnPinchCompleted(sender, e);
            }
        }

        private void OnDragDelta(object sender, ManipulationDeltaEventArgs e)
        {
            // HorizontalChange and VerticalChange from DragDeltaGestureEventArgs are now
            // DeltaManipulation.Translation.X and DeltaManipulation.Translation.Y.
            transform.TranslateX += e.DeltaManipulation.Translation.X;
            transform.TranslateY += e.DeltaManipulation.Translation.Y;
        }

        private void OnPinchStarted(object sender, ManipulationDeltaEventArgs e)
        {
            border.Background = redBrush;
            initialAngle = transform.Rotation;
        }

        private void OnPinchDelta(object sender, ManipulationDeltaEventArgs e)
        {
            // Rather than providing the rotation, the event args now just provide
            // the raw points of contact for the pinch manipulation.
            // However, calculating the rotation from these two points is fairly trivial;
            // the utility method used here illustrates how that's done.
            double angleDelta = this.GetAngle(e.PinchManipulation.Current) - this.GetAngle(e.PinchManipulation.Original);

            transform.Rotation = initialAngle + angleDelta;

            // DistanceRatio from PinchGestureEventArgs is now replaced by
            // PinchManipulation.DeltaScale and PinchManipulation.CumulativeScale,
            // which expose the scale from the pinch directly.
            transform.ScaleX *= e.PinchManipulation.DeltaScale;
            transform.ScaleY *= e.PinchManipulation.DeltaScale;
        }

        private void OnPinchCompleted(object sender, ManipulationDeltaEventArgs e)
        {
            border.Background = greenBrush;
        }

        private double GetAngle(PinchContactPoints points)
        {
            Point directionVector = new Point(points.SecondaryContact.X - points.PrimaryContact.X, points.SecondaryContact.Y - points.PrimaryContact.Y);
            return GetAngle(directionVector.X, directionVector.Y);
        }

        // UIElement.ManipulationStarted is the same thing as GestureListener.DragCompleted.
        // If e.IsInertial is true, then it's also the same thing as GestureListener.Flick,
        // although the event args API for the flick case are different, as will be noted inside that method.
        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            border.Background = normalBrush;

            if (e.IsInertial)
            {
                this.OnFlick(sender, e);
            }
        }

        private void OnFlick(object sender, ManipulationCompletedEventArgs e)
        {
            // All of the properties on FlickGestureEventArgs have been replaced by the single property
            // FinalVelocities.LinearVelocity.  This method shows how to retrieve from FinalVelocities.LinearVelocity
            // the properties that used to be in FlickGestureEventArgs.
            double horizontalVelocity = e.FinalVelocities.LinearVelocity.X;
            double verticalVelocity = e.FinalVelocities.LinearVelocity.Y;

            flickData.Text = string.Format("{0} Flick: Angle {1} Velocity {2},{3}",
                this.GetDirection(horizontalVelocity, verticalVelocity), Math.Round(this.GetAngle(horizontalVelocity, verticalVelocity)), horizontalVelocity, verticalVelocity);
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
    }
}