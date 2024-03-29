/*
Copyright (C) 2018  Prism Framework Team

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
using Android.Runtime;
using Android.Text;
using Android.Views.InputMethods;
using Prism.Input;
using Prism.Native;
using Prism.UI;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeTextBox"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTextBox))]
    public class TextBox : TextEntryBase, INativeTextBox
    {
        /// <summary>
        /// Occurs when the value of the <see cref="P:Text"/> property has changed.
        /// </summary>
        public new event EventHandler<Prism.UI.Controls.TextChangedEventArgs> TextChanged;

        /// <summary>
        /// Gets or sets the type of action key to use for the soft keyboard when the control has focus.
        /// </summary>
        public ActionKeyType ActionKeyType
        {
            get { return ImeOptions.GetActionKeyType(); }
            set
            {
                var action = value.GetImeAction();
                if (action != ImeOptions)
                {
                    ImeOptions = action;
                    OnPropertyChanged(Prism.UI.Controls.TextBox.ActionKeyTypeProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of text that the user is expected to input.
        /// </summary>
        public new InputType InputType
        {
            get { return base.InputType.GetInputType(); }
            set
            {
                if (value != base.InputType.GetInputType())
                {
                    base.InputType = value.GetInputTypes();
                    OnPropertyChanged(Prism.UI.Controls.TextBox.InputTypeProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of characters that are allowed to be entered into the control.
        /// A value of 0 means there is no limit.
        /// </summary>
        public int MaxLength
        {
            get { return maxLength; }
            set
            {
                if (value != maxLength)
                {
                    maxLength = value;
                    OnPropertyChanged(Prism.UI.Controls.TextBox.MaxLengthProperty);

                    if (maxLength == 0)
                    {
                        SetFilters(new IInputFilter[0]);
                    }
                    else
                    {
                        SetFilters(new[] { new InputFilterLengthFilter(maxLength) });
                        if (Text != null && Text.Length > maxLength)
                        {
                            Text = Text.Substring(0, maxLength);
                        }
                    }
                }
            }
        }
        private int maxLength;

        /// <summary>
        /// Gets or sets the text to display when the control does not have a value.
        /// </summary>
        public string Placeholder
        {
            get { return Hint; }
            set
            {
                if (value != Hint)
                {
                    Hint = value;
                    OnPropertyChanged(Prism.UI.Controls.TextBox.PlaceholderProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the text of the control.
        /// </summary>
        public new string Text
        {
            get { return base.Text; }
            set
            {
                if (value != base.Text)
                {
                    base.Text = value;

                    if (IsFocused)
                    {
                        SetSelection(base.Text.Length);
                    }

                    OnPropertyChanged(Prism.UI.Controls.TextBox.TextProperty);
                    currentValue = base.Text;
                }
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the text within the control.
        /// </summary>
        public new Prism.UI.TextAlignment TextAlignment
        {
            get { return Gravity.GetTextAlignment(); }
            set
            {
                if (value != Gravity.GetTextAlignment())
                {
                    Gravity = value.GetGravity();
                    OnPropertyChanged(Prism.UI.Controls.TextBox.TextAlignmentProperty);
                }
            }
        }

        private string currentValue = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBox"/> class.
        /// </summary>
        public TextBox()
        {
            SetSingleLine();
        }

        /// <summary>
        /// Called when an attached input method calls <see cref="M:InputConnection.performEditorAction(int)"/> for this text view.
        /// </summary>
        /// <param name="actionCode">The code of the action being performed.</param>
        public override void OnEditorAction(ImeAction actionCode)
        {
            var e = new HandledEventArgs();
            OnActionKeyPressed(e);
            if (!e.IsHandled)
            {
                Unfocus();
            }
        }

        /// <summary>
        /// This method is called when the text is changed.
        /// </summary>
        /// <param name="text">The text the TextView is displaying.</param>
        /// <param name="start"></param>
        /// <param name="lengthBefore"></param>
        /// <param name="lengthAfter"></param>
        protected override void OnTextChanged(Java.Lang.ICharSequence text, int start, int lengthBefore, int lengthAfter)
        {
            base.OnTextChanged(text, start, lengthBefore, lengthAfter);

            if (currentValue != base.Text)
            {
                OnPropertyChanged(Prism.UI.Controls.TextBox.TextProperty);
                TextChanged(this, new Prism.UI.Controls.TextChangedEventArgs(currentValue, base.Text));
                currentValue = base.Text;
            }
        }
    }
}

