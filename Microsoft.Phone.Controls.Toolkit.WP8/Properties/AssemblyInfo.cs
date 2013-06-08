// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Microsoft.Phone.Controls.Toolkit")]
[assembly: AssemblyDescription("Windows Phone Toolkit")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyProduct("Microsoft® Windows Phone")]
[assembly: AssemblyCopyright("© Microsoft Corporation.  All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("486642f0-ab94-4f28-b49c-106ca3134239")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("8.0.1.0")]
[assembly: AssemblyFileVersion("8.0.1.0")]

[assembly: CLSCompliant(false)] // IApplicationBar is not CLS-compliant, but its use matches the type of the platform's PhoneApplicationPage.ApplicationBar property
[assembly: NeutralResourcesLanguageAttribute("en-US")]

[assembly: XmlnsPrefix("clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit", "toolkit")]
[assembly: XmlnsDefinition("clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit", "Microsoft.Phone.Controls")]
[assembly: XmlnsPrefix("clr-namespace:Microsoft.Phone.Controls.Primitives;assembly=Microsoft.Phone.Controls.Toolkit", "toolkitPrimitives")]
[assembly: XmlnsDefinition("clr-namespace:Microsoft.Phone.Controls.Primitives;assembly=Microsoft.Phone.Controls.Toolkit", "Microsoft.Phone.Controls.Primitives")]