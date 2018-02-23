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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.Media;
using Android.Runtime;
using Prism.Media;
using Prism.Native;

namespace Prism.Android.Media
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeMediaPlaybackItem"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeMediaPlaybackItem))]
    public class MediaPlaybackItem : MediaPlayer, INativeMediaPlaybackItem, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnErrorListener, MediaPlayer.IOnPreparedListener
    {
        /// <summary>
        /// Occurs when the status of the item has changed.
        /// </summary>
        public event EventHandler StatusChanged;

        /// <summary>
        /// Gets the duration of the playback item.
        /// </summary>
        public new TimeSpan Duration
        {
            get { return Status < MediaPlayerStatus.Prepared || base.Duration < 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(base.Duration); }
        }

        /// <summary>
        /// Gets the error that has caused the item to enter the Error status.
        /// </summary>
        public new MediaError Error { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the item is open.
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Gets the status of the item.
        /// </summary>
        public MediaPlayerStatus Status
        {
            get { return status; }
            private set
            {
                if (value != status)
                {
                    status = value;
                    if (status == MediaPlayerStatus.Started)
                    {
                        ActivePlayers.Add(this);
                    }
                    else if (status == MediaPlayerStatus.Stopped || status == MediaPlayerStatus.Finished)
                    {
                        ActivePlayers.Remove(this);
                    }

                    StatusChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        private MediaPlayerStatus status;

        /// <summary>
        /// Gets a collection of the individual media tracks that contain the playback data.
        /// </summary>
        public ReadOnlyCollection<MediaTrack> Tracks
        {
            get
            {
                if (Status < MediaPlayerStatus.Prepared)
                {
                    return null;
                }

                var trackInfo = GetTrackInfo();
                if (trackInfo != null && (tracks == null || tracks.Count != trackInfo.Length))
                {
                    var trackArray = new MediaTrack[trackInfo.Length];
                    for (int i = 0; i < trackArray.Length; i++)
                    {
                        var track = trackInfo[i];
                        switch (track.TrackType)
                        {
                        case global::Android.Media.MediaTrackType.Audio:
                            trackArray[i] = new MediaTrack(string.Empty, Prism.Media.MediaTrackType.Audio, track.Language);
                            break;
                        case global::Android.Media.MediaTrackType.Video:
                            trackArray[i] = new MediaTrack(string.Empty, Prism.Media.MediaTrackType.Video, track.Language);
                            break;
                        case global::Android.Media.MediaTrackType.Timedtext:
                            trackArray[i] = new MediaTrack(string.Empty, Prism.Media.MediaTrackType.TimedMetadata, track.Language);
                            break;
                        case global::Android.Media.MediaTrackType.Unknown:
                            trackArray[i] = new MediaTrack(string.Empty, Prism.Media.MediaTrackType.Unknown, track.Language);
                            break;
                        default:
                            trackArray[i] = new MediaTrack(string.Empty, Prism.Media.MediaTrackType.Other, track.Language);
                            break;
                        }
                    }

                    tracks = new ReadOnlyCollection<MediaTrack>(trackArray);
                }

                return tracks;
            }
        }
        private ReadOnlyCollection<MediaTrack> tracks;

        /// <summary>
        /// Gets the URI of the playback item.
        /// </summary>
        public Uri Uri { get; }

        internal static HashSet<MediaPlayer> ActivePlayers { get; } = new HashSet<MediaPlayer>();

        private bool playAfterPrepare;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPlaybackItem"/> class.
        /// </summary>
        /// <param name="uri">The URI of the playback item..</param>
        public MediaPlaybackItem(Uri uri)
        {
            SetOnCompletionListener(this);
            SetOnErrorListener(this);
            SetOnPreparedListener(this);

            Uri = uri;
            Status = MediaPlayerStatus.Uninitialized;
        }

        /// <summary></summary>
        /// <param name="mp"></param>
        public void OnCompletion(MediaPlayer mp)
        {
            Status = MediaPlayerStatus.Finished;
        }

        /// <summary></summary>
        /// <param name="mp"></param>
        /// <param name="what"></param>
        /// <param name="extra"></param>
        public bool OnError(MediaPlayer mp, MediaError what, int extra)
        {
            Error = what;
            Status = MediaPlayerStatus.Error;
            return true;
        }

        /// <summary></summary>
        /// <param name="mp"></param>
        public void OnPrepared(MediaPlayer mp)
        {
            Status = MediaPlayerStatus.Prepared;
            IsOpen = true;

            if (CurrentPosition > 30)
            {
                SeekTo(0);
            }

            if (playAfterPrepare)
            {
                Start();
                playAfterPrepare = false;
            }
        }

        /// <summary>
        /// Pauses playback.
        /// </summary>
        public override void Pause()
        {
            playAfterPrepare = false;
            if (Status == MediaPlayerStatus.Started)
            {
                base.Pause();
                Status = MediaPlayerStatus.Paused;
            }
        }

        /// <summary>
        /// Prepares the player for playback synchronously.
        /// </summary>
        public override void Prepare()
        {
            if (Status == MediaPlayerStatus.Uninitialized || Status == MediaPlayerStatus.Stopped)
            {
                bool setSource = Status == MediaPlayerStatus.Uninitialized;
                Status = MediaPlayerStatus.Preparing;

                if (setSource)
                {
                    var fd = Application.GetAsset(Uri);
                    if (fd == null)
                    {
                        SetDataSource(Uri.OriginalString);
                    }
                    else
                    {
                        try
                        {
                            SetDataSource(fd.FileDescriptor, fd.StartOffset, fd.Length);
                        }
                        finally
                        {
                            fd.Close();
                        }
                    }
                }

                base.Prepare();
            }
        }

        /// <summary>
        /// Prepares the player for playback asynchronously.
        /// </summary>
        public override async void PrepareAsync()
        {
            if (Status == MediaPlayerStatus.Uninitialized || Status == MediaPlayerStatus.Stopped)
            {
                bool setSource = Status == MediaPlayerStatus.Uninitialized;
                Status = MediaPlayerStatus.Preparing;

                if (setSource)
                {
                    var fd = Application.GetAsset(Uri);
                    if (fd == null)
                    {
                        await SetDataSourceAsync(Uri.OriginalString);
                    }
                    else
                    {
                        try
                        {
                            await SetDataSourceAsync(fd.FileDescriptor, fd.StartOffset, fd.Length);
                        }
                        finally
                        {
                            fd.Close();
                        }
                    }
                }

                base.PrepareAsync();
            }
        }

        /// <summary>
        /// Resets the player to its uninitialized state.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            Status = MediaPlayerStatus.Uninitialized;
        }

        /// <summary>
        /// Starts or resumes playback.
        /// </summary>
        public override void Start()
        {System.Diagnostics.Debug.WriteLine(Uri + " " + status + " @ " + CurrentPosition);
            if (Status == MediaPlayerStatus.Uninitialized || Status == MediaPlayerStatus.Stopped)
            {
                Prepare();
            }
            else if (Status == MediaPlayerStatus.Preparing)
            {
                playAfterPrepare = true;
                return;
            }

            if (Status == MediaPlayerStatus.Prepared || Status == MediaPlayerStatus.Started ||
                Status == MediaPlayerStatus.Paused || Status == MediaPlayerStatus.Finished)
            {
                base.Start();
                Status = MediaPlayerStatus.Started;
            }
        }

        /// <summary>
        /// Stops playback.
        /// </summary>
        public override void Stop()
        {
            playAfterPrepare = false;
            if (Status == MediaPlayerStatus.Prepared || Status == MediaPlayerStatus.Started ||
                Status == MediaPlayerStatus.Paused || Status == MediaPlayerStatus.Finished)
            {
                base.Stop();
                Status = MediaPlayerStatus.Stopped;
            }
        }

        void INativeMediaPlaybackItem.Dispose()
        {
            Release();
        }
    }
}

