using System;
using System.Collections.ObjectModel;

namespace PhoneToolkitSample.Data
{
    public class LockScreenAppCollection : ObservableCollection<LockScreenApp>
    {
        public LockScreenAppCollection()
        {
            Add(new LockScreenApp("none", new Uri("/Images/lock.add.png", UriKind.Relative)));
            Add(new LockScreenApp("Email", new Uri("/Images/lock.email.png", UriKind.Relative)));
            Add(new LockScreenApp("Messaging", new Uri("/Images/lock.messaging.png", UriKind.Relative)));
            Add(new LockScreenApp("Phone", new Uri("/Images/lock.phone.png", UriKind.Relative)));
        }
    }
}
