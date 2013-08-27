using Microsoft.Phone.Controls;
using System;

namespace PhoneToolkitSample.Samples
{
    public partial class ApplicationBarSubmenuSample : BasePage
    {
        private ApplicationBarSubmenu _respondSubmenu;
        private ApplicationBarSubmenu _prioritySubmenu;

        public ApplicationBarSubmenuSample()
        {
            InitializeComponent();

            _respondSubmenu = new ApplicationBarSubmenu
            {
                Items =
                {
                    CreateItem("reply"),
                    CreateItem("reply all"),
                    CreateItem("forward"),
                }
            };

            _prioritySubmenu = new ApplicationBarSubmenu
            {
                Items =
                {
                    CreateItem("high"),
                    CreateItem("normal"),
                    CreateItem("low"),
                }
            };
        }

        private void AppBarRespond_Click(object sender, EventArgs e)
        {
            _respondSubmenu.Show();
        }

        private void AppBarPriority_Click(object sender, EventArgs e)
        {
            _prioritySubmenu.Show();
        }

        private ApplicationBarSubmenuItem CreateItem(object header)
        {
            var item = new ApplicationBarSubmenuItem { Header = header };
            item.Click += delegate
            {
                LastSelectionTextBlock.Text = header.ToString();
            };
            return item;
        }
    }
}