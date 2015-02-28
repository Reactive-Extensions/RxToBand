using Microsoft.Band.Sensors;
using System;
using System.Reactive.Linq;

namespace RxToBandSample
{
    /// <summary>
    /// Exposes Band sensors as observable sequences.
    /// </summary>
    class BandSensors
    {
        private readonly IObservable<IBandAccelerometerReading> _accelerometer;
        private readonly IObservable<IBandContactReading> _contact;
        private readonly IObservable<IBandDistanceReading> _distance;
        private readonly IObservable<IBandGyroscopeReading> _gyroscope;
        private readonly IObservable<IBandHeartRateReading> _heartRate;
        private readonly IObservable<IBandPedometerReading> _pedometer;
        private readonly IObservable<IBandSkinTemperatureReading> _skintemperature;
        private readonly IObservable<IBandUltravioletLightReading> _ultraviolet;

        /// <summary>
        /// Creates a set of observable wrappers for Band sensors.
        /// </summary>
        /// <param name="sensorManager">The Band's sensor manager.</param>
        public BandSensors(IBandSensorManager sensorManager)
        {
            _accelerometer = sensorManager.Accelerometer.ToObservable();
            _contact = sensorManager.Contact.ToObservable().DistinctUntilChanged(c => c.State).Replay(1).RefCount();
            _distance = sensorManager.Distance.ToObservable();
            _gyroscope = sensorManager.Gyroscope.ToObservable();
            _heartRate = sensorManager.HeartRate.ToObservable();
            _pedometer = sensorManager.Pedometer.ToObservable();
            _skintemperature = sensorManager.SkinTemperature.ToObservable();
            _ultraviolet = sensorManager.Ultraviolet.ToObservable();
        }

        /// <summary>
        /// Gets an observable sequence for the accelerometer sensor of the Band.
        /// </summary>
        public IObservable<IBandAccelerometerReading> Accelerometer
        {
            get { return _accelerometer; }
        }

        /// <summary>
        /// Gets an observable sequence for the contact sensor of the Band.
        /// </summary>
        public IObservable<IBandContactReading> Contact
        {
            get { return _contact; }
        }

        /// <summary>
        /// Gets an observable sequence for the distance sensor of the Band.
        /// </summary>
        public IObservable<IBandDistanceReading> Distance
        {
            get { return _distance; }
        }

        /// <summary>
        /// Gets an observable sequence for the gyroscope sensor of the Band.
        /// </summary>
        public IObservable<IBandGyroscopeReading> Gyroscope
        {
            get { return _gyroscope; }
        }

        /// <summary>
        /// Gets an observable sequence for the heart rate sensor of the Band.
        /// </summary>
        public IObservable<IBandHeartRateReading> HeartRate
        {
            get { return _heartRate; }
        }

        /// <summary>
        /// Gets an observable sequence for the pedometer sensor of the Band.
        /// </summary>
        public IObservable<IBandPedometerReading> Pedometer
        {
            get { return _pedometer; }
        }

        /// <summary>
        /// Gets an observable sequence for the skin temperature sensor of the Band.
        /// </summary>
        public IObservable<IBandSkinTemperatureReading> SkinTemperature
        {
            get { return _skintemperature; }
        }

        /// <summary>
        /// Gets an observable sequence for the untraviolet sensor of the Band.
        /// </summary>
        public IObservable<IBandUltravioletLightReading> Ultraviolet
        {
            get { return _ultraviolet; }
        }
    }
}
