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
using Android.Hardware;
using Android.OS;
using Prism.Native;
using Prism.Systems.Sensors;

namespace Prism.Android.Systems.Sensors
{
    /// <summary>
    /// Represents an Android implementation for an <see cref="INativeAccelerometer"/>.
    /// </summary>
    [Register(typeof(INativeAccelerometer), IsSingleton = true)]
    public class Accelerometer : Java.Lang.Object, INativeAccelerometer, ISensorEventListener
    {
        /// <summary>
        /// Occurs when the reading of the accelerometer has changed.
        /// </summary>
        public event EventHandler<AccelerometerReadingChangedEventArgs> ReadingChanged;

        /// <summary>
        /// Gets a value indicating whether an accelerometer is available for the current device.
        /// </summary>
        public bool IsAvailable
        {
            get { return manager != null && sensor != null; }
        }

        /// <summary>
        /// Gets or sets the amount of time, in milliseconds, that should pass between readings.
        /// </summary>
        public double UpdateInterval
        {
            get { return updateInterval; }
            set
            {
                updateInterval = value;

                manager.UnregisterListener(this, sensor);
                if (!double.IsNaN(updateInterval) && updateInterval != 0)
                {
                    manager.RegisterListener(this, sensor, (SensorDelay)(updateInterval * 1000));
                }
            }
        }
        private double updateInterval = double.NaN;

        private AccelerometerReading currentReading;
        private readonly SensorManager manager;
        private readonly Sensor sensor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Accelerometer"/> class.
        /// </summary>
        public Accelerometer()
        {
            manager = Application.MainActivity.GetSystemService(Context.SensorService) as SensorManager;
            sensor = manager?.GetDefaultSensor(SensorType.Accelerometer);
        }

        /// <summary>
        /// Gets the current reading of the accelerometer.
        /// </summary>
        /// <returns>The current reading of the accelerometer as an <see cref="AccelerometerReading"/> instance.</returns>
        public AccelerometerReading GetCurrentReading()
        {
            return currentReading;
        }

        void ISensorEventListener.OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        void ISensorEventListener.OnSensorChanged(SensorEvent e)
        {
            if (e.Sensor.Type == SensorType.Accelerometer)
            {
                currentReading = new AccelerometerReading(
                    SystemClock.UptimeMillis() - ((SystemClock.ElapsedRealtimeNanos() - e.Timestamp) / 1000000),
                    e.Values[0] / -SensorManager.GravityEarth, e.Values[1] / -SensorManager.GravityEarth,
                    e.Values[2] / -SensorManager.GravityEarth);

                ReadingChanged(this, new AccelerometerReadingChangedEventArgs(currentReading));
            }
        }
    }
}
