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
using Android.Media;
using Android.OS;
using Android.Runtime;
using Prism.Native;

namespace Prism.Android.Media
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeAudioPlayer"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeAudioPlayer))]
    public class AudioPlayer : MediaPlayer, INativeAudioPlayer, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnErrorListener, MediaPlayer.IOnInfoListener, MediaPlayer.IOnPreparedListener
    {
        /// <summary>
        /// Occurs when there is an error during loading or playing of the audio track.
        /// </summary>
        public event EventHandler<Prism.ErrorEventArgs> AudioFailed;

        /// <summary>
        /// Occurs when buffering of the audio track has finished.
        /// </summary>
        public event EventHandler BufferingEnded;

        /// <summary>
        /// Occurs when buffering of the audio track has begun.
        /// </summary>
        public event EventHandler BufferingStarted;

        /// <summary>
        /// Occurs when playback of the audio track has finished.
        /// </summary>
        public event EventHandler PlaybackEnded;

        /// <summary>
        /// Occurs when playback of the audio track has begun.
        /// </summary>
        public event EventHandler PlaybackStarted;

        /// <summary>
        /// Gets or sets a value indicating whether playback of the audio track should automatically begin once buffering is finished.
        /// </summary>
        public bool AutoPlay { get; set; }

        /// <summary>
        /// Gets the duration of the audio track.
        /// </summary>
        public new TimeSpan Duration
        {
            get { return base.Duration < 0 ? TimeSpan.MinValue : TimeSpan.FromMilliseconds(base.Duration); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the audio track will automatically begin playing again once it has finished.
        /// </summary>
        public bool IsLooping
        {
            get { return isLooping; }
            set { base.Looping = isLooping = value; }
        }
        private bool isLooping;

        /// <summary>
        /// Gets or sets a coefficient of the rate at which the audio track is played back.
        /// </summary>
        public double PlaybackRate
        {
            get { return Build.VERSION.SdkInt < BuildVersionCodes.M || base.PlaybackParams == null ? 1 : base.PlaybackParams.Speed; }
            set
            {
                if (value != playbackRate)
                {
                    playbackRate = value;
                    if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                    {
                        Prism.Utilities.Logger.Warn("Changing playback rate requires Android 6.0 (Marshmallow) or later.");
                    }
                    else
                    {
                        if (base.PlaybackParams == null)
                        {
                            base.PlaybackParams = new PlaybackParams();
                            base.PlaybackParams.AllowDefaults();
                        }
                        base.PlaybackParams.SetSpeed((float)playbackRate);
                    }
                }
            }
        }
        private double playbackRate = 1;

        /// <summary>
        /// Gets or sets the position of the audio track.
        /// </summary>
        public TimeSpan Position
        {
            get { return isPrepared ? TimeSpan.FromMilliseconds(base.CurrentPosition) : position; }
            set
            {
                if (isPrepared)
                {
                    base.SeekTo((int)position.TotalMilliseconds);
                }
                else
                {
                    position = value;
                }
            }
        }
        private TimeSpan position;

        /// <summary>
        /// Gets or sets the volume of the audio track.
        /// </summary>
        public double Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                base.SetVolume((float)volume, (float)volume);
            }
        }
        private double volume;

        private bool isPrepared;
        private bool playAfterPrepare;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioPlayer"/> class.
        /// </summary>
        public AudioPlayer()
        {
            SetOnCompletionListener(this);
            SetOnErrorListener(this);
            SetOnInfoListener(this);
            SetOnPreparedListener(this);
        }

        /// <summary></summary>
        /// <param name="mp"></param>
        public void OnCompletion(MediaPlayer mp)
        {
            if (!isLooping)
            {
                PlaybackEnded(this, EventArgs.Empty);
            }
        }

        /// <summary></summary>
        /// <param name="mp"></param>
        /// <param name="what"></param>
        /// <param name="extra"></param>
        public bool OnError(MediaPlayer mp, MediaError what, int extra)
        {
            AudioFailed(this, new Prism.ErrorEventArgs(new Exception(what.ToString())));
            return true;
        }

        /// <summary></summary>
        /// <param name="mp"></param>
        /// <param name="what"></param>
        /// <param name="extra"></param>
        public bool OnInfo(MediaPlayer mp, MediaInfo what, int extra)
        {
            if (what == MediaInfo.BufferingStart)
            {
                BufferingStarted(this, EventArgs.Empty);
            }
            else if (what == MediaInfo.BufferingEnd)
            {
                BufferingEnded(this, EventArgs.Empty);
            }

            return true;
        }

        /// <summary></summary>
        /// <param name="mp"></param>
        public void OnPrepared(MediaPlayer mp)
        {
            base.Looping = isLooping;
            base.SetVolume((float)volume, (float)volume);
            base.SeekTo((int)position.TotalMilliseconds);
            position = TimeSpan.Zero;
            isPrepared = true;

            if (AutoPlay || playAfterPrepare)
            {
                Play();
            }
        }

        /// <summary>
        /// Loads the audio track from the file at the specified location.
        /// </summary>
        /// <param name="source">The URI of the source file for the audio track.</param>
        public async void Open(Uri source)
        {
            isPrepared = false;
            playAfterPrepare = false;
            base.Reset();

            try
            {
                var fd = Application.GetAsset(source);
                if (fd == null)
                {
                    await base.SetDataSourceAsync(source.OriginalString);
                }
                else
                {
                    await base.SetDataSourceAsync(fd.FileDescriptor);
                    fd.Close();
                }

                base.PrepareAsync();
            }
            catch (Java.IO.IOException e)
            {
                AudioFailed(this, new Prism.ErrorEventArgs(e));
            }
        }

        /// <summary>
        /// Pauses playback of the audio track.
        /// </summary>
        public new void Pause()
        {
            if (!isPrepared)
            {
                playAfterPrepare = false;
            }
            else if (base.IsPlaying)
            {
                base.Pause();
            }
        }

        /// <summary>
        /// Starts or resumes playback of the audio track.
        /// </summary>
        public void Play()
        {
            if (!isPrepared)
            {
                playAfterPrepare = true;
            }
            else if (!base.IsPlaying)
            {
                base.Start();
                PlaybackStarted(this, EventArgs.Empty);
            }
        }
    }
}

