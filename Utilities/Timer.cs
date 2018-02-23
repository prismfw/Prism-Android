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
using Prism.Native;

namespace Prism.Android.Utilities
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeTimer"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTimer))]
    public class Timer : System.Timers.Timer, INativeTimer
    {
        /// <summary>
        /// Occurs when the number of milliseconds specified by <see cref="P:Interval"/> have passed.
        /// </summary>
        public new event EventHandler Elapsed;

        /// <summary>
        /// Gets a value indicating whether the timer is current running.
        /// </summary>
        public bool IsRunning
        {
            get { return base.Enabled; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer"/> class.
        /// </summary>
        public Timer()
        {
            AutoReset = false;
            base.Elapsed += (sender, e) =>
            {
                Elapsed(this, EventArgs.Empty);
            };
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void StartTimer()
        {
            Start();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void StopTimer()
        {
            Stop();
        }
    }
}

