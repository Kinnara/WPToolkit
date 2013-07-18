// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Enables navigation transitions for
    /// <see cref="T:Microsoft.Phone.Controls.PhoneApplicationPage"/>s.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public class TransitionFrameSlim : PhoneApplicationFrame
    {
        #region Constants and Statics
        /// <summary>
        /// A single shared instance for setting BitmapCache on a visual.
        /// </summary>
        internal static readonly CacheMode BitmapCacheMode = new BitmapCache();
        #endregion

        #region Template Parts
        private ContentPresenter _contentPresenter;
        #endregion

        /// <summary>
        /// Indicates whether a navigation is forward.
        /// </summary>
        private bool _isForwardNavigation;

        /// <summary>
        /// A value indicating whether the old transition has completed and the
        /// new transition can begin.
        /// </summary>
        private bool _readyToTransitionToNewContent;

        /// <summary>
        /// A value indicating whether the exit transition is currently being performed.
        /// </summary>
        private bool _performingExitTransition;

        /// <summary>
        /// A value indicating whether the navigation is cancelled.
        /// </summary>
        private bool _navigationStopped;

        /// <summary>
        /// The transition to use to move in new content once the old transition
        /// is complete and ready for movement.
        /// </summary>
        private ITransition _storedNewTransition;

        /// <summary>
        /// The stored NavigationIn transition instance to use once the old
        /// transition is complete and ready for movement.
        /// </summary>
        private NavigationInTransition _storedNavigationInTransition;

        /// <summary>
        /// The transition to use to complete the old transition.
        /// </summary>
        private ITransition _storedOldTransition;

        /// <summary>
        /// The stored NavigationOut transition instance.
        /// </summary>
        private NavigationOutTransition _storedNavigationOutTransition;

        private NavigatingCancelEventArgs _deferredNavigation;

        private bool _deferredNavigationRequested;

        /// <summary>
        /// Initialzies a new instance of the TransitionFrameSlim class.
        /// </summary>
        public TransitionFrameSlim()
            : base()
        {
            Navigating += OnNavigating;
            NavigationStopped += OnNavigationStopped;
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application 
        /// code or internal processes (such as a rebuilding layout pass) call
        /// <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// In simplest terms, this means the method is called just before a UI 
        /// element displays in an application.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _contentPresenter = this.GetFirstLogicalChildByType<ContentPresenter>(false);
        }

        /// <summary>
        /// Handles the Navigating event of the frame, the immediate way to
        /// begin a transition out before the new page has loaded or had its
        /// layout pass.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            // If the current application is not the origin
            // and destination of the navigation, ignore it.
            // e.g. do not play a transition when the 
            // application gets deactivated because the shell
            // will animate the frame out automatically.
            if (!e.IsNavigationInitiator)
            {
                return;
            }

            if (_deferredNavigationRequested)
            {
                _deferredNavigationRequested = false;
                return;
            }

            if (e.NavigationMode != NavigationMode.Back && e.NavigationMode != NavigationMode.New)
            {
                return;
            }

            _isForwardNavigation = e.NavigationMode != NavigationMode.Back;

            var oldElement = Content as UIElement;
            if (oldElement == null)
            {
                return;
            }

            EnsureLastTransitionIsComplete();

            TransitionElement oldTransitionElement = null;
            NavigationOutTransition navigationOutTransition = null;
            ITransition oldTransition = null;

            navigationOutTransition = TransitionService.GetNavigationOutTransition(oldElement);

            if (navigationOutTransition != null)
            {
                oldTransitionElement = _isForwardNavigation ? navigationOutTransition.Forward : navigationOutTransition.Backward;
            }
            if (oldTransitionElement != null)
            {
                oldTransition = oldTransitionElement.GetTransition(oldElement);
            }
            if (oldTransition != null)
            {
                _deferredNavigation = e;
                e.Cancel = true;

                EnsureStoppedTransition(oldTransition);

                _storedNavigationOutTransition = navigationOutTransition;
                _storedOldTransition = oldTransition;
                oldTransition.Completed += OnExitTransitionCompleted;

                _performingExitTransition = true;

                PerformTransition(navigationOutTransition, _contentPresenter, oldTransition);

                PrepareContentPresenterForCompositor(_contentPresenter);
            }
        }

        /// <summary>
        /// Handles the NavigationStopped event of the frame. Set a value indicating 
        /// that the navigation is cancelled.
        /// </summary>
        private void OnNavigationStopped(object sender, NavigationEventArgs e)
        {
            if (_deferredNavigation != null)
            {
                return;
            }

            _navigationStopped = true;

            if (!_performingExitTransition)
            {
                RestoreContentPresenterInteractivity(_contentPresenter);
                _navigationStopped = false;
            }
        }

        /// <summary>
        /// Stops the last navigation transition if it's active and a new navigation occurs.
        /// </summary>
        private void EnsureLastTransitionIsComplete()
        {
            _readyToTransitionToNewContent = false;

            if (_performingExitTransition)
            {
                Debug.Assert(_storedOldTransition != null && _storedNavigationOutTransition != null);

                // If the app calls GoBack on NavigatedTo, we want the old content to be null
                // because you can't have the same content in two spots on the visual tree.
                //_oldContentPresenter.Content = null;

                _storedOldTransition.Stop();

                _storedNavigationOutTransition = null;
                _storedOldTransition = null;

                if (_storedNewTransition != null)
                {
                    _storedNewTransition.Stop();

                    _storedNewTransition = null;
                    _storedNavigationInTransition = null;
                }

                _performingExitTransition = false;

                DoDeferredNavigation();
            }
        }

        /// <summary>
        /// Handles the completion of the exit transition, automatically 
        /// continuing to bring in the new element's transition as well if it is
        /// ready.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnExitTransitionCompleted(object sender, EventArgs e)
        {
            _performingExitTransition = false;

            if (_navigationStopped)
            {
                // Restore the old content presenter's interactivity if the navigation is cancelled.
                CompleteTransition(_storedNavigationOutTransition, _contentPresenter, _storedOldTransition);
                _navigationStopped = false;
            }
            else
            {
                CompleteTransition(_storedNavigationOutTransition, /*_oldContentPresenter*/ null, _storedOldTransition);
            }
            
            _storedNavigationOutTransition = null;
            _storedOldTransition = null;

            DoDeferredNavigation();
        }

        /// <summary>
        /// Called when the value of the
        /// <see cref="P:System.Windows.Controls.ContentControl.Content"/>
        /// property changes.
        /// </summary>
        /// <param name="oldContent">The old <see cref="T:System.Object"/>.</param>
        /// <param name="newContent">The new <see cref="T:System.Object"/>.</param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            FrameworkElement newElement = newContent as FrameworkElement;

            // Require the appropriate template parts plus a new element to
            // transition to.
            if (Template == null || newElement == null)
            {
                return;
            }

            NavigationInTransition navigationInTransition = null;
            ITransition newTransition = null;

            if (newElement != null)
            {
                navigationInTransition = TransitionService.GetNavigationInTransition(newElement);
                TransitionElement newTransitionElement = null;
                if (navigationInTransition != null)
                {
                    newTransitionElement = _isForwardNavigation ? navigationInTransition.Forward : navigationInTransition.Backward;
                }
                if (newTransitionElement != null)
                {
                    newElement.UpdateLayout();

                    newTransition = newTransitionElement.GetTransition(newElement);
                    PrepareContentPresenterForCompositor(_contentPresenter);
                }
            }

            _readyToTransitionToNewContent = newTransition == null;

            if (_readyToTransitionToNewContent)
            {
                TransitionNewContent(newTransition, navigationInTransition);
            }
            else
            {
                _storedNewTransition = newTransition;
                _storedNavigationInTransition = navigationInTransition;

                RoutedEventHandler onNewElementLoaded = null;
                onNewElementLoaded = delegate
                {
                    newElement.Loaded -= onNewElementLoaded;
                    if (_storedNewTransition == newTransition)
                    {
                        AnimationHelper.InvokeOnSecondRendering(() =>
                        {
                            if (_storedNewTransition == newTransition)
                            {
                                TransitionNewContent(_storedNewTransition, _storedNavigationInTransition);
                            }
                        });
                    }
                };
                newElement.Loaded += onNewElementLoaded;
            }
        }

        /// <summary>
        /// Transitions the new <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        /// <param name="newTransition">The <see cref="T:Microsoft.Phone.Controls.ITransition"/> 
        /// for the new <see cref="T:System.Windows.UIElement"/>.</param>
        /// <param name="navigationInTransition">The <see cref="T:Microsoft.Phone.Controls.NavigationInTransition"/> 
        /// for the new <see cref="T:System.Windows.UIElement"/>.</param>
        private void TransitionNewContent(ITransition newTransition, NavigationInTransition navigationInTransition)
        {
            if (null == newTransition)
            {
                RestoreContentPresenterInteractivity(_contentPresenter);
                return;
            }

            EnsureStoppedTransition(newTransition);
            newTransition.Completed += delegate
            {
                CompleteTransition(navigationInTransition, _contentPresenter, newTransition);
            };

            _readyToTransitionToNewContent = false;
            _storedNavigationInTransition = null;
            _storedNewTransition = null;

            PerformTransition(navigationInTransition, _contentPresenter, newTransition);
        }

        private void DoDeferredNavigation()
        {
            if (_deferredNavigation != null)
            {
                if (_contentPresenter != null)
                {
                    _contentPresenter.Opacity = 0;
                }

                NavigatingCancelEventArgs pendingNavigation = _deferredNavigation;
                _deferredNavigation = null;

                _deferredNavigationRequested = true;

                if (pendingNavigation.NavigationMode == NavigationMode.Back)
                {
                    GoBack();
                }
                else
                {
                    Navigate(pendingNavigation.Uri);
                }
            }
        }

        /// <summary>
        /// This checks to make sure that, if the transition not be in the clock
        /// state of Stopped, that is will be stopped.
        /// </summary>
        /// <param name="transition">The transition instance.</param>
        private static void EnsureStoppedTransition(ITransition transition)
        {
            if (transition != null && transition.GetCurrentState() != ClockState.Stopped)
            {
                transition.Stop();
            }
        }

        /// <summary>
        /// Performs a transition when given the appropriate components,
        /// includes calling the appropriate start event and ensuring opacity
        /// on the content presenter.
        /// </summary>
        /// <param name="navigationTransition">The navigation transition.</param>
        /// <param name="presenter">The content presenter.</param>
        /// <param name="transition">The transition instance.</param>
        private static void PerformTransition(NavigationTransition navigationTransition, ContentPresenter presenter, ITransition transition)
        {
            if (navigationTransition != null)
            {
                navigationTransition.OnBeginTransition();
            }
            if (presenter != null && presenter.Opacity != 1)
            {
                presenter.Opacity = 1;
            }
            if (transition != null)
            {
                transition.Begin();
            }
        }

        /// <summary>
        /// Completes a transition operation by stopping it, restoring 
        /// interactivity, and then firing the OnEndTransition event.
        /// </summary>
        /// <param name="navigationTransition">The navigation transition.</param>
        /// <param name="presenter">The content presenter.</param>
        /// <param name="transition">The transition instance.</param>
        private static void CompleteTransition(NavigationTransition navigationTransition, ContentPresenter presenter, ITransition transition)
        {
            if (transition != null)
            {
                transition.Stop();
            }

            RestoreContentPresenterInteractivity(presenter);

            if (navigationTransition != null)
            {
                navigationTransition.OnEndTransition();
            }
        }

        /// <summary>
        /// Updates the content presenter for off-thread compositing for the
        /// transition animation. Also disables interactivity on it to prevent
        /// accidental touches.
        /// </summary>
        /// <param name="presenter">The content presenter instance.</param>
        /// <param name="applyBitmapCache">A value indicating whether to apply
        /// a bitmap cache.</param>
        private static void PrepareContentPresenterForCompositor(ContentPresenter presenter, bool applyBitmapCache = true)
        {
            if (presenter != null)
            {
                if (applyBitmapCache)
                {
                    presenter.CacheMode = BitmapCacheMode;
                }
                presenter.IsHitTestVisible = false;
            }
        }

        /// <summary>
        /// Restores the interactivity for the presenter post-animation, also
        /// removes the BitmapCache value.
        /// </summary>
        /// <param name="presenter">The content presenter instance.</param>
        private static void RestoreContentPresenterInteractivity(ContentPresenter presenter)
        {
            if (presenter != null)
            {
                presenter.CacheMode = null;

                if (presenter.Opacity != 1)
                {
                    presenter.Opacity = 1;
                }

                presenter.IsHitTestVisible = true;
            }
        }
    }
}
