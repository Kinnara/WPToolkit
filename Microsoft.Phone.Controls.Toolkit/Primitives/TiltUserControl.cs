// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Tiltable UserControl.
    /// </summary>
    public class TiltUserControl : UserControl
    {
        static TiltUserControl()
        {
            TiltEffect.TiltableItems.Add(typeof(TiltUserControl));
        }
    }
}