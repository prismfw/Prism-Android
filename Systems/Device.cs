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
using Android.Content;
using Android.Content.Res;
using Android.Hardware;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Prism.Native;
using Prism.Systems;

namespace Prism.Android.Systems
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeDevice"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeDevice), IsSingleton = true)]
    public class Device : OrientationEventListener, INativeDevice, ISensorEventListener
    {
        /// <summary>
        /// Occurs when the battery level of the device has changed by at least 1 percent.
        /// </summary>
        public event EventHandler BatteryLevelChanged;

        /// <summary>
        /// Occurs when the orientation of the device has changed.
        /// </summary>
        public event EventHandler OrientationChanged;

        /// <summary>
        /// Occurs when the power source of the device has changed.
        /// </summary>
        public event EventHandler PowerSourceChanged;

        /// <summary>
        /// Gets the battery level of the device as a percentage value between 0 (empty) and 100 (full).
        /// </summary>
        public int BatteryLevel { get; private set; }

        /// <summary>
        /// Gets the scaling factor of the display monitor.
        /// </summary>
        public double DisplayScale { get; private set; }

        /// <summary>
        /// Gets the form factor of the device on which the application is running.
        /// </summary>
        public FormFactor FormFactor
        {
            get
            {
                var sizeFlags = Application.MainActivity.Resources.Configuration.ScreenLayout & ScreenLayout.SizeMask;
                if (sizeFlags >= ScreenLayout.SizeLarge)
                    return FormFactor.Tablet;
                if (sizeFlags >= ScreenLayout.SizeSmall)
                    return FormFactor.Phone;
                return FormFactor.Unknown;
            }
        }

        /// <summary>
        /// Gets a unique identifier for the device.
        /// </summary>
        public string Id => Settings.Secure.GetString(Application.MainActivity.ContentResolver, Settings.Secure.AndroidId);

        /// <summary>
        /// Gets or sets a value indicating whether the orientation of the device should be monitored.
        /// This affects the ability to read the orientation of the device.
        /// </summary>
        public bool IsOrientationMonitoringEnabled
        {
            get { return isOrientationMonitoringEnabled; }
            set
            {
                if (value != isOrientationMonitoringEnabled)
                {
                    isOrientationMonitoringEnabled = value && CanDetectOrientation();
                    if (isOrientationMonitoringEnabled)
                    {
                        Enable();
                        sensorManager.RegisterListener(this, gravitySensor, SensorDelay.Normal);
                    }
                    else
                    {
                        Disable();
                        sensorManager.UnregisterListener(this, gravitySensor);
                    }
                }
            }
        }
        private bool isOrientationMonitoringEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether the power state of the device should be monitored.
        /// This affects the ability to read the power source and battery level of the device.
        /// </summary>
        public bool IsPowerMonitoringEnabled
        {
            get { return isPowerMonitoringEnabled; }
            set
            {
                if (value != isPowerMonitoringEnabled)
                {
                    isPowerMonitoringEnabled = value;
                    if (isPowerMonitoringEnabled)
                    {
                        var batteryFilter = new IntentFilter(Intent.ActionBatteryChanged);
                        var intent = global::Android.App.Application.Context.RegisterReceiver(batteryReceiver, batteryFilter);

                        BatteryLevel = intent.GetIntExtra(BatteryManager.ExtraLevel, -1);
                        SetPowerSource(intent.GetIntExtra(BatteryManager.ExtraPlugged, -1));
                    }
                    else
                    {
                        global::Android.App.Application.Context.UnregisterReceiver(batteryReceiver);
                    }
                }
            }
        }
        private bool isPowerMonitoringEnabled;

        /// <summary>
        /// Gets the model of the device.
        /// </summary>
        public string Model => Build.Model;

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        public string Name => Build.Model;

        /// <summary>
        /// Gets the operating system that is running on the device.
        /// </summary>
        public Prism.Systems.OperatingSystem OperatingSystem => Prism.Systems.OperatingSystem.Android;

        /// <summary>
        /// Gets the physical orientation of the device.
        /// </summary>
        public DeviceOrientation Orientation { get; private set; }

        /// <summary>
        /// Gets the version of the operating system that is running on the device.
        /// </summary>
        public Version OSVersion => new Version(Build.VERSION.Release);

        /// <summary>
        /// Gets the source from which the device is receiving its power.
        /// </summary>
        public PowerSource PowerSource { get; private set; }

        /// <summary>
        /// Gets the amount of time, in milliseconds, that the system has been awake since it was last restarted.
        /// </summary>
        public long SystemUptime => SystemClock.UptimeMillis();

        private readonly BatteryReceiver batteryReceiver;
        private readonly Sensor gravitySensor;
        private readonly SensorManager sensorManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class.
        /// </summary>
        public Device() : base(global::Android.App.Application.Context, SensorDelay.Normal)
        {
            batteryReceiver = new BatteryReceiver(this);
            sensorManager = (SensorManager)Application.MainActivity.GetSystemService(Context.SensorService);
            gravitySensor = sensorManager.GetDefaultSensor(SensorType.Gravity);

            Application.MainActivityChanged += (sender, e) => SetDisplayScale();
            SetDisplayScale();
        }
        
        /// <summary></summary>
        /// <param name="sensor"></param>
        /// <param name="status"></param>
        public void OnAccuracyChanged(Sensor sensor, SensorStatus status)
        {
        }

        /// <summary>
        /// Raises the orientation changed event.
        /// </summary>
        /// <param name="orientation">The new orientation of the device.</param>
        public override void OnOrientationChanged(int orientation)
        {
            var dOrientation = Orientation;
            if (orientation == OrientationUnknown)
            {
                Orientation = DeviceOrientation.Unknown;
            }
            else
            {
                var orient = Application.MainActivity.Resources.Configuration.Orientation;
                var rotation = Application.MainActivity.WindowManager.DefaultDisplay.Rotation;
                var naturalOrientation = (orient == global::Android.Content.Res.Orientation.Portrait &&
                    (rotation == SurfaceOrientation.Rotation90 || rotation == SurfaceOrientation.Rotation270)) ||
                    (orient == global::Android.Content.Res.Orientation.Landscape &&
                    (rotation == SurfaceOrientation.Rotation0 || rotation == SurfaceOrientation.Rotation180)) ?
                    DeviceOrientation.LandscapeLeft : DeviceOrientation.PortraitUp;
                
                if (orientation <= 45 || orientation >= 315)
                {
                    dOrientation = naturalOrientation;
                }
                else if (orientation >= 45 && orientation <= 135)
                {
                    dOrientation = (naturalOrientation == DeviceOrientation.PortraitUp) ?
                        DeviceOrientation.LandscapeRight : DeviceOrientation.PortraitUp;
                }
                else if (orientation >= 135 && orientation <= 225)
                {
                    dOrientation = (naturalOrientation == DeviceOrientation.PortraitUp) ?
                        DeviceOrientation.PortraitDown : DeviceOrientation.LandscapeRight;
                }
                else
                {
                    dOrientation = (naturalOrientation == DeviceOrientation.PortraitUp) ?
                        DeviceOrientation.LandscapeLeft : DeviceOrientation.PortraitDown;
                }
            }

            if (dOrientation != Orientation)
            {
                Orientation = dOrientation;
                OrientationChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        /// <summary></summary>
        /// <param name="evt"></param>
        public void OnSensorChanged(SensorEvent evt)
        {
            var source = evt.Sensor;
            if (source.Type == SensorType.Gravity &&
                (Orientation == DeviceOrientation.Unknown || Orientation == DeviceOrientation.FaceUp || Orientation == DeviceOrientation.FaceDown))
            {
                float z = evt.Values[2];
                float threshold = SensorManager.StandardGravity / 1.5f;
                
                var dOrientation = Orientation;
                if (z >= threshold)
                {
                    dOrientation = DeviceOrientation.FaceUp;
                }
                else if (z <= -threshold)
                {
                    dOrientation = DeviceOrientation.FaceDown;
                }
                
                if (dOrientation != Orientation)
                {
                    Orientation = dOrientation;
                    OrientationChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary></summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            global::Android.App.Application.Context.UnregisterReceiver(batteryReceiver);
        }

        private void OnBatteryLevelChanged()
        {
            BatteryLevelChanged(this, EventArgs.Empty);
        }

        private void SetDisplayScale()
        {
            var metrics = new DisplayMetrics();
            Application.MainActivity.WindowManager.DefaultDisplay.GetMetrics(metrics);
            DisplayScale = metrics.Density;
        }

        private void SetPowerSource(int value)
        {
            PowerSource source;
            switch (value)
            {
                case -1:
                    source = PowerSource.Unknown;
                    break;
                case 0:
                    source = PowerSource.Battery;
                    break;
                default:
                    source = PowerSource.External;
                    break;
            }

            if (source != PowerSource)
            {
                PowerSource = source;
                PowerSourceChanged(this, EventArgs.Empty);
            }
        }

        private class BatteryReceiver : BroadcastReceiver
        {
            private readonly Device device;

            public BatteryReceiver(Device device)
            {
                this.device = device;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                int level = intent.GetIntExtra(BatteryManager.ExtraLevel, -1);
                if (level != device.BatteryLevel)
                {
                    device.BatteryLevel = level;
                    device.OnBatteryLevelChanged();
                }

                device.SetPowerSource(intent.GetIntExtra(BatteryManager.ExtraPlugged, -1));
            }
        }
    }
}