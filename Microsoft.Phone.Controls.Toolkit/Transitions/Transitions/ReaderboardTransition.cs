using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides readerboard <see cref="T:Microsoft.Phone.Controls.ITransition"/>s.
    /// </summary>
    public class ReaderboardTransition : TransitionElement
    {
        /// <summary>
        /// The
        /// <see cref="T:System.Windows.DependencyProperty"/>
        /// for the
        /// <see cref="T:Microsoft.Phone.Controls.ReaderboardTransitionMode"/>.
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(ReaderboardTransitionMode), typeof(ReaderboardTransition), null);

        /// <summary>
        /// The <see cref="T:Microsoft.Phone.Controls.ReaderboardTransitionMode"/>.
        /// </summary>
        public ReaderboardTransitionMode Mode
        {
            get
            {
                return (ReaderboardTransitionMode)GetValue(ModeProperty);
            }
            set
            {
                SetValue(ModeProperty, value);
            }
        }

        public static readonly DependencyProperty NoDelayProperty =
            DependencyProperty.Register("NoDelay", typeof(bool), typeof(ReaderboardTransition), null);

        public bool NoDelay
        {
            get
            {
                return (bool)GetValue(NoDelayProperty);
            }
            set
            {
                SetValue(NoDelayProperty, value);
            }
        }

        /// <summary>
        /// The
        /// <see cref="T:System.Windows.DependencyProperty"/>
        /// for the time at which the transition should begin.
        /// </summary>
        public static readonly DependencyProperty BeginTimeProperty =
            DependencyProperty.Register("BeginTime", typeof(TimeSpan?), typeof(ReaderboardTransition), new PropertyMetadata(TimeSpan.Zero));

        /// <summary>
        /// The time at which the transition should begin.
        /// </summary>
        public TimeSpan? BeginTime
        {
            get
            {
                return (TimeSpan?)GetValue(BeginTimeProperty);
            }
            set
            {
                SetValue(BeginTimeProperty, value);
            }
        }

        /// <summary>
        /// Creates a new
        /// <see cref="T:Microsoft.Phone.Controls.ITransition"/>
        /// for a
        /// <see cref="T:System.Windows.UIElement"/>.
        /// Saves and clears the existing
        /// <see cref="F:System.Windows.UIElement.ProjectionProperty"/>
        /// value before the start of the transition, then restores it after it is stopped or completed.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <returns>The <see cref="T:Microsoft.Phone.Controls.ITransition"/>.</returns>
        public override ITransition GetTransition(UIElement element)
        {
            return Readerboard(element, Mode, NoDelay, BeginTime);
        }

        private static ITransition Readerboard(UIElement element, ReaderboardTransitionMode readerboardTransitionMode, bool noDelay, TimeSpan? beginTime)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (!Enum.IsDefined(typeof(ReaderboardTransitionMode), readerboardTransitionMode))
            {
                throw new ArgumentOutOfRangeException("readerboardTransitionMode");
            }
            return new CustomTransition(element, new Storyboard(), readerboardTransitionMode, noDelay, beginTime);
        }

        private class CustomTransition : Transition
        {
            private ReaderboardTransitionMode _mode;

            private bool _noDelay;

            private TimeSpan? _beginTime;

            public CustomTransition(UIElement element, Storyboard storyboard, ReaderboardTransitionMode mode, bool noDelay, TimeSpan? beginTime)
                : base(element, storyboard)
            {
                _mode = mode;
                _noDelay = noDelay;
                _beginTime = beginTime;
            }

            public override void Begin()
            {
                ReaderboardEffect.ComposeStoryboard(Storyboard, _beginTime, _mode, _noDelay);
                base.Begin();
            }
        }
    }
}
