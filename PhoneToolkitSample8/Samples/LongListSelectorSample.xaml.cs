// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using PhoneToolkitSample.Data;
using System.Globalization;

namespace PhoneToolkitSample.Samples
{
    public partial class LongListSelectorSample : PhoneApplicationPage
    {
        public List<AlphaKeyGroup<Person>> Buddies { get; private set; }
        public List<AlphaKeyGroup<Movie>> Movies { get; private set; }

        public LongListSelectorSample()
        {
            InitializeComponent();
            DataContext = this;

            Buddies = AlphaKeyGroup<Person>.CreateGroups(AllPeople.Current, CultureInfo.CurrentUICulture, (p) => { return p.FirstName; }, true);
            buddies.SelectionChanged += PersonSelectionChanged;

            LoadLinqMovies();

            MessageBox.Show(
@"The LongListSelector is now built into Windows Phone 8, for better performance and new features including grid layout mode and ""sticky headers"".

This sample and the sample code demonstrates how to use the new, improved LongListSelector.");
        }

        private void LoadLinqMovies()
        {
            List<Movie> movies = new List<Movie>();

            for (int i = 0; i < 50; ++i)
            {
                movies.Add(Movie.CreateRandom());
            }

            var moviesByCategory = from movie in movies
                                   group movie by movie.Category into c
                                   orderby c.Key
                                   select new AlphaKeyGroup<Movie>(c);

            Movies = moviesByCategory.ToList();
        }

        private void PersonSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Person person = buddies.SelectedItem as Person;
            if (person != null)
            {
                NavigationService.Navigate(new Uri("/Samples/PersonDetail.xaml?ID=" + person.ID, UriKind.Relative));
            }
        }   
    }
}