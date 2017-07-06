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
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Prism.Native;
using Prism.UI;

namespace Prism.Android
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeApplication"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeApplication), IsSingleton = true)]
    public class Application : INativeApplication
    {
        /// <summary>
        /// Occurs when the <see cref="P:MainActivity"/> property has changed.
        /// </summary>
        public static event EventHandler<ActivityChangedEventArgs> MainActivityChanged;

        /// <summary>
        /// Occurs when the application is shutting down.
        /// </summary>
        public event EventHandler Exiting;

        /// <summary>
        /// Occurs when the application is resuming from suspension.
        /// </summary>
        public event EventHandler Resuming;

        /// <summary>
        /// Occurs when the application is suspending.
        /// </summary>
        public event EventHandler Suspending;

        /// <summary>
        /// Occurs when an unhandled exception is encountered.
        /// </summary>
        public event EventHandler<ErrorEventArgs> UnhandledException;

        /// <summary>
        /// Gets or sets the main activity for the application.
        /// </summary>
        public static Activity MainActivity
        {
            get
            {
                if (mainActivity == null)
                {
                    throw new InvalidOperationException("Platform has not been properly initialized.  Use the AndroidInitializer class to initialize the platform.");
                }

                return mainActivity;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(MainActivity));
                }

                if (value != mainActivity)
                {
                    var oldValue = mainActivity;
                    mainActivity = value;
                    mainActivity.ActionBar?.Hide();

                    MainActivityChanged?.Invoke(null, new ActivityChangedEventArgs(mainActivity, oldValue));
                }
            }
        }
        private static Activity mainActivity;

        /// <summary>
        /// Gets the default theme that is used by the application.
        /// </summary>
        public Theme DefaultTheme { get; }

        /// <summary>
        /// Gets the platform on which the application is running.
        /// </summary>
        public Platform Platform
        {
            get { return Platform.Android; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        public Application()
        {
            AndroidEnvironment.UnhandledExceptionRaiser += (o, e) => UnhandledException(this, new ErrorEventArgs(e.Exception));
            MainActivity.Application.RegisterActivityLifecycleCallbacks(new ActivityLifecycleCallbacks(this));

            var textColor = Resources.GetColor(null, Resource.Attribute.TextColorPrimary);
            DefaultTheme = textColor.GetBrightness() > 0.5f ? Theme.Dark : Theme.Light;
        }

        /// <summary>
        /// Returns an <see cref="AssetFileDescriptor"/> for an Android asset.
        /// </summary>
        /// <param name="assetUri">The URI pointing to the asset.</param>
        /// <returns>An <see cref="AssetFileDescriptor"/> for the asset, or <c>null</c> if the URI does not point to an asset.</returns>
        public static AssetFileDescriptor GetAsset(Uri assetUri)
        {
            if (assetUri == null)
            {
                return null;
            }

            try
            {
                var assetName = assetUri.OriginalString;
                if (assetName.StartsWith(Prism.IO.Directory.AssetDirectory))
                {
                    assetName = assetName.Remove(0, Prism.IO.Directory.AssetDirectory.Length);
                }

                return MainActivity.Assets.OpenFd(assetName);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Signals the system to begin ignoring any user interactions within the application.
        /// </summary>
        public void BeginIgnoringUserInput()
        {
            if (MainActivity.Window.IsActive)
            {
                MainActivity.Window.SetFlags(WindowManagerFlags.NotTouchable, WindowManagerFlags.NotTouchable);
            }
        }

        /// <summary>
        /// Asynchronously invokes the specified delegate on the platform's main thread.
        /// </summary>
        /// <param name="action">The action to invoke on the main thread.</param>
        public void BeginInvokeOnMainThread(Action action)
        {
            MainActivity.RunOnUiThread(action);
        }

        /// <summary>
        /// Asynchronously invokes the specified delegate on the platform's main thread.
        /// </summary>
        /// <param name="del">A delegate to a method that takes multiple parameters.</param>
        /// <param name="parameters">The parameters for the delegate method.</param>
        public void BeginInvokeOnMainThreadWithParameters(Delegate del, params object[] parameters)
        {
            MainActivity.RunOnUiThread(() => del.DynamicInvoke(parameters));
        }

        /// <summary>
        /// Signals the system to stop ignoring user interactions within the application.
        /// </summary>
        public void EndIgnoringUserInput()
        {
            MainActivity.Window.ClearFlags(WindowManagerFlags.NotTouchable);
        }

        /// <summary>
        /// Launches the specified URL in an external application, most commonly a web browser.
        /// </summary>
        /// <param name="url">The URL to launch to.</param>
        public void LaunchUrl(Uri url)
        {
            var externalIntent = Intent.ParseUri(url.OriginalString, IntentUriType.Scheme);
            externalIntent.AddFlags(ActivityFlags.NewTask);
            if (MainActivity.PackageManager.QueryIntentActivities(externalIntent, PackageInfoFlags.MatchDefaultOnly).Count > 0)
            {
                MainActivity.StartActivity(externalIntent);
            }
        }

        private void OnApplicationResume()
        {
            foreach (var mediaPlayer in Media.MediaPlaybackItem.ActivePlayers)
            {
                mediaPlayer.Start();
            }

            Resuming(this, EventArgs.Empty);
        }

        private void OnApplicationShutdown()
        {
            var players = new global::Android.Media.MediaPlayer[Media.MediaPlaybackItem.ActivePlayers.Count];
            Media.MediaPlaybackItem.ActivePlayers.CopyTo(players, 0);
            foreach (var mediaPlayer in players)
            {
                mediaPlayer.Stop();
                mediaPlayer.Release();
            }

            Exiting(this, EventArgs.Empty);
        }

        private void OnApplicationSuspend()
        {
            foreach (var mediaPlayer in Media.MediaPlaybackItem.ActivePlayers)
            {
                mediaPlayer.Pause();
            }

            Suspending(this, EventArgs.Empty);
        }

        private class ActivityLifecycleCallbacks : Java.Lang.Object, global::Android.App.Application.IActivityLifecycleCallbacks
        {
            private readonly Application application;
            private bool isActivityStarted;

            public ActivityLifecycleCallbacks(Application app)
            {
                application = app;
            }

            public ActivityLifecycleCallbacks(IntPtr handle, JniHandleOwnership transfer)
                : base(handle, transfer)
            {
            }

            public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
            {
                if (Application.MainActivity == activity)
                {
                    isActivityStarted = false;
                }
            }

            public void OnActivityDestroyed(Activity activity)
            {
                if (Application.MainActivity == activity && !activity.IsChangingConfigurations)
                {
                    application?.OnApplicationShutdown();
                }
            }

            public void OnActivityPaused(Activity activity)
            {
                if (Application.MainActivity == activity && !activity.IsChangingConfigurations)
                {
                    application?.OnApplicationSuspend();
                }
            }

            public void OnActivityResumed(Activity activity)
            {
                if (isActivityStarted && Application.MainActivity == activity)
                {
                    application?.OnApplicationResume();
                }

                isActivityStarted = true;
            }

            public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
            {
            }

            public void OnActivityStarted(Activity activity)
            {
            }

            public void OnActivityStopped(Activity activity)
            {
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="E:Application.ActivityChanged"/> event.
    /// </summary>
    public class ActivityChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the new activity.
        /// </summary>
        public Activity NewActivity { get; }

        /// <summary>
        /// Gets the old activity.
        /// </summary>
        public Activity OldActivity { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityChangedEventArgs"/> class.
        /// </summary>
        /// <param name="newActivity">The new activity.</param>
        /// <param name="oldActivity">The old activity.</param>
        public ActivityChangedEventArgs(Activity newActivity, Activity oldActivity)
        {
            NewActivity = newActivity;
            OldActivity = oldActivity;
        }
    }
}

