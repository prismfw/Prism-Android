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


using Android.Media;

namespace Prism.Android.Media
{
    /// <summary>
    /// Describes the status of a <see cref="MediaPlayer"/>.
    /// </summary>
    public enum MediaPlayerStatus
    {
        /// <summary>
        /// The player has been created but not yet prepared.
        /// </summary>
        Uninitialized = 0,
        /// <summary>
        /// The player has encountered an error.
        /// </summary>
        Error = 1,
        /// <summary>
        /// The player is being prepared.
        /// </summary>
        Preparing = 2,
        /// <summary>
        /// The player has been prepared and is ready for playing.
        /// </summary>
        Prepared = 3,
        /// <summary>
        /// The player has been started and is currently playing.
        /// </summary>
        Started = 4,
        /// <summary>
        /// The player has been paused.
        /// </summary>
        Paused = 5,
        /// <summary>
        /// The player has been stopped.
        /// </summary>
        Stopped = 6,
        /// <summary>
        /// The player has completed playback.
        /// </summary>
        Finished = 7
    }
}

