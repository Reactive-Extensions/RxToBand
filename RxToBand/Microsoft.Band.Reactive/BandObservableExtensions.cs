using Microsoft.Band.Sensors;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Microsoft.Band.Reactive
{
    /// <summary>
    /// Provides a set of extension methods to enable using Reactive Extensions with the Microsoft Band SDK.
    /// </summary>
    public static class BandObservableExtensions
    {

        /// <summary>
        /// Convert an observable sequence of Band sensor readings into a timestamped sequence.
        /// </summary>
        /// <typeparam name="T">Type of the Band sensor readings exposed by the observable sequence.</typeparam>
        /// <param name="sensor">The Band sensor observable sequence to convert.</param>
        /// <returns>Observable sequence of timestamped Band sensor readings.</returns>
        public static IObservable<Timestamped<T>> ToTimestamped<T>(this IObservable<T> sensor)
            where T : IBandSensorReading
        {
            if (sensor == null)
            {
                throw new ArgumentNullException("sensor");
            }

            //
            // Just extract the timestamp and promote it to the Rx struct.
            //
            return sensor.Select(v => new Timestamped<T>(v, v.Timestamp));
        }

        /// <summary>
        /// Creates an observable sequence that only receives Band sensor readings when the Band is worn by the user.
        /// </summary>
        /// <typeparam name="T">Type of the Band sensor readings exposed by the observable sequence.</typeparam>
        /// <param name="sensor">The Band sensor observable sequence to receive readings from when the Band is worn by the user.</param>
        /// <param name="contact">The observable sequence for the contact sensor of the Band.</param>
        /// <returns>Observable sequence that only receives Band sensor readings when the Band is worn by the user.</returns>
        public static IObservable<T> OnlyWhenWorn<T>(this IObservable<T> sensor, IObservable<IBandContactReading> contact)
        {
            if (sensor == null)
            {
                throw new ArgumentNullException("sensor");
            }

            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            //
            // Switch between the specified sensor and the never sequence based on changes to contact state.
            //
            return contact.Select(c => c.State == BandContactState.Worn ? sensor : Observable.Never<T>()).Switch();
        }

    }
}
