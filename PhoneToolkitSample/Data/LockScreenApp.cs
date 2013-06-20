using System;

namespace PhoneToolkitSample.Data
{
    public class LockScreenApp
    {
        public LockScreenApp(string name, Uri iconUri)
        {
            Name = name;
            IconUri = iconUri;
        }

        public string Name { get; set; }

        public Uri IconUri { get; set; }
    }
}
