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
using Android.Runtime;
using Android.Text;
using Android.Text.Method;
using Android.Views.InputMethods;
using Prism.Input;
using Prism.Native;
using Prism.UI;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativePasswordBox"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativePasswordBox))]
    public class PasswordBox : TextEntryBase, INativePasswordBox
    {
        /// <summary>
        /// Occurs when the value of the <see cref="P:Password"/> property has changed.
        /// </summary>
        public event EventHandler PasswordChanged;

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
                    OnPropertyChanged(Prism.UI.Controls.PasswordBox.ActionKeyTypeProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of text that the user is expected to input.
        /// </summary>
        public new InputType InputType
        {
            get { return inputType; }
            set
            {
                var inputTypes = value.GetInputTypes() | (value == InputType.Number ? InputTypes.NumberVariationPassword : InputTypes.TextVariationPassword);
                if (inputTypes != base.InputType)
                {
                    inputType = value;
                    base.InputType = inputTypes;
                    OnPropertyChanged(Prism.UI.Controls.PasswordBox.InputTypeProperty);
                }
            }
        }
        private InputType inputType;

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
                    OnPropertyChanged(Prism.UI.Controls.PasswordBox.MaxLengthProperty);

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
        /// Gets or sets the password value of the control.
        /// </summary>
        public string Password
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

                    OnPropertyChanged(Prism.UI.Controls.PasswordBox.PasswordProperty);
                }
            }
        }

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
                    OnPropertyChanged(Prism.UI.Controls.PasswordBox.PlaceholderProperty);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordBox"/> class.
        /// </summary>
        public PasswordBox()
        {
            base.InputType = InputTypes.TextVariationPassword;
            TransformationMethod = new PasswordTransformationMethod();
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

            if (text.Length() > 0 || lengthBefore > 0)
            {
                OnPropertyChanged(Prism.UI.Controls.PasswordBox.PasswordProperty);
                PasswordChanged(this, EventArgs.Empty);
            }
        }
    }
}

