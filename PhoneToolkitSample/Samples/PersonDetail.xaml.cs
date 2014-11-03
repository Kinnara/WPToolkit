// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneToolkitSample.Data;
using System;
using System.Windows;
using System.Windows.Navigation;

namespace PhoneToolkitSample.Samples
{
    public partial class PersonDetail : BasePage
    {
        public PersonDetail()
        {
            InitializeComponent();

            quote.Text = 
                LoremIpsum.GetParagraph(4) + Environment.NewLine + Environment.NewLine + 
                LoremIpsum.GetParagraph(8) + Environment.NewLine + Environment.NewLine + 
                LoremIpsum.GetParagraph(6);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string idParam;
            if (NavigationContext.QueryString.TryGetValue("ID", out idParam))
            {
                int id = Int32.Parse(idParam);
                DataContext = AllPeople.Current[id];
            }
        }
    }
}