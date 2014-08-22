using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Adaptive.ReactiveTrader.Client.Domain.ServiceClients;
using Adaptive.ReactiveTrader.Shared.DTO.Control;

namespace Adaptive.ReactiveTrader.Client.Domain.Repositories
{
    public class ControlRepository : IControlRepository
    {
        private readonly IControlServiceClient _controlServiceClient;

        public ControlRepository(IControlServiceClient controlServiceClient)
        {
            _controlServiceClient = controlServiceClient;
        }

        public IObservable<Unit> SetPriceFeedThroughput(double throughput)
        {
            var dto = new FeedThroughputDto
            {
                Throughput = throughput
            };

            return _controlServiceClient.SetPriceFeedThroughput(dto)
                .Select(_ => Unit.Default);
        }

        public IObservable<IEnumerable<CurrencyPairStateDto>> GetCurrencyPairStates()
        {
            return _controlServiceClient.GetCurrencyPairStates();
        }

        public IObservable<Unit> SetCurrencyPairState(string symbol, bool enabled, bool stale)
        {
            var dto = new CurrencyPairStateDto
            {
                Symbol = symbol,
                Enabled = enabled,
                Stale = stale
            };

            return _controlServiceClient.SetCurrencyPairState(dto)
                .Select(_ => Unit.Default);
        } 
    }
}