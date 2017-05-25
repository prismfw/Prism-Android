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
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Prism.Android.Media;
using Prism.Input;
using Prism.Native;
using Prism.Systems;
using Prism.UI;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeMediaElement"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeMediaElement))]
    public class MediaElement : VideoView, INativeMediaElement, ISurfaceHolderCallback, MediaPlayer.IOnBufferingUpdateListener, MediaPlayer.IOnSeekCompleteListener, MediaController.IMediaPlayerControl
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when playback of a media source has finished.
        /// </summary>
        public event EventHandler MediaEnded;

        /// <summary>
        /// Occurs when a media source has failed to open.
        /// </summary>
        public event EventHandler<ErrorEventArgs> MediaFailed;

        /// <summary>
        /// Occurs when a media source has been successfully opened.
        /// </summary>
        public event EventHandler MediaOpened;

        /// <summary>
        /// Occurs when the system loses track of the pointer for some reason.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerCanceled;

        /// <summary>
        /// Occurs when the pointer has moved while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerMoved;

        /// <summary>
        /// Occurs when the pointer has been pressed while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerPressed;

        /// <summary>
        /// Occurs when the pointer has been released while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerReleased;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Occurs when a seek operation has been completed.
        /// </summary>
        public event EventHandler SeekCompleted;

        /// <summary>
        /// Occurs when this instance has been detached from the visual tree.
        /// </summary>
        public event EventHandler Unloaded;

        /// <summary>
        /// Gets or sets a value indicating whether animations are enabled for this instance.
        /// </summary>
        public bool AreAnimationsEnabled
        {
            get { return areAnimationsEnabled; }
            set
            {
                if (value != areAnimationsEnabled)
                {
                    areAnimationsEnabled = value;
                    OnPropertyChanged(Visual.AreAnimationsEnabledProperty);
                }
            }
        }
        private bool areAnimationsEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether to show the default playback controls (play, pause, etc).
        /// </summary>
        public bool ArePlaybackControlsEnabled
        {
            get { return Controller.Enabled; }
            set
            {
                if (value != Controller.Enabled)
                {
                    Controller.Enabled = value;
                    if (Controller.IsShowing && !Controller.Enabled)
                    {
                        Controller.Hide();
                    }

                    OnPropertyChanged(Prism.UI.Controls.MediaElement.ArePlaybackControlsEnabledProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests an arrangement of its children.
        /// </summary>
        public ArrangeRequestHandler ArrangeRequest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether playback of a media source should automatically begin once buffering is finished.
        /// </summary>
        public bool AutoPlay
        {
            get { return autoPlay; }
            set
            {
                if (value != autoPlay)
                {
                    autoPlay = value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.AutoPlayProperty);
                }
            }
        }
        private bool autoPlay;

        /// <summary>
        /// Gets the amount that the current playback item has buffered as a value between 0.0 and 1.0.
        /// </summary>
        public double BufferingProgress
        {
            get { return bufferingProgress; }
            private set
            {
                value = double.IsNaN(value) || double.IsInfinity(value) ? 0 : value;
                if (value != bufferingProgress)
                {
                    bufferingProgress = value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.BufferingProgressProperty);
                }
            }
        }
        private double bufferingProgress;

        /// <summary>
        /// Gets the duration of the current playback item.
        /// </summary>
        public new TimeSpan Duration
        {
            get { return duration; }
            private set
            {
                if (value != duration)
                {
                    duration = value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.DurationProperty);
                }
            }
        }
        private TimeSpan duration;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame
        {
            get
            {
                return new Rectangle(Left / Device.Current.DisplayScale, Top / Device.Current.DisplayScale,
                    Width / Device.Current.DisplayScale, Height / Device.Current.DisplayScale);
            }
            set
            {
                Left = (int)(value.Left * Device.Current.DisplayScale);
                Top = (int)(value.Top * Device.Current.DisplayScale);
                Right = (int)(value.Right * Device.Current.DisplayScale);
                Bottom = (int)(value.Bottom * Device.Current.DisplayScale);

                Measure(MeasureSpec.MakeMeasureSpec(Right - Left, MeasureSpecMode.Exactly),
                    MeasureSpec.MakeMeasureSpec(Bottom - Top, MeasureSpecMode.Exactly));
                Layout(Left, Top, Right, Bottom);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can be considered a valid result for hit testing.
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return isHitTestVisible; }
            set
            {
                if (value != isHitTestVisible)
                {
                    isHitTestVisible = value;
                    OnPropertyChanged(Visual.IsHitTestVisibleProperty);
                }
            }
        }
        private bool isHitTestVisible = true;

        /// <summary>
        /// Gets a value indicating whether this instance has been loaded and is ready for rendering.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current media source will automatically begin playback again once it has finished.
        /// </summary>
        public bool IsLooping
        {
            get { return isLooping; }
            set
            {
                if (value != isLooping)
                {
                    isLooping = value;
                    if (CurrentItem != null)
                    {
                        CurrentItem.Looping = isLooping;
                    }

                    OnPropertyChanged(Prism.UI.Controls.MediaElement.IsLoopingProperty);
                }
            }
        }
        private bool isLooping;

        /// <summary>
        /// Gets or sets a value indicating whether the media content is muted.
        /// </summary>
        public bool IsMuted
        {
            get { return isMuted; }
            set
            {
                if (value != isMuted)
                {
                    isMuted = value;
                    if (isMuted)
                    {
                        CurrentItem?.SetVolume(0, 0);
                    }
                    else
                    {
                        CurrentItem?.SetVolume((float)volume, (float)volume);
                    }

                    OnPropertyChanged(Prism.UI.Controls.MediaElement.IsMutedProperty);
                }
            }
        }
        private bool isMuted;

        /// <summary>
        /// Gets a value indicating whether a playback item is currently playing.
        /// </summary>
        public new bool IsPlaying
        {
            get { return isPlaying; }
            private set
            {
                if (value != isPlaying)
                {
                    isPlaying = value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.IsPlayingProperty);
                }
            }
        }
        private bool isPlaying;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets or sets the level of opacity for the element.
        /// </summary>
        public double Opacity
        {
            get { return Alpha; }
            set
            {
                if (value != Alpha)
                {
                    Alpha = (float)value;
                    OnPropertyChanged(Element.OpacityProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a coefficient of the rate at which media content is played back.  A value of 1.0 is a normal playback rate.
        /// </summary>
        public double PlaybackRate
        {
            get { return playbackRate; }
            set
            {
                if (value != playbackRate)
                {
                    playbackRate = value;
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                    {
                        if (isPlaying && CurrentItem != null)
                        {
                            CurrentItem.PlaybackParams = CurrentItem.PlaybackParams.SetSpeed((float)playbackRate);
                        }
                    }
                    else
                    {
                        Prism.Utilities.Logger.Warn("Setting MediaElement playback rate requires Android 6.0 (Marshmallow) or later.");
                    }

                    OnPropertyChanged(Prism.UI.Controls.MediaElement.PlaybackRateProperty);
                }
            }
        }
        private double playbackRate = 1;

        /// <summary>
        /// Gets or sets the position of the playback item.
        /// </summary>
        public TimeSpan Position
        {
            get { return TimeSpan.FromMilliseconds(CurrentItem?.CurrentPosition ?? 0); }
            set { CurrentItem?.SeekTo((int)value.TotalMilliseconds); }
        }

        /// <summary>
        /// Gets or sets transformation information that affects the rendering position of this instance.
        /// </summary>
        public INativeTransform RenderTransform
        {
            get { return renderTransform; }
            set
            {
                if (value != renderTransform)
                {
                    (renderTransform as Media.Transform)?.RemoveView(this);
                    renderTransform = value;

                    var transform = renderTransform as Media.Transform;
                    if (transform == null)
                    {
                        Animation = renderTransform as global::Android.Views.Animations.Animation;
                    }
                    else
                    {
                        transform.AddView(this);
                    }

                    OnPropertyChanged(Visual.RenderTransformProperty);
                }
            }
        }
        private INativeTransform renderTransform;

        /// <summary>
        /// Gets or sets the visual theme that should be used by this instance.
        /// </summary>
        public Theme RequestedTheme { get; set; }

        /// <summary>
        /// Gets or sets the source of the media content to be played.
        /// </summary>
        public object Source
        {
            get { return source; }
            set
            {
                if (value != source)
                {
                    var mpl = source as MediaPlaybackList;
                    if (mpl != null)
                    {
                        mpl.PlaylistEnded -= OnPlaylistEnded;
                        mpl.Deactivate();
                    }

                    source = value;
                    isMediaOpened = false;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.SourceProperty);

                    var player = source as MediaPlayer;
                    if (player != null)
                    {
                        SetCurrentItem(player);
                        if (autoPlay && IsLoaded)
                        {
                            StartPlayback();
                        }
                    }
                    else
                    {
                        var list = source as INativeMediaPlaybackList;
                        if (list != null)
                        {
                            list.CurrentItemChanged -= OnCurrentListItemChanged;
                            list.CurrentItemChanged += OnCurrentListItemChanged;

                            mpl = list as MediaPlaybackList;
                            if (mpl != null)
                            {
                                mpl.PlaylistEnded -= OnPlaylistEnded;
                                mpl.PlaylistEnded += OnPlaylistEnded;
                                mpl.Activate();
                            }
                        }
                    }
                }
            }
        }
        private object source;

        /// <summary>
        /// Gets or sets the manner in which video content is stretched within its allocated space.
        /// </summary>
        public Stretch Stretch
        {
            get { return stretch; }
            set
            {
                if (value != stretch)
                {
                    stretch = value;
                    if (isPlaying && CurrentItem != null)
                    {
                        CurrentItem.SetVideoScalingMode(stretch == Stretch.Fill ? VideoScalingMode.ScaleToFit : VideoScalingMode.ScaleToFitWithCropping);
                    }

                    OnPropertyChanged(Prism.UI.Controls.MediaElement.StretchProperty);
                }
            }
        }
        private Stretch stretch;

        /// <summary>
        /// Gets the size of the video content, or Size.Empty if there is no video content.
        /// </summary>
        public Size VideoSize
        {
            get { return videoSize; }
            private set
            {
                if (value.Width != videoSize.Width || value.Height != videoSize.Height)
                {
                    videoSize = value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.VideoSizeProperty);

                    Post(() =>
                    {
                        if (videoSize.Width == 0 && videoSize.Height == 0)
                        {
                            SetBackgroundColor(global::Android.Graphics.Color.Black);
                        }
                        else
                        {
                            SetBackgroundColor(global::Android.Graphics.Color.Transparent);
                        }
                    });
                }
            }
        }
        private Size videoSize;

        /// <summary>
        /// Gets or sets the display state of the element.
        /// </summary>
        public new Visibility Visibility
        {
            get { return base.Visibility.GetVisibility(); }
            set
            {
                var visibility = value.GetViewStates();
                if (visibility != base.Visibility)
                {
                    base.Visibility = visibility;
                    OnPropertyChanged(Element.VisibilityProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the volume of the media content as a range between 0.0 (silent) and 1.0 (full).
        /// </summary>
        public double Volume
        {
            get { return volume; }
            set
            {
                if (value != volume)
                {
                    volume = value;
                    if (!isMuted)
                    {
                        CurrentItem?.SetVolume((float)volume, (float)volume);
                    }

                    OnPropertyChanged(Prism.UI.Controls.MediaElement.VolumeProperty);
                }
            }
        }
        private double volume = 1;

        int MediaController.IMediaPlayerControl.CurrentPosition
        {
            get { return CurrentItem?.CurrentPosition ?? 0; }
        }

        int MediaController.IMediaPlayerControl.Duration
        {
            get { return CurrentItem?.Duration ?? 0; }
        }

        bool MediaController.IMediaPlayerControl.IsPlaying
        {
            get { return isPlaying; }
        }

        /// <summary>
        /// Gets the controller that contains the playback controls.
        /// </summary>
        protected MediaController Controller { get; }

        /// <summary>
        /// Gets the media item that is currently being used by the element.
        /// </summary>
        protected MediaPlayer CurrentItem { get; private set; }

        private bool isMediaOpened;
        private bool isSurfaceAvailable;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaElement"/> class.
        /// </summary>
        public MediaElement()
            : base(Application.MainActivity)
        {
            Controller = new MediaController(Context);
            Controller.SetAnchorView(this);
            Controller.SetMediaPlayer(this);
            SetMediaController(Controller);

            Holder.AddCallback(this);
        }

        /// <summary>
        /// Returns a value indicating whether the Pause button of the playback controls should be enabled.
        /// </summary>
        public override bool CanPause()
        {
            return true;
        }

        /// <summary>
        /// Returns a value indicating whether the Seek Forward button of the playback controls should be enabled.
        /// </summary>
        public override bool CanSeekForward()
        {
            return true;
        }

        /// <summary>
        /// Returns a value indicating whether the Seek Backward button of the playback controls should be enabled.
        /// </summary>
        public override bool CanSeekBackward()
        {
            return true;
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public override bool DispatchTouchEvent(MotionEvent e)
        {
            var parent = Parent as ITouchDispatcher;
            if (parent == null || parent.IsDispatching)
            {
                return base.DispatchTouchEvent(e);
            }

            return false;
        }

        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            RequestLayout();
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            RequestLayout();
        }

        /// <summary>
        /// Measures the element and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the element is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            int width = MeasuredWidth;
            int height = MeasuredHeight;

            var widthSpec = (int)Math.Min(int.MaxValue, constraints.Width * Device.Current.DisplayScale);
            var heightSpec = (int)Math.Min(int.MaxValue, constraints.Height * Device.Current.DisplayScale);
            base.OnMeasure(MeasureSpec.MakeMeasureSpec(widthSpec, MeasureSpecMode.AtMost),
                MeasureSpec.MakeMeasureSpec(heightSpec, MeasureSpecMode.AtMost));

            var size = new Size(MeasuredWidth, MeasuredHeight) / Device.Current.DisplayScale;
            SetMeasuredDimension(width, height);

            return new Size(Math.Min(constraints.Width, size.Width), Math.Min(constraints.Height, size.Height));
        }

        /// <summary></summary>
        /// <param name="mp"></param>
        /// <param name="percent"></param>
        public void OnBufferingUpdate(MediaPlayer mp, int percent)
        {
            if (mp == CurrentItem)
            {
                BufferingProgress = percent / 100d;
            }
        }

        /// <summary></summary>
        /// <param name="mp"></param>
        public void OnSeekComplete(MediaPlayer mp)
        {
            if (mp == CurrentItem)
            {
                SeekCompleted(this, EventArgs.Empty);
            }
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public override bool OnTouchEvent(MotionEvent e)
        {
            if (!isHitTestVisible)
            {
                return false;
            }

            if (e.Action == MotionEventActions.Cancel)
            {
                PointerCanceled(this, e.GetPointerEventArgs(this));
            }
            if (e.Action == MotionEventActions.Down)
            {
                if (Controller.Enabled)
                {
                    Controller.Show();
                }

                PointerPressed(this, e.GetPointerEventArgs(this));
            }
            if (e.Action == MotionEventActions.Move)
            {
                PointerMoved(this, e.GetPointerEventArgs(this));
            }
            if (e.Action == MotionEventActions.Up)
            {
                PointerReleased(this, e.GetPointerEventArgs(this));
            }
            return base.OnTouchEvent(e);
        }

        /// <summary>
        /// Pauses playback.
        /// </summary>
        public override void Pause()
        {
            CurrentItem?.Pause();
        }

        /// <summary>
        /// Pauses playback of the current media source.
        /// </summary>
        public void PausePlayback()
        {
            CurrentItem?.Pause();
        }

        /// <summary>
        /// Seeks to the specified time position.
        /// </summary>
        /// <param name="msec">The offset in milliseconds from the start to seek to.</param>
        public override void SeekTo(int msec)
        {
            CurrentItem?.SeekTo(msec);
        }

        /// <summary>
        /// Starts or resumes playback.
        /// </summary>
        public override void Start()
        {
            CurrentItem?.Start();
        }

        /// <summary>
        /// Starts or resumes playback of the current media source.
        /// </summary>
        public void StartPlayback()
        {
            CurrentItem?.Start();
        }

        /// <summary>
        /// Stops playback of the current media source.
        /// </summary>
        public new void StopPlayback()
        {
            CurrentItem?.Stop();
        }

        /// <summary></summary>
        /// <param name="holder"></param>
        /// <param name="format"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
        }

        /// <summary></summary>
        /// <param name="holder"></param>
        public void SurfaceCreated(ISurfaceHolder holder)
        {
            isSurfaceAvailable = true;
            CurrentItem?.SetDisplay(Holder);
        }

        /// <summary></summary>
        /// <param name="holder"></param>
        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            isSurfaceAvailable = false;
            CurrentItem?.SetDisplay(null);
        }

        /// <summary>
        /// This is called when the view is attached to a window.
        /// </summary>
        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            OnLoaded();

            if (autoPlay)
            {
                StartPlayback();
            }
        }

        /// <summary>
        /// This is called when the view is detached from a window.
        /// </summary>
        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            OnUnloaded();
        }

        /// <summary>
        /// Called from layout when this view should assign a size and position to each of its children.
        /// </summary>
        /// <param name="changed"></param>
        /// <param name="left">Left position, relative to parent.</param>
        /// <param name="top">Top position, relative to parent.</param>
        /// <param name="right">Right position, relative to parent.</param>
        /// <param name="bottom">Bottom position, relative to parent.</param>
        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            ArrangeRequest(false, null);
            base.OnLayout(changed, Left, Top, Right, Bottom);
        }

        /// <summary>
        /// Measure the view and its content to determine the measured width and the measured height.
        /// </summary>
        /// <param name="widthMeasureSpec">Horizontal space requirements as imposed by the parent.</param>
        /// <param name="heightMeasureSpec">Vertical space requirements as imposed by the parent.</param>
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            MeasureRequest(false, null);
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private void OnCurrentListItemChanged(object sender, NativeItemChangedEventArgs args)
        {
            SetCurrentItem(args.NewItem as MediaPlayer);
            if (!isPlaying && autoPlay && IsLoaded)
            {
                StartPlayback();
            }
        }

        private void OnItemStatusChanged(object sender, EventArgs args)
        {
            var item = sender as MediaPlaybackItem;
            if (item != null)
            {
                switch (item.Status)
                {
                    case MediaPlayerStatus.Error:
                        if (source == item)
                        {
                            MediaFailed(this, new ErrorEventArgs(new Exception(item.Error.ToString())));
                        }
                        break;
                    case MediaPlayerStatus.Finished:
                        if (source == item)
                        {
                            IsPlaying = false;
                            MediaEnded(this, EventArgs.Empty);
                        }
                        break;
                    case MediaPlayerStatus.Prepared:
                        Duration = item.Duration;
                        VideoSize = new Size(item.VideoWidth, item.VideoHeight);
                        if (!isMediaOpened)
                        {
                            isMediaOpened = true;
                            MediaOpened(this, EventArgs.Empty);
                        }
                        break;
                    case MediaPlayerStatus.Started:
                        item.SetVideoScalingMode(stretch == Stretch.Fill ? VideoScalingMode.ScaleToFit : VideoScalingMode.ScaleToFitWithCropping);
                        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                        {
                            item.PlaybackParams = item.PlaybackParams.SetSpeed((float)playbackRate);
                        }
                        IsPlaying = true;
                        break;
                    case MediaPlayerStatus.Paused:
                    case MediaPlayerStatus.Stopped:
                    System.Diagnostics.Debug.WriteLine(item.Uri.OriginalString);
                        IsPlaying = false;
                        break;
                }
            }
        }

        private void OnLoaded()
        {
            if (Controller.Enabled)
            {
                Controller.Show();
            }

            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);
            }
        }

        private void OnPlaylistEnded(object sender, EventArgs e)
        {
            IsPlaying = false;
            MediaEnded(this, EventArgs.Empty);
        }

        private void OnUnloaded()
        {
            Controller.Hide();
            CurrentItem?.Stop();

            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);
            }
        }

        private void SetCurrentItem(MediaPlayer player)
        {
            var oldItem = CurrentItem as MediaPlaybackItem;
            if (oldItem != null)
            {
                oldItem.StatusChanged -= OnItemStatusChanged;
            }

            if (CurrentItem == player && player != null)
            {
                player.Stop();
                player.Prepare();
            }

            var newItem = player as MediaPlaybackItem;
            if (newItem != null)
            {
                newItem.StatusChanged -= OnItemStatusChanged;
                newItem.StatusChanged += OnItemStatusChanged;
            }

            if (CurrentItem == player)
            {
                return;
            }

            if (CurrentItem != null)
            {
                CurrentItem.SetOnBufferingUpdateListener(null);
                CurrentItem.SetOnSeekCompleteListener(null);
                CurrentItem.SetDisplay(null);
            }

            CurrentItem = player;
            if (player != null)
            {
                player.Looping = isLooping;
                player.SetDisplay(isSurfaceAvailable ? Holder : null);
                player.SetOnBufferingUpdateListener(this);
                player.SetOnSeekCompleteListener(this);

                if (isMuted)
                {
                    player.SetVolume(0, 0);
                }
                else
                {
                    player.SetVolume((float)volume, (float)volume);
                }

                player.PrepareAsync();
            }

            
            if (newItem != null)
            {
                if (newItem.Status > MediaPlayerStatus.Preparing)
                {
                    Duration = newItem.Duration;
                    VideoSize = new Size(newItem.VideoWidth, newItem.VideoHeight);
                    if (!isMediaOpened)
                    {
                        isMediaOpened = true;
                        MediaOpened(this, EventArgs.Empty);
                    }
                }
            }
        }
    }
}

