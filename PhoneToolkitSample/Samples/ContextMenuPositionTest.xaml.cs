using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace PhoneApp1
{
    public partial class AnotherContextMenuTestingPage : PhoneApplicationPage
    {
        public AnotherContextMenuTestingPage()
        {
            InitializeComponent();
            this.Loaded += AnotherContextMenuTestingPage_Loaded;
            int maxListItemCount = 25;
            int maxMenuItemCount = 14;
            for (int i = 0; i < maxListItemCount; i++)
            {
                ListBoxItem item = new ListBoxItem();

                int menuItemCount = i % maxMenuItemCount;
                if (i == 7)
                {
                    menuItemCount = 8;
                }
                else if (i == 24)
                {
                    item.Padding = new Thickness(0, 23, 0, 23);
                }

                item.Content = string.Format("#{0} ({1} menu items)", i, menuItemCount);
                lb.Items.Add(item);

                ContextMenu cm = new ContextMenu();
                for (int j = 0; j < menuItemCount; j++)
                {
                    MenuItem mi = new MenuItem();
                    mi.Header = "Menu# " + j;
                    cm.Items.Add(mi);
                }
                ContextMenuService.SetContextMenu(item, cm);
                item.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(item_Tap);
            }

            AddMoreDescriptionOnListItem(lb.Items[4] as ListBoxItem, "show below current element");
            AddMoreDescriptionOnListItem(lb.Items[7] as ListBoxItem, "show at current position");
            AddMoreDescriptionOnListItem(lb.Items[8] as ListBoxItem, "show below & expand upward");
            AddMoreDescriptionOnListItem(lb.Items[13] as ListBoxItem, "move to top & truncate at bottom");
            AddMoreDescriptionOnListItem(lb.Items[18] as ListBoxItem, "show above current element");
            AddMoreDescriptionOnListItem(lb.Items[24] as ListBoxItem, "show at current position");
        }

        void AnotherContextMenuTestingPage_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
@"Try Tap&Hold gesture or Click on below yellow items to see the different behaviors.

Click will try to place the ContextMenu aligned to the top or bottom of current element.
Tap&Hold will try above policy and if there is not enough to do so, it will try to place the menu at the current gesture point (Y).

See Items #7 and #24 for the difference.
"
);
        }

        void AddMoreDescriptionOnListItem(ListBoxItem item, string description)
        {
            item.Content = item.Content.ToString() + " : " + description;
            item.Foreground = new SolidColorBrush(Colors.Yellow);
        }

        void item_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ContextMenuService.GetContextMenu(sender as DependencyObject).IsOpen = true;
        }
    }
}