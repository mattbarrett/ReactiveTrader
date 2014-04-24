using System.Reactive.Concurrency;
using Adaptive.ReactiveTrader.Client.Concurrency;

namespace Adaptive.ReactiveTrader.Client.iOS.ConcurrencyService
{
    public class ConcurrencyService : IConcurrencyService
    {
        private readonly IScheduler _dispatcher;

        public ConcurrencyService()
        {
            _dispatcher = new iOSDispatcherScheduler();
        }

        public IScheduler Dispatcher
        {
            get { return _dispatcher; }
        }

        public IScheduler DispatcherPeriodic { get { return new PeriodicBatchScheduler(_dispatcher);} }
        public IScheduler ThreadPool { get { return TaskPoolScheduler.Default; } }
    }

    
}