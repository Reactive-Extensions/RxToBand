using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Microsoft.Band.Sensors
{
    /// <summary>
    /// Provides a set of extension methods to enable using Reactive Extensions with the Microsoft Band SDK.
    /// </summary>
    public static class BandSensorExtensions
    {
        /// <summary>
        /// Converts a Band sensor to an observable sequence.
        /// </summary>
        /// <typeparam name="T">Type of the Band sensor readings exposed by the observable sequence.</typeparam>
        /// <param name="sensor">The Band sensor to obtain sensor readings from.</param>
        /// <param name="startAndStopSensorReadings">If set to <c>true</c>, the lifecycle of the resulting observable sequence manages starting and stopping the readings from the sensor; if set to <c>false</c>, the caller is responsible to manage the lifetime of sensor readings manually.</param>
        /// <returns>Observable sequence exposing the Band sensor readings.</returns>
        public static IObservable<T> ToObservable<T>(this IBandSensor<T> sensor, bool startAndStopSensorReadings = true)
            where T : IBandSensorReading
        {
            if (sensor == null)
            {
                throw new ArgumentNullException("sensor");
            }

            //
            // Task to track an outstanding stop operation, if any. This is used to ensure
            // sequential execution of start and stop requests.
            //
            var stopping = (Task)Task.FromResult(true);

            //
            // Observable wrapper around the ReadingChanged event for the Band sensor. It
            // differs from FromEvent behavior in that it can optionally deal with lifecycle
            // management to start and stop sensor readings.
            //
            var res = Observable.Create<T>(async (observer, ct) =>
            {
                EventHandler<BandSensorReadingEventArgs<T>> h = (o, e) =>
                {
                    observer.OnNext(e.SensorReading);
                };

                sensor.ReadingChanged += h;

                if (startAndStopSensorReadings)
                {
                    //
                    // If we have an outstanding stop request, await it to avoid races.
                    //
                    await stopping.ConfigureAwait(false);

                    //
                    // Do the proper start.
                    //
                    try
                    {
                        await sensor.StartReadingsAsync(ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken != ct)
                        {
                            observer.OnError(ex);
                        }

                        return Disposable.Empty;
                    }
                }

                return Disposable.Create(async () =>
                {
                    sensor.ReadingChanged -= h;

                    if (startAndStopSensorReadings)
                    {
                        //
                        // NOTE: Alternatively, we could just block on the task. Future releases
                        // of Rx may support IAsyncDisposable, which would provide a natural fit.
                        //

                        try
                        {
                            //
                            // If we have an outstanding stop request, await it to avoid races.
                            //
                            await stopping.ConfigureAwait(false);

                            //
                            // Initiate the stop operation and track it.
                            //
                            stopping = sensor.StopReadingsAsync();
                            await stopping.ConfigureAwait(false);
                        }
                        finally
                        {
                            //
                            // Unblock future awaits for the stop operation.
                            //
                            stopping = Task.FromResult(true);
                        }
                    }
                });
            });

            //
            // Try to avoid attaching multiple event handlers and excessive start and stop
            // requests to the underlying sensor. Even though it seems the Band SDK deals
            // with this at a lower level, this allows fan-out to multiple subscriptions
            // within the Rx layer.
            //
            res = res.Publish().RefCount();

            return res;
        }
    }
}
