/*
Copyright (C) 2016  Prism Framework Team

This file is part of the Prism Framework.

The Prism Framework is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

The Prism Framework is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/


using System;
using Android.Graphics;
using Android.Runtime;
using Prism.Native;

namespace Prism.Android.UI.Media
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeFontFamily"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeFontFamily))]
    public class FontFamily : INativeFontFamily
    {
        /// <summary>
        /// Gets the name of the font family.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the traits for the font.
        /// </summary>
        public PaintFlags Traits { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FontFamily"/> class.
        /// </summary>
        /// <param name="familyName">The family name of the font.</param>
        /// <param name="traits">Any special traits to assist in defining the font.</param>
        public FontFamily(string familyName, string traits)
        {
            Name = familyName;

            Traits = PaintFlags.AntiAlias | PaintFlags.DevKernText;
            if (traits != null)
            {
                string[] traitArray = traits.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < traitArray.Length; i++)
                {
                    PaintFlags trait;
                    if (Enum.TryParse(traitArray[i], true, out trait))
                    {
                        Traits |= trait;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="Typeface"/> for the font family.
        /// </summary>
        public Typeface GetTypeface()
        {
            string family;
            return Resources.FontFamilies.TryGetValue(Name, out family) ? Typeface.CreateFromFile(family) : Typeface.Default;
        }
    }
}