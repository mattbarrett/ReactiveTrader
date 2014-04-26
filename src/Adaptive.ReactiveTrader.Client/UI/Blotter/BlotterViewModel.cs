﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Adaptive.ReactiveTrader.Client.Concurrency;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.Domain.Models.Execution;
using Adaptive.ReactiveTrader.Client.Domain.Repositories;
using Adaptive.ReactiveTrader.Shared.UI;
using log4net;
using PropertyChanged;

namespace Adaptive.ReactiveTrader.Client.UI.Blotter
{
    [ImplementPropertyChanged]
    public class BlotterViewModel : ViewModelBase, IBlotterViewModel
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly Func<ITrade, bool, ITradeViewModel> _tradeViewModelFactory;
        private readonly IConcurrencyService _concurrencyService;
        public ObservableCollection<ITradeViewModel> Trades { get; private set; }

		private static readonly ILog Log = LogManager.GetLogger();
        private bool _stale;
        private bool _stowReceived;

        public BlotterViewModel(IReactiveTrader reactiveTrader,
                                Func<ITrade, bool, ITradeViewModel> tradeViewModelFactory,
                                IConcurrencyService concurrencyService)
        {
            _tradeRepository = reactiveTrader.TradeRepository;
            _tradeViewModelFactory = tradeViewModelFactory;
            _concurrencyService = concurrencyService;
            Trades = new ObservableCollection<ITradeViewModel>();

            LoadTrades();
        }

        private void LoadTrades()
        {
            _tradeRepository.GetTradesStream()
                            .ObserveOn(_concurrencyService.Dispatcher)
                            .SubscribeOn(_concurrencyService.ThreadPool)
                            .Subscribe(
                                AddTrades,
                                ex => Log.Error("An error occurred within the trade stream", ex));
        }

        private void AddTrades(IEnumerable<ITrade> trades)
        {
            var allTrades = trades as IList<ITrade> ?? trades.ToList();
            if (!allTrades.Any())
            {
                // empty list of trades means we are disconnected
                _stale = true;
            }
            else
            {
                if (_stale)
                {
                    Trades.Clear();
                    _stale = false;
                }
            }

            allTrades.ForEach(trade =>
                {
                    var tradeViewModel = _tradeViewModelFactory(trade, !_stowReceived);
                    Trades.Add(tradeViewModel);
                });

            _stowReceived = true;
        }
    }
}
