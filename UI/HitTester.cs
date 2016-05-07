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
using Android.Runtime;
using Android.Views;
using Prism.Native;

namespace Prism.Android.UI
{
    /// <summary>
    /// Represents a touch event listener that determines whether to block touches for <see cref="INativeVisual"/>
    /// objects based on the values of their <see cref="P:IsHitTestVisible"/> properties. 
    /// </summary>
    public class HitTester : Java.Lang.Object, View.IOnTouchListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HitTester"/> class.
        /// </summary>
        public HitTester()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HitTester"/> class.
        /// </summary>
        /// <param name="handle">An <see cref="IntPtr"/> containing a Java Native Interface (JNI) object reference.</param>
        /// <param name="transfer">A <see cref="JniHandleOwnership"/> indicating how to handle <paramref name="handle"/>.</param>
        public HitTester(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

        /// <summary></summary>
        /// <param name="v"></param>
        /// <param name="e"></param>
        public bool OnTouch(View v, MotionEvent e)
        {
            var visual = (v as INativeVisual) ?? v.GetParent<INativeVisual>();
            return visual != null && !visual.IsHitTestVisible;
        }
    }
}

