using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides attached properties to flip FrameworkElements in
    /// and out during page transitions. The result is a 'readerboard' effect
    /// added to the select elements.
    /// </summary>
    public sealed class ReaderboardEffect : DependencyObject
    {
        /// <summary>
        /// The duration in milliseconds that each element takes
        /// to flip in.
        /// </summary>
        private const double InDuration = 350.0;

        /// <summary>
        /// The initial angle position for an element 
        /// that flips in.
        /// </summary>
        private const double InAngle = -60.0;

        /// <summary>
        /// The duration in milliseconds that each element takes
        /// to flip out.
        /// </summary>
        private const double OutDuration = 250.0;

        /// <summary>
        /// The final angle position for an element 
        /// that flips out.
        /// </summary>
        private const double OutAngle = 90.0;

        /// <summary>
        /// The delay in milliseconds between each element.
        /// </summary>
        private const double Delay = 20.0;

        /// <summary>
        /// The easing function that defines the exponential inwards 
        /// interpolation of the storyboards.
        /// </summary>
        private static readonly ExponentialEase ExponentialEaseIn = new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 5 };

        /// <summary>
        /// The easing function that defines the exponential outwards
        /// interpolation of the storyboards.
        /// </summary>
        private static readonly ExponentialEase ExponentialEaseOut = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 5 };

        /// <summary>
        /// The property path used to map the animation's target property
        /// to the RotationX property of the plane projection of a UI element.
        /// </summary>
        private static readonly PropertyPath RotationXPropertyPath = new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationX)");

        /// <summary>
        /// The property path used to map the animation's target property
        /// to the Opacity property of a UI element.
        /// </summary>
        private static readonly PropertyPath OpacityPropertyPath = new PropertyPath("(UIElement.Opacity)");

        /// <summary>
        /// A point with coordinate (0, 0).
        /// </summary>
        private static readonly Point Origin = new Point(0, 0);

        /// <summary>
        /// Private manager that represents a correlation between pages
        /// and the indexed elements it contains.
        /// </summary>
        private static Dictionary<PhoneApplicationPage, List<WeakReference>> _pagesToReferences = new Dictionary<PhoneApplicationPage, List<WeakReference>>();

        /// <summary>
        /// Identifies the set of framework elements that are targeted
        /// to be animated.
        /// </summary>
        private static IList<WeakReference> _targets;

        /// <summary>
        /// Indicates whether the targeted framework elements need their
        /// projections to be restored.
        /// </summary>
        private static bool _pendingRestore;

        /// <summary>
        /// Default list of types that cannot be animated.
        /// </summary>
        private static IList<Type> _nonPermittedTypes = new List<Type>() 
            { 
                typeof(PhoneApplicationFrame), 
                typeof(PhoneApplicationPage), 
                typeof(PivotItem),
                typeof(Panorama),
                typeof(PanoramaItem)
            };

        /// <summary>
        /// Default list of types that cannot be animated.
        /// </summary>
        public static IList<Type> NonPermittedTypes
        {
            get { return _nonPermittedTypes; }
        }

        #region RowIndex DependencyProperty

        /// <summary>
        /// Gets the row index of the specified dependency object.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <returns>The row index.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        public static int GetRowIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(RowIndexProperty);
        }

        /// <summary>
        /// Sets the row index of the specified dependency object.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="value">The row index.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        public static void SetRowIndex(DependencyObject obj, int value)
        {
            obj.SetValue(RowIndexProperty, value);
        }

        /// <summary>
        /// Identifies the row index of the current element,
        /// which represents its place in the flipping order sequence.
        /// </summary>
        public static readonly DependencyProperty RowIndexProperty =
            DependencyProperty.RegisterAttached("RowIndex", typeof(int), typeof(ReaderboardEffect), new PropertyMetadata(-1, OnRowIndexPropertyChanged));

        /// <summary>
        /// Subscribes an element to the private manager.
        /// </summary>
        /// <param name="obj">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnRowIndexPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement target = obj as FrameworkElement;

            if (target == null)
            {
                string message = string.Format(CultureInfo.InvariantCulture, "The dependency object must be of the type {0}.", typeof(FrameworkElement));
                throw new InvalidOperationException(message);
            }

            CheckForTypePermission(target);

            int index = (int)e.NewValue;

            if (index < 0)
            {
                // Dettach event handlers.
                if (ReaderboardEffect.GetHasEventsAttached(target))
                {
                    target.SizeChanged -= Target_SizeChanged;
                    target.Unloaded -= Target_Unloaded;
                    ReaderboardEffect.SetHasEventsAttached(target, false);
                }

                UnsubscribeFrameworkElement(target);
            }
            else
            {
                // Attach event handlers.
                if (!ReaderboardEffect.GetHasEventsAttached(target))
                {
                    target.SizeChanged += Target_SizeChanged;
                    target.Unloaded += Target_Unloaded;
                    ReaderboardEffect.SetHasEventsAttached(target, true);
                }

                SubscribeFrameworkElement(target);
            }
        }

        #endregion

        #region ParentPage DependencyProperty

        /// <summary>
        /// Gets the parent page of the specified dependency object.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <returns>The page.</returns>
        private static PhoneApplicationPage GetParentPage(DependencyObject obj)
        {
            return (PhoneApplicationPage)obj.GetValue(ParentPageProperty);
        }

        /// <summary>
        /// Sets the parent page of the specified dependency object.
        /// </summary>
        /// <param name="obj">The depedency object.</param>
        /// <param name="value">The page.</param>
        private static void SetParentPage(DependencyObject obj, PhoneApplicationPage value)
        {
            obj.SetValue(ParentPageProperty, value);
        }

        /// <summary>
        /// Identifies the ParentPage dependency property.
        /// </summary>
        private static readonly DependencyProperty ParentPageProperty =
            DependencyProperty.RegisterAttached("ParentPage", typeof(PhoneApplicationPage), typeof(ReaderboardEffect), new PropertyMetadata(null, OnParentPagePropertyChanged));

        /// <summary>
        /// Manages subscription to a page.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnParentPagePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement target = (FrameworkElement)obj;
            PhoneApplicationPage oldPage = (PhoneApplicationPage)e.OldValue;
            PhoneApplicationPage newPage = (PhoneApplicationPage)e.NewValue;
            List<WeakReference> references;

            if (newPage != null)
            {
                if (!_pagesToReferences.TryGetValue(newPage, out references))
                {
                    references = new List<WeakReference>();
                    _pagesToReferences.Add(newPage, references);
                }
                else
                {
                    WeakReferenceHelper.RemoveNullTargetReferences(references);
                }

                if (!WeakReferenceHelper.ContainsTarget(references, target))
                {
                    references.Add(new WeakReference(target));
                }

                references.Sort(SortReferencesByIndex);
            }
            else
            {
                if (_pagesToReferences.TryGetValue(oldPage, out references))
                {
                    WeakReferenceHelper.TryRemoveTarget(references, target);

                    if (references.Count == 0)
                    {
                        _pagesToReferences.Remove(oldPage);
                    }
                }
            }
        }

        #endregion

        #region IsSubscribed DependencyProperty

        /// <summary>
        /// Gets whether the specified dependency object
        /// is subscribed to the private manager or not.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <returns>The value.</returns>        
        private static bool GetIsSubscribed(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSubscribedProperty);
        }

        /// <summary>
        /// Sets whether the specified dependency object
        /// is subscribed to the private manager or not.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="value">The value.</param>
        private static void SetIsSubscribed(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSubscribedProperty, value);
        }

        /// <summary>
        /// Identifies the IsSubscribed dependency property.
        /// </summary>
        private static readonly DependencyProperty IsSubscribedProperty =
            DependencyProperty.RegisterAttached("IsSubscribed", typeof(bool), typeof(ReaderboardEffect), new PropertyMetadata(false));

        #endregion

        #region HasEventsAttached DependencyProperty

        /// <summary>
        /// Gets whether the specified dependency object
        /// has events attached to it or not.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <returns>The value.</returns>
        private static bool GetHasEventsAttached(DependencyObject obj)
        {
            return (bool)obj.GetValue(HasEventsAttachedProperty);
        }

        /// <summary>
        /// Sets whether the specified dependency object
        /// has events attached to it or not.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="value">The value.</param>
        private static void SetHasEventsAttached(DependencyObject obj, bool value)
        {
            obj.SetValue(HasEventsAttachedProperty, value);
        }

        /// <summary>
        /// Identifies the HasEventsAttached dependency property.
        /// </summary>
        private static readonly DependencyProperty HasEventsAttachedProperty =
            DependencyProperty.RegisterAttached("HasEventsAttached", typeof(bool), typeof(ReaderboardEffect), new PropertyMetadata(false));

        #endregion

        #region OriginalProjection DependencyProperty

        /// <summary>
        /// Gets the original projection of the specified dependency object
        /// after the projection needed to apply the readerboard effect
        /// has been attached to it.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <returns>The original projection.</returns>
        private static Projection GetOriginalProjection(DependencyObject obj)
        {
            return (Projection)obj.GetValue(OriginalProjectionProperty);
        }

        /// <summary>
        /// Sets the original projection of the specified dependency object.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="value">The original projection.</param>
        private static void SetOriginalProjection(DependencyObject obj, Projection value)
        {
            obj.SetValue(OriginalProjectionProperty, value);
        }

        /// <summary>
        /// Identifies the OriginalProjection dependency property.
        /// </summary>
        private static readonly DependencyProperty OriginalProjectionProperty =
            DependencyProperty.RegisterAttached("OriginalProjection", typeof(Projection), typeof(ReaderboardEffect), new PropertyMetadata(null));

        #endregion

        #region OriginalOpacity DependencyProperty

        /// <summary>
        /// Gets the original opacity of the specified dependency 
        /// object before the readerboard effect is applied to it.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <returns>The original opacity.</returns>
        private static double GetOriginalOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(OriginalOpacityProperty);
        }

        /// <summary>
        /// Sets the original opacity of the specified 
        /// dependency object.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="value">The original opacity.</param>
        private static void SetOriginalOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(OriginalOpacityProperty, value);
        }

        /// <summary>
        /// Identifies the OriginalOpacity dependency property.
        /// </summary>
        private static readonly DependencyProperty OriginalOpacityProperty =
            DependencyProperty.RegisterAttached("OriginalOpacity", typeof(double), typeof(ReaderboardEffect), new PropertyMetadata(0.0));

        #endregion

        /// <summary>
        /// Called when an element gets resized.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        /// <remarks>
        /// Ideally, the Loaded event should be handled instead of
        /// the SizeChanged event. However, the Loaded event does not occur
        /// by the time the TransitionFrame tries to animate a forward in transition.
        /// Handling the SizeChanged event instead guarantees that
        /// the newly created FrameworkElements can be subscribed in time
        /// before the transition begins.
        /// </remarks>
        private static void Target_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SubscribeFrameworkElement((FrameworkElement)sender);
        }

        /// <summary>
        /// Called when an element gets unloaded.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private static void Target_Unloaded(object sender, RoutedEventArgs e)
        {
            UnsubscribeFrameworkElement((FrameworkElement)sender);
        }

        /// <summary>
        /// Throws an exception if the object sent as parameter is of a type
        /// that is included in the list of non-permitted types.
        /// </summary>
        /// <param name="obj">The object.</param>
        private static void CheckForTypePermission(object obj)
        {
            Type type = obj.GetType();

            if (NonPermittedTypes.Contains(type))
            {
                string message = string.Format(CultureInfo.InvariantCulture, "Objects of the type {0} cannot be flipped.", type);
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Compares two weak references targeting dependency objects
        /// to sort them based on their row index.
        /// </summary>
        /// <param name="x">The first weak reference.</param>
        /// <param name="y">The second weak reference.</param>
        /// <returns>
        /// 0 if both weak references target dependency objects with
        /// the same row index.
        /// 1 if the first reference targets a dependency 
        /// object with a greater row index.
        /// -1 if the second reference targets a dependency 
        /// object with a greater row index.       
        /// </returns>
        private static int SortReferencesByIndex(WeakReference x, WeakReference y)
        {
            DependencyObject targetX = x.Target as DependencyObject;
            DependencyObject targetY = y.Target as DependencyObject;

            if (targetX == null)
            {
                if (targetY == null)
                {
                    // If x is null and y is null, 
                    // they're equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, 
                    // y is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null and y is null,
                //  x is greater.
                if (targetY == null)
                {
                    return 1;
                }
                else
                {
                    int xIndex = GetRowIndex(targetX);
                    int yIndex = GetRowIndex(targetY);

                    return xIndex.CompareTo(yIndex);
                }
            }
        }

        /// <summary>
        /// Returns the set of weak references to the items 
        /// that must be animated.
        /// </summary>
        /// <returns>
        /// A set of weak references to items sorted by their row index.
        /// </returns>
        private static IList<WeakReference> GetTargetsToAnimate()
        {
            List<WeakReference> references;
            List<WeakReference> targets = new List<WeakReference>();
            PhoneApplicationPage page = null;
            PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;

            if (frame != null)
            {
                page = frame.Content as PhoneApplicationPage;
            }

            if (page == null)
            {
                return null;
            }

            if (!_pagesToReferences.TryGetValue(page, out references))
            {
                return null;
            }

            foreach (WeakReference r in references)
            {
                FrameworkElement target = r.Target as FrameworkElement;

                // If target is null, skip.
                if (target == null)
                {
                    continue;
                }

                // If target is not on the screen, skip.
                if (!IsOnScreen(target))
                {
                    continue;
                }

                ItemsControl itemsControl = r.Target as ItemsControl;
                LongListSelector longListSelector = r.Target as LongListSelector;
#if !WP7
                if (target is LongListMultiSelector || target is ListView)
                {
                    longListSelector = target.GetFirstLogicalChildByType<LongListSelector>(false);
                }
#endif

                if (itemsControl != null)
                {
                    // If the target is an ItemsControl, flip its items individually.
                    itemsControl.GetItemsInViewPort(targets);
                }
                else if (longListSelector != null)
                {
                    // If the target is a LongListSelector, flip its items individually.
#if WP7
                    ListBox child = longListSelector.GetFirstLogicalChildByType<ListBox>(false);

                    if (child != null)
                    {
                        child.GetItemsInViewPort(targets);
                    }
#else
                    longListSelector.GetItemsInViewPort(targets);
#endif
                }
                else
                {
                    // Else, flip the target as a whole.
                    targets.Add(r);
                }
            }

            return targets;
        }

        /// <summary>
        /// Subscribes an element to the private managers.
        /// </summary>
        /// <param name="target">The framework element.</param>
        private static void SubscribeFrameworkElement(FrameworkElement target)
        {
            if (!ReaderboardEffect.GetIsSubscribed(target))
            {
                // Find the parent page.
                PhoneApplicationPage page = target.GetParentByType<PhoneApplicationPage>();
                if (page == null)
                {
                    return;
                }

                ReaderboardEffect.SetParentPage(target, page);
                ReaderboardEffect.SetIsSubscribed(target, true);
            }
        }

        /// <summary>
        /// Unsubscribes an element from the private manager.
        /// </summary>
        /// <param name="target">The framework element.</param>
        private static void UnsubscribeFrameworkElement(FrameworkElement target)
        {
            // If element is subscribed, unsubscribe.
            if (ReaderboardEffect.GetIsSubscribed(target))
            {
                ReaderboardEffect.SetParentPage(target, null);
                ReaderboardEffect.SetIsSubscribed(target, false);
            }
        }

        /// <summary>
        /// Prepares a framework element to be flipped by adding a plane projection to it.
        /// </summary>
        /// <param name="root">The root visual.</param>
        /// <param name="element">The framework element.</param>
        private static bool TryAttachProjection(PhoneApplicationFrame root, FrameworkElement element)
        {
            // Cache original projection.
            ReaderboardEffect.SetOriginalProjection(element, element.Projection);

            // Attach projection.
            PlaneProjection projection = new PlaneProjection();
            element.Projection = projection;

            return true;
        }

        /// <summary>
        /// Restores the original projection of
        /// the targeted framework elements.
        /// </summary>
        private static void RestoreProjections()
        {
            if (_targets == null || !_pendingRestore)
            {
                return;
            }

            foreach (WeakReference r in _targets)
            {
                FrameworkElement element = r.Target as FrameworkElement;

                if (element == null)
                {
                    continue;
                }

                Projection projection = ReaderboardEffect.GetOriginalProjection(element);

                element.Projection = projection;
            }

            _pendingRestore = false;
        }

        /// <summary>
        /// Indicates whether the specified framework element
        /// is within the bounds of the application's root visual.
        /// </summary>
        /// <param name="element">The framework element.</param>
        /// <returns>
        /// True if the rectangular bounds of the framework element
        /// are completely outside the bounds of the application's root visual.
        /// </returns>
        private static bool IsOnScreen(FrameworkElement element)
        {
            PhoneApplicationFrame root = Application.Current.RootVisual as PhoneApplicationFrame;

            if (root == null)
            {
                return false;
            }

            GeneralTransform generalTransform;
            double height = root.GetUsefulHeight();
            double width = root.GetUsefulWidth();

            try
            {
                generalTransform = element.TransformToVisual(root);
            }
            catch (ArgumentException)
            {
                return false;
            }

            Rect bounds = new Rect(
                generalTransform.Transform(Origin),
                generalTransform.Transform(new Point(element.ActualWidth, element.ActualHeight)));

            bool isParentTransparent = false;
            IList<FrameworkElement> ancestors = element.GetVisualAncestors().ToList();

            if (ancestors != null)
            {
                for (int i = 0; i < ancestors.Count; i++)
                {
                    if (ancestors[i].Opacity <= 0.001)
                    {
                        isParentTransparent = true;
                        break;
                    }
                }
            }

            return (bounds.Bottom > 0) && (bounds.Top < height)
                && (bounds.Right > 0) && (bounds.Left < width)
                && !isParentTransparent;
        }

        /// <summary>
        /// Adds a set of animations corresponding to the 
        /// readerboard in effect.
        /// </summary>
        /// <param name="storyboard">
        /// The storyboard where the animations
        /// will be added.
        /// </param>
        private static void ComposeInStoryboard(Storyboard storyboard, TimeSpan? beginTime, bool noDelay)
        {
            int counter = 0;
            PhoneApplicationFrame root = Application.Current.RootVisual as PhoneApplicationFrame;

            foreach (WeakReference r in _targets)
            {
                FrameworkElement element = (FrameworkElement)r.Target;
                double originalOpacity = element.Opacity;
                ReaderboardEffect.SetOriginalOpacity(element, originalOpacity);

                // Hide the element until the storyboard is begun.
                element.Opacity = 0.0;

                if (!TryAttachProjection(root, element))
                {
                    continue;
                }

                TimeSpan initialDelay = beginTime.GetValueOrDefault();

                DoubleAnimationUsingKeyFrames doubleAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    BeginTime = noDelay ? (TimeSpan?)null : TimeSpan.FromMilliseconds(Delay * counter),
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = InAngle },
                        new DiscreteDoubleKeyFrame { KeyTime = initialDelay, Value = InAngle },
                        new EasingDoubleKeyFrame { KeyTime = initialDelay + TimeSpan.FromMilliseconds(InDuration), Value = 0, EasingFunction = ExponentialEaseOut }
                    }
                };

                Storyboard.SetTarget(doubleAnimation, element);
                Storyboard.SetTargetProperty(doubleAnimation, RotationXPropertyPath);
                storyboard.Children.Add(doubleAnimation);

                doubleAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    BeginTime = noDelay ? (TimeSpan?)null : TimeSpan.FromMilliseconds(Delay * counter),
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = 0 },
                        new DiscreteDoubleKeyFrame { KeyTime = initialDelay, Value = ReaderboardEffect.GetOriginalOpacity(element) }
                    }
                };

                Storyboard.SetTarget(doubleAnimation, element);
                Storyboard.SetTargetProperty(doubleAnimation, OpacityPropertyPath);
                storyboard.Children.Add(doubleAnimation);

                counter++;
            }
        }

        /// <summary>
        /// Adds a set of animations corresponding to the 
        /// readerboard out effect.
        /// </summary>
        /// <param name="storyboard">
        /// The storyboard where the animations
        /// will be added.
        /// </param>
        private static void ComposeOutStoryboard(Storyboard storyboard, bool noDelay)
        {
            int counter = 0;
            PhoneApplicationFrame root = Application.Current.RootVisual as PhoneApplicationFrame;

            foreach (WeakReference r in _targets)
            {
                FrameworkElement element = (FrameworkElement)r.Target;
                double originalOpacity = element.Opacity;
                ReaderboardEffect.SetOriginalOpacity(element, originalOpacity);

                if (!TryAttachProjection(root, element))
                {
                    continue;
                }

                DoubleAnimation doubleAnimation = new DoubleAnimation()
                {
                    Duration = TimeSpan.FromMilliseconds(OutDuration),
                    From = 0.0,
                    To = OutAngle,
                    BeginTime = noDelay ? (TimeSpan?)null : TimeSpan.FromMilliseconds(Delay * counter),
                    EasingFunction = ExponentialEaseIn
                };

                Storyboard.SetTarget(doubleAnimation, element);
                Storyboard.SetTargetProperty(doubleAnimation, RotationXPropertyPath);
                storyboard.Children.Add(doubleAnimation);

                doubleAnimation = new DoubleAnimation()
                {
                    Duration = TimeSpan.Zero,
                    From = originalOpacity,
                    To = 0.0,
                    BeginTime = TimeSpan.FromMilliseconds((noDelay ? 0 : (Delay * counter)) + OutDuration)
                };

                Storyboard.SetTarget(doubleAnimation, element);
                Storyboard.SetTargetProperty(doubleAnimation, OpacityPropertyPath);
                storyboard.Children.Add(doubleAnimation);

                counter++;
            }
        }

        /// <summary>
        /// Adds a set of animations corresponding to the 
        /// readerboard effect.
        /// </summary>
        /// <param name="storyboard">
        /// The storyboard where the animations
        /// will be added.</param>
        /// <param name="beginTime">
        /// The time at which the storyboard should begin.</param>
        /// <param name="mode">
        /// The mode of the readerboard effect.
        /// </param>
        internal static void ComposeStoryboard(Storyboard storyboard, TimeSpan? beginTime, ReaderboardTransitionMode mode, bool noDelay)
        {
            RestoreProjections();

            _targets = GetTargetsToAnimate();

            if (_targets == null)
            {
                return;
            }

            _pendingRestore = true;

            switch (mode)
            {
                case ReaderboardTransitionMode.In:
                    ComposeInStoryboard(storyboard, beginTime, noDelay);
                    break;
                case ReaderboardTransitionMode.Out:
                    ComposeOutStoryboard(storyboard, noDelay);
                    storyboard.BeginTime = beginTime;
                    break;
                default:
                    break;
            }

            storyboard.Completed += (s, e) =>
            {
                foreach (WeakReference r in _targets)
                {
                    FrameworkElement element = (FrameworkElement)r.Target;
                    double originalOpacity = ReaderboardEffect.GetOriginalOpacity(element);
                    element.Opacity = originalOpacity;
                }

                RestoreProjections();
            };
        }
    }
}