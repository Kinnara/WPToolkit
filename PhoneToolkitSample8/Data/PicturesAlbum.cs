// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneToolkitSample.Data
{
    /// <summary>
    /// Sample class for picture based samples
    /// </summary>
    public class Picture
    {
        /// <summary>
        /// Gets the City name associated to the picture
        /// </summary>
        public string City { get; private set; }
        /// <summary>
        /// Gets the local Url of the picture
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Constructs a Picture object
        /// </summary>
        /// <param name="city"></param>
        public Picture(string city)
        {
            this.City = city;
            this.Url = "/Images/" + city + ".jpg";
        }
    }

    /// <summary>
    /// Sample pictures album
    /// </summary>
    public class PicturesAlbum : ObservableCollection<Picture>
    {
        /// <summary>
        /// Constructs a PicturesAlbum object
        /// </summary>
        public PicturesAlbum()
        {
            this.Add(new Picture("Copenhagen"));
            this.Add(new Picture("Mürren"));
            this.Add(new Picture("Neuschwanstein"));
            this.Add(new Picture("Paris"));
            this.Add(new Picture("Seattle"));
            this.Add(new Picture("Venice"));
        }
    }
}
