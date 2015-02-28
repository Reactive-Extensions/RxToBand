using Microsoft.Band;
using Microsoft.Band.Reactive;
using Microsoft.Band.Sensors;
using System;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace RxToBandSample
{
    public sealed partial class MainPage : Page
    {
        private BandSensors _sensors;
        private IDisposable _simpleHeartRateSubscription;
        private IDisposable _heartRateStatsSubscription;
        private IDisposable _skinTemperatureSubscription;
        private IDisposable _stepGoalsSubscription;
        private IDisposable _averageSpeedSubscription;

        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //
                // Try to get the Band.
                //
                var pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands.Length < 1)
                {
                    txtConnect.Text = "This sample app requires a Microsoft Band paired to your phone. Also make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                    return;
                }

                var pairedBand = pairedBands[0];

                //
                // Connect to the Band and get the sensors.
                //
                var bandClient = await BandClientManager.Instance.ConnectAsync(pairedBand);
                _sensors = new BandSensors(bandClient.SensorManager);

                //
                // Tweak UI.
                //
                btnConnect.IsEnabled = false;
                btnSimpleHeart.IsEnabled = true;
                btnHeartStats.IsEnabled = true;
                btnSkinTemperature.IsEnabled = true;
                btnStepGoals.IsEnabled = true;
                btnAverageSpeed.IsEnabled = true;
            }
            catch (Exception ex)
            {
                txtConnect.Text = ex.ToString();
            }
        }

        private void SimpleHeartRate_Click(object sender, RoutedEventArgs e)
        {
            if (_simpleHeartRateSubscription != null)
            {
                _simpleHeartRateSubscription.Dispose();
                _simpleHeartRateSubscription = null;

                //
                // Change UI to allow retrying the example.
                //
                txtSimpleHeartRate.Text = "";
                btnSimpleHeart.Content = "Simple heart rate";
            }
            else
            {
                var heartRate = _sensors.HeartRate;
                var contact = _sensors.Contact;

                //
                // Locked heart rate when device is worn.
                //
                var lockedHeartrate = from h in heartRate.OnlyWhenWorn(contact)
                                      where h.Quality == HeartRateQuality.Locked
                                      select h.HeartRate;

                //
                // Subscribe to observable sequence and update the UI.
                //
                _simpleHeartRateSubscription = lockedHeartrate.ObserveOnDispatcher().Subscribe(h => txtSimpleHeartRate.Text = h.ToString() + " beats per minute");

                //
                // Change UI to allow stopping the readings.
                //
                btnSimpleHeart.Content = "Simple heart rate - Stop";
            }
        }

        private void HeartStats_Click(object sender, RoutedEventArgs e)
        {
            if (_heartRateStatsSubscription != null)
            {
                _heartRateStatsSubscription.Dispose();
                _heartRateStatsSubscription = null;

                //
                // Change UI to allow retrying the example.
                //
                txtHeartRateStats.Text = "";
                btnHeartStats.Content = "Heart rate stats";
            }
            else
            {
                var heartRate = _sensors.HeartRate;
                var contact = _sensors.Contact;

                //
                // Locked heart rate when device is worn.
                //
                var lockedHeartrate = from h in heartRate.OnlyWhenWorn(contact)
                                      where h.Quality == HeartRateQuality.Locked
                                      select h.HeartRate;

                //
                // Average, minimum, and maximum heart rate over 1 minute windows, every 10 seconds.
                //
                var heartrateStats = (from w in lockedHeartrate.Window(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10))
                                      let d = w.DefaultIfEmpty()
                                      from s in Observable.CombineLatest(d.Average(), d.Min(), d.Max(), (avg, min, max) => new { Average = avg, Min = min, Max = max })
                                      select s.ToString())
                                     .StartWith("Hold on for a minute...");

                //
                // Subscribe to observable sequence and update the UI.
                //
                _heartRateStatsSubscription = heartrateStats.ObserveOnDispatcher().Subscribe(s => txtHeartRateStats.Text = s);

                //
                // Change UI to allow stopping the readings.
                //
                btnHeartStats.Content = "Heart rate stats - Stop";
            }
        }

        private void SkinTemperature_Click(object sender, RoutedEventArgs e)
        {
            if (_skinTemperatureSubscription != null)
            {
                _skinTemperatureSubscription.Dispose();
                _skinTemperatureSubscription = null;

                //
                // Change UI to allow retrying the example.
                //
                txtSkinTemperature.Text = "";
                btnSkinTemperature.Content = "Skin temperature";
            }
            else
            {
                var skinTemperature = _sensors.SkinTemperature;
                var contact = _sensors.Contact;

                //
                // Skin temperature in Farenheit.
                //
                var skinInF = from t in skinTemperature.OnlyWhenWorn(contact)
                              let c = t.Temperature
                              select c * 9 / 5 + 32;

                //
                // Subscribe to observable sequence and update the UI.
                //
                _skinTemperatureSubscription = skinInF.ObserveOnDispatcher().Subscribe(s => txtSkinTemperature.Text = s + "F");

                //
                // Change UI to allow stopping the readings.
                //
                btnSkinTemperature.Content = "Skin temperature - Stop";
            }
        }

        private void StepGoals_Click(object sender, RoutedEventArgs e)
        {
            if (_stepGoalsSubscription != null)
            {
                _stepGoalsSubscription.Dispose();
                _stepGoalsSubscription = null;

                //
                // Change UI to allow retrying the example.
                //
                txtStepGoals.Text = "";
                btnStepGoals.Content = "Step goals";
            }
            else
            {
                var pedometer = _sensors.Pedometer;
                var contact = _sensors.Contact;

                //
                // Major step goals.
                //
                var majorSteps = from p in pedometer.OnlyWhenWorn(contact)
                                 where p.TotalSteps % 100 == 0
                                 select p.TotalSteps + " steps reached at " + p.Timestamp;

                //
                // Subscribe to observable sequence and update the UI.
                //
                _stepGoalsSubscription = majorSteps.ObserveOnDispatcher().Subscribe(s => txtStepGoals.Text = s + " steps taken");

                //
                // Change UI to allow stopping the readings.
                //
                btnStepGoals.Content = "Step goals - Stop";
            }
        }

        private void AverageSpeed_Click(object sender, RoutedEventArgs e)
        {
            if (_averageSpeedSubscription != null)
            {
                _averageSpeedSubscription.Dispose();
                _averageSpeedSubscription = null;

                //
                // Change UI to allow retrying the example.
                //
                txtAverageSpeed.Text = "";
                btnAverageSpeed.Content = "Average speed";
            }
            else
            {
                var distance = _sensors.Distance;
                var contact = _sensors.Contact;

                //
                // Hourly average speed in different modes of transport.
                //
                var averageWalkingSpeed = from w in distance.OnlyWhenWorn(contact).Window(TimeSpan.FromSeconds(5))
                                          from a in (from d in w
                                                     group d by d.CurrentMotion into g // TODO: while testing, I didn't get any values other than Idle here
                                                     from avg in g.DefaultIfEmpty().Average(d => d.Speed)
                                                     select new { Motion = g.Key, Average = avg })
                                                    .Aggregate("", (summary, a) => summary += a.ToString() + "\n")
                                          select a;

                //
                // Subscribe to observable sequence and update the UI.
                //
                _averageSpeedSubscription = averageWalkingSpeed.ObserveOnDispatcher().Subscribe(s => txtAverageSpeed.Text = s);

                //
                // Change UI to allow stopping the readings.
                //
                btnAverageSpeed.Content = "Average speed - Stop";
            }
        }

        /*
         * RANDOM THOUGHTS - Some queries that haven't been tested.
         *

        //
        // Warn about UV when running or jogging.
        //
        var uvWarning = Observable.CombineLatest(
                            from d in distance.OnlyWhenWorn(contact) select d.CurrentMotion == MotionType.Jogging || d.CurrentMotion == MotionType.Running,
                            from u in ultraviolet.OnlyWhenWorn(contact) select u.ExposureLevel == UltravioletExposureLevel.High || u.ExposureLevel == UltravioletExposureLevel.VeryHigh,
                            (active, exposed) => active && exposed)
                        .DistinctUntilChanged()
                        .Where(inDanger => inDanger)
                        .Select(_ => "Put on sun screen!");

        */
    }
}
