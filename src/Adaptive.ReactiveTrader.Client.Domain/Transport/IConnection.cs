﻿using System;
using System.Reactive;
using Microsoft.AspNet.SignalR.Client;

namespace Adaptive.ReactiveTrader.Client.Domain.Transport
{
    internal interface IConnection
    {
        IObservable<ConnectionInfo> StatusStream { get; }
		IObservable<object> Initialize();
        string Address { get; }
        IHubProxy ReferenceDataHubProxy { get; }
        IHubProxy PricingHubProxy { get; }
        IHubProxy ExecutionHubProxy { get; }
        IHubProxy BlotterHubProxy { get; }
    }
 }