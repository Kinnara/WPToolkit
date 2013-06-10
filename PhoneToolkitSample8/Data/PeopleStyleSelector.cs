using System.Windows;
using System.Windows.Controls;

namespace PhoneToolkitSample.Data
{
    public class PeopleStyleSelector : StyleSelector
    {
        public Style EnabledStyle { get; set; }

        public Style DisabledStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            return ((Person)item).FullName.StartsWith("D") ? DisabledStyle : EnabledStyle;
        }
    }
}
