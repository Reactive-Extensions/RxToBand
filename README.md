# RxToBand
Reactive Extensions (Rx) support for the Microsoft Band

Adds a few extension methods for the IBandSensor<T> interface in the Microsoft Band SDK to enable writing reactive queries against the sensors. This is a quick preview of what's possible with Rx and the Band SDK; the APIs provided here are subject to change.

A simple example of a reactive query is shown below:

```csharp
var lockedHeartrate = from h in heartRate.OnlyWhenWorn(contact)
                      where h.Quality == HeartRateQuality.Locked
                      select h.HeartRate;
```

Thanks to the compositional nature of Rx, one can write elaborate sensor data analyses in very few lines of code. For example,  computing statistics of heart rate using Window and aggregation operators:

```csharp
var heartrateStats = (from w in lockedHeartrate.Window(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10))
                      let d = w.DefaultIfEmpty()
                      from s in Observable.CombineLatest(d.Average(), d.Min(), d.Max(),
                                                         (avg, min, max) => new
                                                         {
                                                             Average = avg,
                                                             Min = min,
                                                             Max = max
                                                         })
                      select s.ToString())
                     .StartWith("Hold on for a minute...");
```

Have fun!
