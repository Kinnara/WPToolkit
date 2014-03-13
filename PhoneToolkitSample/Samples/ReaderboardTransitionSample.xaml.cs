using Microsoft.Phone.Controls;
using System;
using System.Windows;

namespace PhoneToolkitSample.Samples
{
    public partial class ReaderboardTransitionSample : BasePage
    {
        public ReaderboardTransitionSample()
        {
            InitializeComponent();
        }

        private void Readerboard_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Samples/ReaderboardEffectSample1.xaml", UriKind.Relative));
        }

        private void ReaderboardNoDelay_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Samples/ReaderboardEffectSample2.xaml", UriKind.Relative));
        }        
    }
}