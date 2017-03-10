/*
Copyright (C) 2017  Prism Framework Team

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
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Runtime;
using Prism.Native;
using Prism.UI;

namespace Prism.Android.UI
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeAlert"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeAlert))]
    public class Alert : AlertDialog, INativeAlert
    {
        /// <summary>
        /// Gets the number of buttons that have been added to the alert.
        /// </summary>
        public int ButtonCount
        {
            get { return buttons.Count; }
        }

        /// <summary>
        /// Does nothing on Android.
        /// </summary>
        public int CancelButtonIndex { get; set; }

        /// <summary>
        /// Does nothing on Android.
        /// </summary>
        public int DefaultButtonIndex { get; set; }

        /// <summary>
        /// Gets the message text for the alert.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the title text for the alert.
        /// </summary>
        public string Title { get; }

        private readonly List<AlertButton> buttons;

        /// <summary>
        /// Initializes a new instance of the <see cref="Alert"/> class.
        /// </summary>
        /// <param name="message">The message text for the alert.</param>
        /// <param name="title">The title text for the alert.</param>
        public Alert(string message, string title)
            : base(Application.MainActivity)
        {
            SetMessage(Message = message);
            SetTitle(Title = title);
            buttons = new List<AlertButton>();
        }

        /// <summary>
        /// Adds the specified <see cref="AlertButton"/> to the alert.
        /// </summary>
        /// <param name="button">The button to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="button"/> is <c>null</c>.</exception>
        public void AddButton(AlertButton button)
        {
            if (button == null)
            {
                throw new ArgumentNullException(nameof(button));
            }

            buttons.Add(button);
            if (buttons.Count == 1)
            {
                SetButton(button.Title, OnButtonClicked);
            }
            else if (buttons.Count == 2)
            {
                SetButton2(button.Title, OnButtonClicked);
            }
            else if (buttons.Count == 3)
            {
                SetButton3(button.Title, OnButtonClicked);
            }
        }

        /// <summary>
        /// Gets the button at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the button to retrieve.</param>
        /// <returns>The <see cref="AlertButton"/> at the specified index -or- <c>null</c> if there is no button.</returns>
        public new AlertButton GetButton(int index)
        {
            return buttons.ElementAtOrDefault(index);
        }

        private void OnButtonClicked(object sender, DialogClickEventArgs e)
        {
            var b = buttons[Math.Abs(e.Which) -1];
            if (b.Action != null)
            {
                b.Action.Invoke(b);
            }
        }
    }
}

